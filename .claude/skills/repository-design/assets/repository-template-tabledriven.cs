using CSharpFunctionalExtensions;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.Member;

/// <summary>
/// 資料表導向 Repository 範本
///
/// 適用場景：
/// - 簡單主檔管理（會員、產品、分類等）
/// - 單一資料表 CRUD 操作
/// - 無複雜的跨表業務規則
/// - 初期快速開發
///
/// 設計原則：
/// 1. 每個 Repository 對應一個資料表
/// 2. 實作基本的 CRUD 操作
/// 3. 提供便利的查詢方法（GetByXxx）
/// 4. 使用 Result Pattern 進行錯誤處理
/// 5. 支援 CancellationToken，優雅取消
/// </summary>
public class MemberRepository
{
    private readonly IDbContextFactory<JobBank1111DbContext> _dbContextFactory;

    public MemberRepository(IDbContextFactory<JobBank1111DbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <summary>
    /// 根據 ID 獲取會員
    /// </summary>
    /// <param name="id">會員 ID</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>會員實體或失敗結果</returns>
    public async Task<Result<Member>> GetAsync(Guid id, CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            var member = await dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancel);

            return member == null
                ? Result.Failure<Member, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"會員不存在: {id}"
                    })
                : Result.Success<Member, Failure>(member);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 根據 Email 獲取會員
    /// </summary>
    /// <param name="email">會員郵件地址</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>會員實體或失敗結果</returns>
    public async Task<Result<Member>> GetByEmailAsync(string email, CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            var member = await dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Email == email, cancel);

            return member == null
                ? Result.Failure<Member, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"會員不存在: {email}"
                    })
                : Result.Success<Member, Failure>(member);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 分頁查詢會員列表
    /// </summary>
    /// <param name="skip">跳過的記錄數</param>
    /// <param name="take">獲取的記錄數</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>會員列表或失敗結果</returns>
    public async Task<Result<IEnumerable<Member>>> QueryAsync(
        int skip = 0,
        int take = 20,
        CancellationToken cancel = default)
    {
        try
        {
            // 驗證分頁參數
            if (skip < 0 || take <= 0 || take > 100)
            {
                return Result.Failure<IEnumerable<Member>, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "分頁參數無效: skip >= 0, 0 < take <= 100"
                    });
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            var members = await dbContext.Members
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancel);

            return Result.Success<IEnumerable<Member>, Failure>(members);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<IEnumerable<Member>, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Member>, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 根據狀態查詢會員
    /// </summary>
    /// <param name="status">會員狀態</param>
    /// <param name="skip">跳過的記錄數</param>
    /// <param name="take">獲取的記錄數</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>會員列表或失敗結果</returns>
    public async Task<Result<IEnumerable<Member>>> GetByStatusAsync(
        MemberStatus status,
        int skip = 0,
        int take = 20,
        CancellationToken cancel = default)
    {
        try
        {
            if (skip < 0 || take <= 0 || take > 100)
            {
                return Result.Failure<IEnumerable<Member>, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "分頁參數無效"
                    });
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            var members = await dbContext.Members
                .AsNoTracking()
                .Where(m => m.Status == status)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancel);

            return Result.Success<IEnumerable<Member>, Failure>(members);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<IEnumerable<Member>, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Member>, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 新增會員
    /// </summary>
    /// <param name="member">會員實體</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>新增後的會員實體或失敗結果</returns>
    public async Task<Result<Member>> InsertAsync(Member member, CancellationToken cancel = default)
    {
        try
        {
            if (member == null)
            {
                return Result.Failure<Member, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "會員實體不能為空"
                    });
            }

            // 設定初始值
            member.Id = Guid.NewGuid();
            member.CreatedAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            // 檢查 Email 是否已存在
            var existingMember = await dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Email == member.Email, cancel);

            if (existingMember != null)
            {
                return Result.Failure<Member, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.DuplicateEmail),
                        Message = $"郵件地址已被使用: {member.Email}"
                    });
            }

            dbContext.Members.Add(member);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success<Member, Failure>(member);
        }
        catch (DbUpdateException dbEx)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = "資料庫更新失敗，可能是唯一性約束衝突",
                    Exception = dbEx
                });
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.InternalServerError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 更新會員
    /// </summary>
    /// <param name="member">會員實體（必須包含 ID）</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>更新後的會員實體或失敗結果</returns>
    public async Task<Result<Member>> UpdateAsync(Member member, CancellationToken cancel = default)
    {
        try
        {
            if (member == null || member.Id == Guid.Empty)
            {
                return Result.Failure<Member, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "會員實體或 ID 無效"
                    });
            }

            // 更新時間戳
            member.UpdatedAt = DateTime.UtcNow;

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            // 檢查會員是否存在
            var existingMember = await dbContext.Members
                .FirstOrDefaultAsync(m => m.Id == member.Id, cancel);

            if (existingMember == null)
            {
                return Result.Failure<Member, Failure>(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"會員不存在: {member.Id}"
                    });
            }

            // 更新屬性
            dbContext.Entry(existingMember).CurrentValues.SetValues(member);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success<Member, Failure>(member);
        }
        catch (DbUpdateConcurrencyException concEx)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.DbConcurrency),
                    Message = "並發衝突：會員已被其他用戶修改",
                    Exception = concEx
                });
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.Timeout),
                    Message = "操作已取消"
                });
        }
        catch (Exception ex)
        {
            return Result.Failure<Member, Failure>(
                new Failure
                {
                    Code = nameof(FailureCode.InternalServerError),
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    /// <summary>
    /// 刪除會員
    /// </summary>
    /// <param name="id">會員 ID</param>
    /// <param name="cancel">取消令牌</param>
    /// <returns>成功或失敗結果</returns>
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancel = default)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return Result.Failure(
                    new Failure
                    {
                        Code = nameof(FailureCode.ValidationError),
                        Message = "會員 ID 無效"
                    });
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancel);

            var member = await dbContext.Members
                .FirstOrDefaultAsync(m => m.Id == id, cancel);

            if (member == null)
            {
                return Result.Failure(
                    new Failure
                    {
                        Code = nameof(FailureCode.InvalidOperation),
                        Message = $"會員不存在: {id}"
                    });
            }

            dbContext.Members.Remove(member);
            await dbContext.SaveChangesAsync(cancel);

            return Result.Success();
        }
        catch (DbUpdateException dbEx)
        {
            return Result.Failure(
                new Failure
                {
                    Code = nameof(FailureCode.DbError),
                    Message = "刪除失敗，可能有關聯的資料",
                    Exception = dbEx
                });
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
}

/// <summary>
/// 會員狀態列舉（示例）
/// </summary>
public enum MemberStatus
{
    /// <summary>活躍</summary>
    Active = 1,

    /// <summary>停用</summary>
    Inactive = 2,

    /// <summary>待審核</summary>
    Pending = 3
}

/// <summary>
/// 會員實體（示例）
/// 注意：實際應來自 JobBank1111.Job.DB 中的自動產生程式碼
/// </summary>
public class Member
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public MemberStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
