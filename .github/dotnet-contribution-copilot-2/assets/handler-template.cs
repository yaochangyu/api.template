using CSharpFunctionalExtensions;

namespace {{NAMESPACE}};

/// <summary>
/// {{FEATURE_NAME}} 業務邏輯處理器
/// </summary>
public class {{FEATURE_NAME}}Handler
{
    private readonly {{FEATURE_NAME}}Repository _repository;
    private readonly IContextGetter _contextGetter;
    private readonly ILogger<{{FEATURE_NAME}}Handler> _logger;

    public {{FEATURE_NAME}}Handler(
        {{FEATURE_NAME}}Repository repository,
        IContextGetter contextGetter,
        ILogger<{{FEATURE_NAME}}Handler> logger)
    {
        _repository = repository;
        _contextGetter = contextGetter;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<Result<List<{{FEATURE_NAME}}Response>>> GetAllAsync()
    {
        try
        {
            var context = _contextGetter.GetContext();
            
            _logger.LogInformation(
                "Getting all {{FEATURE_NAME}}, RequestId: {RequestId}, UserId: {UserId}",
                context.RequestId,
                context.UserId);

            var entities = await _repository.GetAllAsync();
            var response = entities.Select(MapToResponse).ToList();

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {{FEATURE_NAME}}");
            return Result.Failure<List<{{FEATURE_NAME}}Response>>(
                "Failed to retrieve {{FEATURE_NAME}} list");
        }
    }

    /// <summary>
    /// 根據 ID 取得{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<Result<{{FEATURE_NAME}}Response>> GetByIdAsync(int id)
    {
        try
        {
            var context = _contextGetter.GetContext();
            
            _logger.LogInformation(
                "Getting {{FEATURE_NAME}} {Id}, RequestId: {RequestId}",
                id,
                context.RequestId);

            var entity = await _repository.GetByIdAsync(id);
            
            if (entity == null)
            {
                _logger.LogWarning("{{FEATURE_NAME}} {Id} not found", id);
                return Result.Failure<{{FEATURE_NAME}}Response>(
                    $"{{FEATURE_NAME}} with ID {id} not found");
            }

            return Result.Success(MapToResponse(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {{FEATURE_NAME}} {Id}", id);
            return Result.Failure<{{FEATURE_NAME}}Response>(
                $"Failed to retrieve {{FEATURE_NAME}} with ID {id}");
        }
    }

    /// <summary>
    /// 建立新的{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<Result<{{FEATURE_NAME}}Response>> CreateAsync(
        Create{{FEATURE_NAME}}Request request)
    {
        try
        {
            var context = _contextGetter.GetContext();
            
            _logger.LogInformation(
                "Creating {{FEATURE_NAME}}, RequestId: {RequestId}, UserId: {UserId}",
                context.RequestId,
                context.UserId);

            // TODO: 加入業務驗證邏輯
            // 範例:
            // var existingEntity = await _repository.GetByNameAsync(request.Name);
            // if (existingEntity != null)
            // {
            //     return Result.Failure<{{FEATURE_NAME}}Response>(
            //         $"{{FEATURE_NAME}} with name {request.Name} already exists");
            // }

            var entity = MapToEntity(request);
            
            // 設定審計欄位
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = context.UserId;

            var created = await _repository.CreateAsync(entity);

            _logger.LogInformation(
                "{{FEATURE_NAME}} created successfully with ID {Id}",
                created.Id);

            return Result.Success(MapToResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {{FEATURE_NAME}}");
            return Result.Failure<{{FEATURE_NAME}}Response>(
                "Failed to create {{FEATURE_NAME}}");
        }
    }

    /// <summary>
    /// 更新{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<Result<{{FEATURE_NAME}}Response>> UpdateAsync(
        int id,
        Update{{FEATURE_NAME}}Request request)
    {
        try
        {
            var context = _contextGetter.GetContext();
            
            _logger.LogInformation(
                "Updating {{FEATURE_NAME}} {Id}, RequestId: {RequestId}, UserId: {UserId}",
                id,
                context.RequestId,
                context.UserId);

            var entity = await _repository.GetByIdAsync(id);
            
            if (entity == null)
            {
                _logger.LogWarning("{{FEATURE_NAME}} {Id} not found for update", id);
                return Result.Failure<{{FEATURE_NAME}}Response>(
                    $"{{FEATURE_NAME}} with ID {id} not found");
            }

            // TODO: 更新實體屬性
            // 範例:
            // entity.Name = request.Name;
            // entity.Description = request.Description;

            // 設定審計欄位
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = context.UserId;

            var updated = await _repository.UpdateAsync(entity);

            _logger.LogInformation(
                "{{FEATURE_NAME}} {Id} updated successfully",
                id);

            return Result.Success(MapToResponse(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {{FEATURE_NAME}} {Id}", id);
            return Result.Failure<{{FEATURE_NAME}}Response>(
                $"Failed to update {{FEATURE_NAME}} with ID {id}");
        }
    }

    /// <summary>
    /// 刪除{{FEATURE_DISPLAY_NAME}}
    /// </summary>
    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var context = _contextGetter.GetContext();
            
            _logger.LogInformation(
                "Deleting {{FEATURE_NAME}} {Id}, RequestId: {RequestId}, UserId: {UserId}",
                id,
                context.RequestId,
                context.UserId);

            var entity = await _repository.GetByIdAsync(id);
            
            if (entity == null)
            {
                _logger.LogWarning("{{FEATURE_NAME}} {Id} not found for deletion", id);
                return Result.Failure(
                    $"{{FEATURE_NAME}} with ID {id} not found");
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation(
                "{{FEATURE_NAME}} {Id} deleted successfully",
                id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {{FEATURE_NAME}} {Id}", id);
            return Result.Failure(
                $"Failed to delete {{FEATURE_NAME}} with ID {id}");
        }
    }

    // ==================== Private Methods ====================

    private {{FEATURE_NAME}}Response MapToResponse({{FEATURE_NAME}}Entity entity)
    {
        return new {{FEATURE_NAME}}Response
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
            // TODO: 對應其他屬性
        };
    }

    private {{FEATURE_NAME}}Entity MapToEntity(Create{{FEATURE_NAME}}Request request)
    {
        return new {{FEATURE_NAME}}Entity
        {
            // TODO: 對應請求屬性到實體
        };
    }
}
