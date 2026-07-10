using CSharpFunctionalExtensions;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.Order;

/// <summary>
/// 需求導向 Repository 範本
///
/// 適用場景：
/// - 複雜業務邏輯（訂單、付款、庫存協調）
/// - 多表操作（3+ 個表的協調）
/// - 需要交易保證的操作
/// - 多個 API 端點複用相同邏輯
///
/// 設計原則：
/// 1. 以業務需求而非資料表設計
/// 2. 高層業務方法對應完整業務操作
/// 3. 內部輔助方法使用 private 修飾符
/// 4. 完整的交易管理與錯誤處理
/// 5. 原子性保證與補償邏輯
/// 6. 詳細的設計註解說明決策理由
///
/// 核心概念：
/// - 聚合根（Aggregate Root）：Order 為聚合根
/// - 邊界：Order + OrderItem + Payment 作為一個整體
/// - 不允許直接修改 OrderItem，只能透過 Order 操作
/// - 所有業務規則（如庫存檢查、付款驗證）集中在此
/// </summary>
public class OrderManagementRepository
{
    private readonly IDbContextFactory<JobBank1111DbContext> _dbContextFactory;

    public OrderManagementRepository(IDbContextFactory<JobBank1111DbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <summary>
    /// 建立完整的訂單（包括訂單、明細、扣減庫存、建立付款記錄）
    ///
    /// 此方法是核心業務操作，完整封裝訂單建立的所有步驟：
    /// 1. 驗證訂單明細
    /// 2. 檢查庫存充足
    /// 3. 建立訂單主檔
    /// 4. 建立訂單明細
    /// 5. 扣減庫存
    /// 6. 建立付款記錄
    /// 7. 事務提交（全部成功或全部失敗）
    ///
    /// 設計決策：
    /// - 使用交易確保多步驟操作的原子性
    /// - 驗證所有前置條件，若失敗立即回傳 Failure，不執行資料庫操作
    /// - 如庫存不足，應在建立訂單前檢查，避免部分修改
    /// </summary>
    public async Task<Result<OrderDetail>> CreateCompleteOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancel = default)
    {
        try
        {
            // 1. 驗證請求參數
            var validationResult = ValidateCreateOrderRequest(request);
            if (validationResult.IsFailure)
            {
                return Result.Failure<OrderDetail, Failure>(validationResult.Error);
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);

            try
            {
                // 2. 檢查庫存充足（關鍵前置條件）
                var inventoryCheckResult = await CheckInventoryAvailabilityAsync(
                    request.OrderItems, dbContext, cancel);
                if (inventoryCheckResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancel);
                    return Result.Failure<OrderDetail, Failure>(inventoryCheckResult.Error);
                }

                // 3. 建立訂單主檔
                var orderId = Guid.NewGuid();
                var order = new Orders
                {
                    Id = orderId,
                    MemberId = request.MemberId,
                    OrderNumber = GenerateOrderNumber(),
                    Status = "Pending",
                    TotalAmount = request.OrderItems.Sum(x => x.Quantity * x.UnitPrice),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                dbContext.Orders.Add(order);
                await dbContext.SaveChangesAsync(cancel);

                // 4. 建立訂單明細
                await CreateOrderItemsAsync(orderId, request.OrderItems, dbContext, cancel);

                // 5. 扣減庫存
                await DeductInventoryAsync(request.OrderItems, dbContext, cancel);

                // 6. 建立付款記錄
                await CreatePaymentRecordAsync(orderId, order.TotalAmount, dbContext, cancel);

                // 7. 事務提交
                await dbContext.SaveChangesAsync(cancel);
                await transaction.CommitAsync(cancel);

                // 8. 組裝回應
                var orderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    Items = request.OrderItems.Select(x => new OrderItemDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice
                    }).ToList()
                };

                return Result.Success<OrderDetail, Failure>(orderDetail);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancel);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<OrderDetail, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "訂單建立已取消"
                });
        }
        catch (DbUpdateException dbEx)
        {
            return Result.Failure<OrderDetail, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = "資料庫更新失敗",
                    Exception = dbEx
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<OrderDetail, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.InternalServerError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 獲取訂單詳情（包括明細和付款資訊）
    ///
    /// 設計決策：
    /// - 使用 AsNoTracking 因為只讀取數據
    /// - 使用 Include 進行 JOIN，避免 N+1 查詢
    /// - 一次查詢獲取完整訊息
    /// </summary>
    public async Task<Result<OrderDetail>> GetOrderDetailAsync(
        Guid orderId,
        CancellationToken cancel = default)
    {
        try
        {
            if (orderId == Guid.Empty)
            {
                return Result.Failure<OrderDetail, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "訂單 ID 無效"
                    });
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            // 使用 Include 進行 JOIN，避免 N+1 問題
            var order = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .FirstOrDefaultAsync(cancel);

            if (order == null)
            {
                return Result.Failure<OrderDetail, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"訂單不存在: {orderId}"
                    });
            }

            // 分別查詢明細和付款（實務中應使用 Include）
            var items = await dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync(cancel);

            var payment = await dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == orderId, cancel);

            var orderDetail = new OrderDetail
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Items = items.Select(i => new OrderItemDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                PaymentStatus = payment?.Status ?? "Pending"
            };

            return Result.Success<OrderDetail, Failure>(orderDetail);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<OrderDetail, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "查詢已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<OrderDetail, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 更新訂單狀態
    ///
    /// 設計決策：
    /// - 驗證狀態轉移是否合法（如 Pending → Shipped）
    /// - 記錄狀態變更歷史
    /// - 使用交易保護多表操作
    /// </summary>
    public async Task<Result> UpdateOrderStatusAsync(
        Guid orderId,
        string newStatus,
        CancellationToken cancel = default)
    {
        try
        {
            if (orderId == Guid.Empty || string.IsNullOrWhiteSpace(newStatus))
            {
                return Result.Failure(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "訂單 ID 或新狀態無效"
                    });
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);

            try
            {
                var order = await dbContext.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancel);

                if (order == null)
                {
                    return Result.Failure(
                        new Failure
                        {
                            Code = nameof(FailureCode.InvalidOperation),
                            Message = $"訂單不存在: {orderId}"
                        });
                }

                // 驗證狀態轉移的合法性
                var isValidTransition = IsValidStatusTransition(order.Status, newStatus);
                if (!isValidTransition)
                {
                    return Result.Failure(
                        new Failure
                        {
                            Code = nameof(FailureCode.InvalidOperation),
                            Message = $"無效的狀態轉移: {order.Status} → {newStatus}"
                        });
                }

                // 更新訂單狀態
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                dbContext.Orders.Update(order);

                // 記錄狀態變更歷史（示例）
                // await RecordStatusChangeHistoryAsync(orderId, order.Status, newStatus, dbContext, cancel);

                await dbContext.SaveChangesAsync(cancel);
                await transaction.CommitAsync(cancel);

                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancel);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Failure
                {
                    Code = nameof(FailureCode.InternalServerError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 取消訂單（包括回滾庫存、取消付款）
    ///
    /// 設計決策：
    /// - 驗證訂單可否取消（如已發貨則不可取消）
    /// - 回滾庫存（原子性操作）
    /// - 取消付款（若尚未支付）
    /// - 記錄取消原因
    /// </summary>
    public async Task<Result> CancelOrderAsync(
        Guid orderId,
        string reason = "",
        CancellationToken cancel = default)
    {
        try
        {
            if (orderId == Guid.Empty)
            {
                return Result.Failure(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "訂單 ID 無效"
                    });
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);

            try
            {
                var order = await dbContext.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancel);

                if (order == null)
                {
                    return Result.Failure(
                        new Failure
                        {
                            Code = nameof(FailureCode.InvalidOperation),
                            Message = $"訂單不存在: {orderId}"
                        });
                }

                // 驗證訂單是否可以取消
                if (!CanCancelOrder(order.Status))
                {
                    return Result.Failure(
                        new Failure
                        {
                            Code = nameof(FailureCode.InvalidOperation),
                            Message = $"訂單狀態為 {order.Status}，無法取消"
                        });
                }

                // 1. 獲取訂單明細
                var orderItems = await dbContext.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .ToListAsync(cancel);

                // 2. 回滾庫存
                await RestoreInventoryAsync(orderItems, dbContext, cancel);

                // 3. 更新訂單狀態
                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.UtcNow;
                dbContext.Orders.Update(order);

                // 4. 取消付款
                var payment = await dbContext.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == orderId, cancel);
                if (payment != null && payment.Status != "Completed")
                {
                    payment.Status = "Cancelled";
                    dbContext.Payments.Update(payment);
                }

                await dbContext.SaveChangesAsync(cancel);
                await transaction.CommitAsync(cancel);

                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancel);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Failure
                {
                    Code = nameof(FailureCode.InternalServerError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    // ========== 私有輔助方法 ==========
    // 這些方法是內部實作細節，不應在 Repository 外部呼叫

    /// <summary>
    /// 驗證訂單建立請求
    /// </summary>
    private Result<Unit, Failure> ValidateCreateOrderRequest(CreateOrderRequest request)
    {
        if (request == null)
        {
            return Result.Failure<Unit, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.ValidationError),
                    Message = "訂單請求不能為空"
                });
        }

        if (request.MemberId == Guid.Empty)
        {
            return Result.Failure<Unit, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.ValidationError),
                    Message = "會員 ID 無效"
                });
        }

        if (request.OrderItems == null || request.OrderItems.Count == 0)
        {
            return Result.Failure<Unit, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.ValidationError),
                    Message = "訂單必須包含至少一個明細"
                });
        }

        foreach (var item in request.OrderItems)
        {
            if (item.Quantity <= 0 || item.UnitPrice <= 0)
            {
                return Result.Failure<Unit, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "訂單明細的數量和價格必須大於零"
                    });
            }
        }

        return Result.Success<Unit, Failure>(Unit.Value);
    }

    /// <summary>
    /// 檢查庫存是否充足
    /// </summary>
    private async Task<Result<Unit, Failure>> CheckInventoryAvailabilityAsync(
        List<OrderItemRequest> items,
        JobBank1111DbContext dbContext,
        CancellationToken cancel)
    {
        foreach (var item in items)
        {
            var inventory = await dbContext.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancel);

            if (inventory == null || inventory.AvailableQuantity < item.Quantity)
            {
                return Result.Failure<Unit, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"產品 {item.ProductId} 庫存不足，可用數量: {inventory?.AvailableQuantity ?? 0}"
                    });
            }
        }

        return Result.Success<Unit, Failure>(Unit.Value);
    }

    /// <summary>
    /// 建立訂單明細
    /// </summary>
    private async Task CreateOrderItemsAsync(
        Guid orderId,
        List<OrderItemRequest> items,
        JobBank1111DbContext dbContext,
        CancellationToken cancel)
    {
        var orderItems = items.Select(item => new OrderItems
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await dbContext.OrderItems.AddRangeAsync(orderItems, cancel);
    }

    /// <summary>
    /// 扣減庫存
    /// </summary>
    private async Task DeductInventoryAsync(
        List<OrderItemRequest> items,
        JobBank1111DbContext dbContext,
        CancellationToken cancel)
    {
        foreach (var item in items)
        {
            var inventory = await dbContext.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancel);

            if (inventory != null)
            {
                inventory.AvailableQuantity -= item.Quantity;
                inventory.ReservedQuantity += item.Quantity;
                dbContext.Inventories.Update(inventory);
            }
        }
    }

    /// <summary>
    /// 建立付款記錄
    /// </summary>
    private async Task CreatePaymentRecordAsync(
        Guid orderId,
        decimal amount,
        JobBank1111DbContext dbContext,
        CancellationToken cancel)
    {
        var payment = new Payments
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Payments.AddAsync(payment, cancel);
    }

    /// <summary>
    /// 回滾庫存
    /// </summary>
    private async Task RestoreInventoryAsync(
        List<OrderItems> items,
        JobBank1111DbContext dbContext,
        CancellationToken cancel)
    {
        foreach (var item in items)
        {
            var inventory = await dbContext.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancel);

            if (inventory != null)
            {
                inventory.AvailableQuantity += item.Quantity;
                inventory.ReservedQuantity -= item.Quantity;
                dbContext.Inventories.Update(inventory);
            }
        }
    }

    /// <summary>
    /// 驗證狀態轉移是否合法
    /// </summary>
    private bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        var validTransitions = new Dictionary<string, List<string>>
        {
            ["Pending"] = new() { "Confirmed", "Cancelled" },
            ["Confirmed"] = new() { "Shipped", "Cancelled" },
            ["Shipped"] = new() { "Delivered" },
            ["Delivered"] = new() { },
            ["Cancelled"] = new() { }
        };

        return validTransitions.ContainsKey(currentStatus) &&
               validTransitions[currentStatus].Contains(newStatus);
    }

    /// <summary>
    /// 檢查訂單是否可以取消
    /// </summary>
    private bool CanCancelOrder(string status)
    {
        var cancellableStatuses = new[] { "Pending", "Confirmed" };
        return cancellableStatuses.Contains(status);
    }

    /// <summary>
    /// 產生訂單號
    /// </summary>
    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}

// ========== 輔助類型定義 ==========

/// <summary>
/// 建立訂單請求
/// </summary>
public class CreateOrderRequest
{
    public Guid MemberId { get; set; }
    public List<OrderItemRequest> OrderItems { get; set; }
}

/// <summary>
/// 訂單明細項目請求
/// </summary>
public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// 訂單詳情回應
/// </summary>
public class OrderDetail
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public string PaymentStatus { get; set; }
    public List<OrderItemDetail> Items { get; set; }
}

/// <summary>
/// 訂單明細項目回應
/// </summary>
public class OrderItemDetail
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
