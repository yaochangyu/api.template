using Microsoft.EntityFrameworkCore;

namespace {{NAMESPACE}};

/// <summary>
/// {{FEATURE_NAME}} 資料存取層
/// </summary>
public class {{FEATURE_NAME}}Repository
{
    private readonly {{DB_CONTEXT_NAME}} _context;
    private readonly IContextGetter _contextGetter;
    private readonly ILogger<{{FEATURE_NAME}}Repository> _logger;

    public {{FEATURE_NAME}}Repository(
        {{DB_CONTEXT_NAME}} context,
        IContextGetter contextGetter,
        ILogger<{{FEATURE_NAME}}Repository> logger)
    {
        _context = context;
        _contextGetter = contextGetter;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<List<{{FEATURE_NAME}}Entity>> GetAllAsync()
    {
        try
        {
            return await _context.{{FEATURE_NAME}}s
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all {{FEATURE_NAME}}");
            throw;
        }
    }

    /// <summary>
    /// 根據 ID 取得{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<{{FEATURE_NAME}}Entity?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.{{FEATURE_NAME}}s
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {{FEATURE_NAME}} {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// 建立新的{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<{{FEATURE_NAME}}Entity> CreateAsync({{FEATURE_NAME}}Entity entity)
    {
        try
        {
            var context = _contextGetter.GetContext();

            // 設定審計資訊
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = context.UserId;

            _context.{{FEATURE_NAME}}s.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "{{FEATURE_NAME}} created with ID {Id} by user {UserId}",
                entity.Id,
                context.UserId);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {{FEATURE_NAME}}");
            throw;
        }
    }

    /// <summary>
    /// 更新{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<{{FEATURE_NAME}}Entity> UpdateAsync({{FEATURE_NAME}}Entity entity)
    {
        try
        {
            var context = _contextGetter.GetContext();

            // 設定審計資訊
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = context.UserId;

            _context.{{FEATURE_NAME}}s.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "{{FEATURE_NAME}} {Id} updated by user {UserId}",
                entity.Id,
                context.UserId);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {{FEATURE_NAME}} {Id}", entity.Id);
            throw;
        }
    }

    /// <summary>
    /// 刪除{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        try
        {
            var context = _contextGetter.GetContext();
            var entity = await _context.{{FEATURE_NAME}}s.FindAsync(id);

            if (entity == null)
            {
                throw new InvalidOperationException(
                    $"{{FEATURE_NAME}} with ID {id} not found");
            }

            _context.{{FEATURE_NAME}}s.Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "{{FEATURE_NAME}} {Id} deleted by user {UserId}",
                id,
                context.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {{FEATURE_NAME}} {Id}", id);
            throw;
        }
    }

    // ==================== Custom Query Methods ====================

    /// <summary>
    /// 根據條件查詢{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    /// <example>
    /// public async Task<List<{{FEATURE_NAME}}Entity>> SearchAsync(string keyword)
    /// {
    ///     return await _context.{{FEATURE_NAME}}s
    ///         .AsNoTracking()
    ///         .Where(x => x.Name.Contains(keyword))
    ///         .ToListAsync();
    /// }
    /// </example>

    /// <summary>
    /// 分頁查詢{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    /// <example>
    /// public async Task<(List<{{FEATURE_NAME}}Entity> Items, int TotalCount)> GetPagedAsync(
    ///     int pageNumber, 
    ///     int pageSize)
    /// {
    ///     var query = _context.{{FEATURE_NAME}}s.AsNoTracking();
    ///     
    ///     var totalCount = await query.CountAsync();
    ///     var items = await query
    ///         .OrderByDescending(x => x.CreatedAt)
    ///         .Skip((pageNumber - 1) * pageSize)
    ///         .Take(pageSize)
    ///         .ToListAsync();
    ///     
    ///     return (items, totalCount);
    /// }
    /// </example>

    /// <summary>
    /// 檢查{{FEATURE_DISPLAY_NAME}}是否存在
    /// </summary>
    /// <example>
    /// public async Task<bool> ExistsAsync(int id)
    /// {
    ///     return await _context.{{FEATURE_NAME}}s
    ///         .AnyAsync(x => x.Id == id);
    /// }
    /// </example>
}

// ==================== Entity Class ====================

/// <summary>
/// {{FEATURE_NAME}} 實體類別
/// </summary>
/// <remarks>
/// 如果使用 EF Core 反向工程，此類別會在 AutoGenerated 資料夾自動產生。
/// 此處僅供參考，實際開發請使用自動產生的類別。
/// </remarks>
public class {{FEATURE_NAME}}Entity
{
    public int Id { get; set; }

    // TODO: 加入業務屬性
    // 範例:
    // public string Name { get; set; } = string.Empty;
    // public string Description { get; set; } = string.Empty;

    // 審計欄位
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
