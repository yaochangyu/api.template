using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Domain} Repository - 需求導向
/// </summary>
/// <remarks>
/// 適用場景：
/// - 複雜業務邏輯
/// - 跨多個資料表操作
/// - 需要交易一致性
/// - 封裝完整業務流程
/// </remarks>
public class {Domain}Repository(
    IDbContextFactory<JobBankDbContext> dbContextFactory,
    IContextGetter<TraceContext> traceContextGetter)
{
    /// <summary>
    /// 建立完整訂單（訂單主檔 + 明細 + 付款 + 扣庫存）
    /// </summary>
    public async Task<Result<OrderDetail, Failure>> CreateCompleteOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. 建立訂單主檔
            var order = new Order
            {
                Id = Guid.NewGuid(),
                MemberId = traceContext.UserId!.Value,
                OrderDate = DateTime.UtcNow,
                TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
                Status = OrderStatus.Pending
            };

            dbContext.Orders.Add(order);

            // 2. 建立訂單明細
            var orderItems = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList();

            dbContext.OrderItems.AddRange(orderItems);

            // 3. 建立付款記錄
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Pending
            };

            dbContext.Payments.Add(payment);

            // 4. 更新庫存
            foreach (var item in request.Items)
            {
                var product = await dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                if (product == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<OrderDetail, Failure>(new Failure
                    {
                        Code = nameof(FailureCode.NotFound),
                        Message = $"產品不存在：{item.ProductId}",
                        TraceId = traceContext.TraceId
                    });
                }

                if (product.Stock < item.Quantity)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<OrderDetail, Failure>(new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"庫存不足，產品：{product.Name}，可用庫存：{product.Stock}",
                        TraceId = traceContext.TraceId
                    });
                }

                product.Stock -= item.Quantity;
                dbContext.Products.Update(product);
            }

            // 5. 儲存所有變更
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 6. 回傳完整訂單資訊
            var orderDetail = new OrderDetail
            {
                Order = order,
                Items = orderItems,
                Payment = payment
            };

            return Result.Success<OrderDetail, Failure>(orderDetail);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<OrderDetail, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "訂單建立失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 查詢訂單詳細資訊（訂單 + 明細 + 付款）
    /// </summary>
    public async Task<Result<OrderDetail, Failure>> GetOrderDetailAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var orderDetail = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Select(o => new OrderDetail
                {
                    Order = o,
                    Items = o.OrderItems.ToList(),
                    Payment = o.Payment
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (orderDetail == null)
            {
                return Result.Failure<OrderDetail, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "訂單不存在",
                    TraceId = traceContext.TraceId
                });
            }

            return Result.Success<OrderDetail, Failure>(orderDetail);
        }
        catch (Exception ex)
        {
            return Result.Failure<OrderDetail, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "查詢訂單失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 取消訂單（更新訂單狀態 + 退回庫存 + 取消付款）
    /// </summary>
    public async Task<Result<bool, Failure>> CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. 查詢訂單
            var order = await dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<bool, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "訂單不存在",
                    TraceId = traceContext.TraceId
                });
            }

            // 2. 檢查訂單狀態
            if (order.Status == OrderStatus.Cancelled)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<bool, Failure>(new Failure
                {
                    Code = nameof(FailureCode.InvalidOperation),
                    Message = "訂單已取消",
                    TraceId = traceContext.TraceId
                });
            }

            // 3. 更新訂單狀態
            order.Status = OrderStatus.Cancelled;
            dbContext.Orders.Update(order);

            // 4. 退回庫存
            foreach (var item in order.OrderItems)
            {
                var product = await dbContext.Products.FindAsync(
                    new object[] { item.ProductId }, cancellationToken);

                if (product != null)
                {
                    product.Stock += item.Quantity;
                    dbContext.Products.Update(product);
                }
            }

            // 5. 取消付款
            if (order.Payment != null)
            {
                order.Payment.Status = PaymentStatus.Cancelled;
                dbContext.Payments.Update(order.Payment);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success<bool, Failure>(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "取消訂單失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }
}

/* 使用說明
 *
 * 1. 替換佔位符：
 *    {Domain} = 業務領域名稱（例如：OrderManagement）
 *    {Feature} = 功能名稱（例如：Order）
 *
 * 2. 核心原則：
 *    ✅ 封裝完整業務邏輯
 *    ✅ 跨表操作使用交易
 *    ✅ 一次查詢取得完整資料（使用 Include 或 Select）
 *    ✅ 業務驗證在 Repository 內處理
 *    ✅ 失敗時回滾交易
 *
 * 3. 何時使用需求導向：
 *    - 涉及 3 個以上資料表
 *    - 需要交易一致性
 *    - 複雜業務邏輯
 *    - 多個 API 端點共用
 */
