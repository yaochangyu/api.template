// ============================================================================
// DbContextFactory 完整實作範本
// 適用於 ASP.NET Core 8.0 + EF Core 8.0
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JobBank1111.Infrastructure.Data;

/// <summary>
/// DbContextFactory 模式演示
/// 用於管理 DbContext 生命週期，避免長期持有導致的資源洩漏
/// </summary>
public static class DbContextFactoryConfiguration
{
    /// <summary>
    /// 在 Program.cs 中配置 DbContextFactory
    /// </summary>
    public static void ConfigureDbContextFactory(this IServiceCollection services, string connectionString)
    {
        // ✅ 使用 AddDbContextFactory 而非 AddDbContext
        services.AddDbContextFactory<JobBankDbContext>((serviceProvider, options) =>
        {
            options
                .UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelaySeconds: 10,
                            errorNumbersToAdd: null
                        );
                        sqlOptions.CommandTimeout(30);
                    }
                )
                .EnableSensitiveDataLogging(isDevelopment: false)
                .LogTo(
                    serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("EF"),
                    LogLevel.Warning // 生產環境只記錄警告以上
                );
        });

        // 選擇性：註冊 DbContext 供舊式直接注入（不推薦）
        // services.AddScoped<JobBankDbContext>();
    }
}

// ============================================================================
// 範例 1：標準 Repository 使用方式
// ============================================================================

/// <summary>
/// 標準的 Repository 實作，展示 DbContextFactory 的正確用法
/// </summary>
public class MemberRepository
{
    private readonly IDbContextFactory<JobBankDbContext> _dbContextFactory;
    private readonly ILogger<MemberRepository> _logger;

    public MemberRepository(
        IDbContextFactory<JobBankDbContext> dbContextFactory,
        ILogger<MemberRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// 查詢會員（唯讀）
    /// </summary>
    public async Task<Result<MemberDTO>> GetMemberByIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var member = await dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

            if (member == null)
            {
                return Result.Failure<MemberDTO, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"會員 {memberId} 不存在"
                    }
                );
            }

            var dto = new MemberDTO { Id = member.Id, Name = member.Name };
            return Result.Success<MemberDTO, Failure>(dto);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("查詢會員被取消");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢會員時發生錯誤");
            return Result.Failure<MemberDTO, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                }
            );
        }
    }

    /// <summary>
    /// 建立會員（寫入）
    /// </summary>
    public async Task<Result<MemberDTO>> CreateMemberAsync(
        CreateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            // 檢查重複
            var exists = await dbContext.Members
                .AnyAsync(m => m.Email == request.Email, cancellationToken);

            if (exists)
            {
                return Result.Failure<MemberDTO, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.DuplicateEmail),
                        Message = $"郵件地址 {request.Email} 已存在"
                    }
                );
            }

            var member = new Member
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Members.Add(member);
            await dbContext.SaveChangesAsync(cancellationToken);

            var dto = new MemberDTO { Id = member.Id, Name = member.Name };
            return Result.Success<MemberDTO, Failure>(dto);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        {
            _logger.LogWarning("郵件地址重複：{Email}", request.Email);
            return Result.Failure<MemberDTO, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DuplicateEmail),
                    Message = "郵件地址已被使用",
                    Exception = ex
                }
            );
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "資料庫更新錯誤");
            return Result.Failure<MemberDTO, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = "資料庫更新失敗",
                    Exception = ex
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立會員時發生未預期的錯誤");
            return Result.Failure<MemberDTO, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.InternalServerError),
                    Message = "系統內部錯誤",
                    Exception = ex
                }
            );
        }
    }

    /// <summary>
    /// 批次更新會員（展示交易和批次操作）
    /// </summary>
    public async Task<Result<int>> BulkUpdateMembersAsync(
        List<UpdateMemberRequest> requests,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var memberIds = requests.Select(r => r.Id).ToList();
                var members = await dbContext.Members
                    .Where(m => memberIds.Contains(m.Id))
                    .ToListAsync(cancellationToken);

                foreach (var request in requests)
                {
                    var member = members.FirstOrDefault(m => m.Id == request.Id);
                    if (member != null)
                    {
                        member.Name = request.Name;
                        member.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var count = await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("批次更新會員完成，受影響筆數：{Count}", count);
                return Result.Success<int, Failure>(count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批次更新會員時發生錯誤");
            return Result.Failure<int, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                }
            );
        }
    }
}

// ============================================================================
// 範例 2：背景服務使用方式
// ============================================================================

/// <summary>
/// 背景服務示例，使用 DbContextFactory 進行長時間運行的操作
/// </summary>
public class BackgroundMemberSyncService : BackgroundService
{
    private readonly IDbContextFactory<JobBankDbContext> _dbContextFactory;
    private readonly ILogger<BackgroundMemberSyncService> _logger;

    public BackgroundMemberSyncService(
        IDbContextFactory<JobBankDbContext> dbContextFactory,
        ILogger<BackgroundMemberSyncService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("開始同步會員資料");

                // 每次建立新的 DbContext，避免長期持有
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(stoppingToken);

                var membersToSync = await dbContext.Members
                    .Where(m => m.LastSyncedAt == null || m.LastSyncedAt < DateTime.UtcNow.AddDays(-7))
                    .AsNoTracking()
                    .Take(100) // 分批處理
                    .ToListAsync(stoppingToken);

                if (membersToSync.Count == 0)
                {
                    _logger.LogInformation("無待同步會員");
                }
                else
                {
                    _logger.LogInformation("準備同步 {Count} 個會員", membersToSync.Count);
                    // 同步邏輯...
                }

                // 等待 5 分鐘後再次執行
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("會員同步服務已停止");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "會員同步時發生錯誤，30 秒後重試");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}

// ============================================================================
// 範例 3：測試環境使用方式
// ============================================================================

/// <summary>
/// 測試基類，演示在測試中如何使用 DbContextFactory
/// </summary>
public class MemberRepositoryTests : IAsyncLifetime
{
    private readonly IDbContextFactory<JobBankDbContext> _dbContextFactory;
    private readonly MemberRepository _repository;

    public MemberRepositoryTests()
    {
        // 測試中使用 Testcontainers 或本機資料庫
        var services = new ServiceCollection();
        services.AddDbContextFactory<JobBankDbContext>((options) =>
        {
            options.UseSqlServer("Server=(local);Database=JobBankTest;Integrated Security=true;");
        });

        var serviceProvider = services.BuildServiceProvider();
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<JobBankDbContext>>();

        var logger = serviceProvider.GetRequiredService<ILogger<MemberRepository>>();
        _repository = new MemberRepository(_dbContextFactory, logger);
    }

    public async Task InitializeAsync()
    {
        // 測試前準備資料庫
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // 測試後清理
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.EnsureDeletedAsync();
    }

    // [Fact]
    // public async Task GetMemberById_ShouldReturnMember_WhenMemberExists()
    // {
    //     // Arrange
    //     var memberId = Guid.NewGuid();
    //     // ... 插入測試資料
    //
    //     // Act
    //     var result = await _repository.GetMemberByIdAsync(memberId);
    //
    //     // Assert
    //     Assert.True(result.IsSuccess);
    // }
}

// ============================================================================
// 參考模型與 DTO
// ============================================================================

public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

public record MemberDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record CreateMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public record UpdateMemberRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// ============================================================================
// 檢查清單
// ============================================================================
/*
DbContextFactory 最佳實踐檢查清單：

✅ 使用
  [ ] 使用 AddDbContextFactory<T>() 而非 AddDbContext<T>()
  [ ] 在需要時才建立 DbContext：await dbContextFactory.CreateDbContextAsync()
  [ ] 使用 await using 自動釋放資源
  [ ] 每次操作後 DbContext 會自動 Dispose
  [ ] 傳遞 CancellationToken 到所有非同步方法

✅ 查詢最佳化
  [ ] 唯讀查詢使用 AsNoTracking()
  [ ] 複雜查詢使用 Include/ThenInclude
  [ ] 大量資料使用分頁（Skip/Take）
  [ ] 避免 N+1 查詢問題

✅ 錯誤處理
  [ ] 捕捉 DbUpdateException
  [ ] 捕捉 OperationCanceledException
  [ ] 所有例外轉換為 Result 或 Failure
  [ ] 保存原始例外到 Exception 屬性
  [ ] 使用 ILogger 記錄錯誤

✅ 交易管理
  [ ] 複雜操作使用 BeginTransactionAsync
  [ ] 錯誤時執行 RollbackAsync
  [ ] 成功時執行 CommitAsync

✅ 背景服務
  [ ] 定期建立新的 DbContext（勿長期持有）
  [ ] 尊重 CancellationToken
  [ ] 適當的重試邏輯
  [ ] 關閉時優雅停止

✅ 測試環境
  [ ] 使用 IDbContextFactory 而非直接注入 DbContext
  [ ] 測試前後進行資料庫初始化/清理
  [ ] 使用 Testcontainers 或測試資料庫
*/
