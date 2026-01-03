using Microsoft.EntityFrameworkCore;
using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} Repository - 資料存取層
/// </summary>
/// <remarks>
/// 此範本展示 Repository 層的標準實作模式
/// 
/// 職責：
/// - 資料存取邏輯
/// - EF Core 操作封裝
/// - 資料庫查詢最佳化
/// - 快取策略實作（可選）
/// 
/// 不應該做：
/// - ❌ 業務邏輯判斷（交給 Handler）
/// - ❌ HTTP 相關處理（交給 Controller）
/// 
/// 設計策略選擇：
/// - 簡單主檔：使用資料表導向（本範本）
/// - 複雜業務：考慮使用需求導向（OrderManagementRepository）
/// </remarks>
public class {Feature}Repository(IDbContextFactory<AppDbContext> dbContextFactory)
{
    /// <summary>
    /// 根據 ID 查詢單一 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>{Feature} 實體或錯誤</returns>
    public async Task<Result<{Feature}Entity, Failure>> GetByIdAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entity = await dbContext.{Feature}s
                .AsNoTracking()  // 唯讀查詢使用 AsNoTracking 提升效能
                .FirstOrDefaultAsync(e => e.Id == id, cancel);

            return Result.Success<{Feature}Entity, Failure>(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to retrieve {Feature} by ID",
                Exception = ex  // 保存原始例外
            });
        }
    }

    /// <summary>
    /// 查詢 {Feature} 列表（分頁）
    /// </summary>
    /// <param name="pageNumber">頁碼（從 0 開始）</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>分頁資料或錯誤</returns>
    public async Task<Result<List<{Feature}Entity>, Failure>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entities = await dbContext.{Feature}s
                .AsNoTracking()
                .OrderBy(e => e.CreatedAt)  // 排序
                .Skip(pageNumber * pageSize)  // 分頁：跳過前面的資料
                .Take(pageSize)  // 分頁：取得指定筆數
                .ToListAsync(cancel);

            return Result.Success<List<{Feature}Entity>, Failure>(entities);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<{Feature}Entity>, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to retrieve {Feature} list",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 建立新的 {Feature}
    /// </summary>
    /// <param name="entity">{Feature} 實體</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>建立的實體或錯誤</returns>
    public async Task<Result<{Feature}Entity, Failure>> CreateAsync(
        {Feature}Entity entity,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            dbContext.{Feature}s.Add(entity);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success<{Feature}Entity, Failure>(entity);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            // 處理重複鍵例外（例如：Email 已存在）
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DuplicateEmail),
                Message = "Duplicate key violation",
                Exception = ex
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 處理併發例外
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbConcurrency),
                Message = "Concurrency conflict occurred",
                Exception = ex
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to create {Feature}",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 更新現有的 {Feature}
    /// </summary>
    /// <param name="entity">{Feature} 實體</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>更新的實體或錯誤</returns>
    public async Task<Result<{Feature}Entity, Failure>> UpdateAsync(
        {Feature}Entity entity,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            dbContext.{Feature}s.Update(entity);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success<{Feature}Entity, Failure>(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbConcurrency),
                Message = "The record has been modified by another user",
                Exception = ex
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to update {Feature}",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 刪除 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>成功或錯誤</returns>
    public async Task<Result<bool, Failure>> DeleteAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entity = await dbContext.{Feature}s.FindAsync(new object[] { id }, cancel);
            
            if (entity == null)
            {
                return Result.Failure<bool, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = $"{Feature} with ID {id} not found"
                });
            }

            dbContext.{Feature}s.Remove(entity);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success<bool, Failure>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to delete {Feature}",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 檢查 {Feature} 是否存在（by ID）
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>是否存在或錯誤</returns>
    public async Task<Result<bool, Failure>> ExistsAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var exists = await dbContext.{Feature}s
                .AsNoTracking()
                .AnyAsync(e => e.Id == id, cancel);

            return Result.Success<bool, Failure>(exists);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to check {Feature} existence",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 根據條件查詢（範例：by Email）
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>{Feature} 實體或錯誤</returns>
    public async Task<Result<{Feature}Entity, Failure>> GetByEmailAsync(
        string email,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entity = await dbContext.{Feature}s
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == email, cancel);

            return Result.Success<{Feature}Entity, Failure>(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<{Feature}Entity, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to retrieve {Feature} by email",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 檢查 Email 是否已存在
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>是否存在或錯誤</returns>
    public async Task<Result<bool, Failure>> ExistsByEmailAsync(
        string email,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var exists = await dbContext.{Feature}s
                .AsNoTracking()
                .AnyAsync(e => e.Email == email, cancel);

            return Result.Success<bool, Failure>(exists);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to check email existence",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 批次插入（效能最佳化）
    /// </summary>
    /// <param name="entities">{Feature} 實體清單</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>成功或錯誤</returns>
    public async Task<Result<bool, Failure>> BulkInsertAsync(
        List<{Feature}Entity> entities,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            // 批次插入，一次 SaveChanges
            dbContext.{Feature}s.AddRange(entities);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success<bool, Failure>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to bulk insert {Feature}s",
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 檢查是否為重複鍵例外
    /// </summary>
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        // SQL Server
        if (ex.InnerException?.Message.Contains("duplicate key") == true)
            return true;

        // SQLite
        if (ex.InnerException?.Message.Contains("UNIQUE constraint") == true)
            return true;

        // PostgreSQL
        if (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true)
            return true;

        return false;
    }
}

/// <summary>
/// AppDbContext（範例）
/// </summary>
/// <remarks>
/// 實際的 DbContext 應定義在 DB 專案中
/// </remarks>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<{Feature}Entity> {Feature}s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 實體配置（範例）
        modelBuilder.Entity<{Feature}Entity>(entity =>
        {
            entity.ToTable("{Feature}s");
            entity.HasKey(e => e.Id);
            
            // TODO: 根據實際需求配置
            // entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            // entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

/*
使用方式：

1. 將 {Feature} 替換為實際的功能名稱（例如：Member、Product、Order）
2. 根據實際需求新增查詢方法
3. 註冊服務到 DI 容器

範例：
services.AddScoped<MemberRepository>();
services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

注意事項：
- ✅ 使用 IDbContextFactory 而非直接注入 DbContext
- ✅ 使用 await using 確保資源釋放
- ✅ 唯讀查詢使用 AsNoTracking()
- ✅ 使用 Result Pattern 處理錯誤
- ✅ 保存原始例外到 Failure.Exception
- ✅ 傳遞 CancellationToken
- ✅ 使用 Skip + Take 實作分頁
- ❌ 不在 Repository 中處理業務邏輯
- ❌ 不記錄錯誤日誌

進階技巧：
- 使用 Include/ThenInclude 避免 N+1 問題
- 使用 Select 投影減少資料傳輸
- 使用編譯查詢提升效能（頻繁執行的查詢）
- 考慮實作快取策略（使用 IDistributedCache）
*/
