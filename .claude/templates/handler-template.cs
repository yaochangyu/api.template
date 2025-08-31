using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI.{{ENTITY}};

public class {{ENTITY}}Handler(
    {{ENTITY}}Repository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{{ENTITY}}Handler> logger)
{
    public async Task<Result<{{ENTITY}}, Failure>>
        CreateAsync(Create{{ENTITY}}Request request,
                    CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        // 前置條件檢查 - 可以用 Fluent Pattern 重構
        var validateResult = ValidateCreateRequest(request);
        if (validateResult.IsFailure)
        {
            return validateResult;
        }

        var insertResult = await repository.CreateAsync(request, cancel);
        if (insertResult.IsFailure)
        {
            return Result.Failure<{{ENTITY}}, Failure>(insertResult.Error);
        }

        return insertResult;
    }

    public async Task<Result<{{ENTITY}}, Failure>>
        UpdateAsync(int id, Update{{ENTITY}}Request request,
                    CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        // 檢查實體是否存在
        var existingResult = await repository.GetByIdAsync(id, cancel);
        if (existingResult.IsFailure)
        {
            return existingResult;
        }

        // 驗證更新請求
        var validateResult = ValidateUpdateRequest(request);
        if (validateResult.IsFailure)
        {
            return validateResult;
        }

        var updateResult = await repository.UpdateAsync(id, request, cancel);
        if (updateResult.IsFailure)
        {
            return Result.Failure<{{ENTITY}}, Failure>(updateResult.Error);
        }

        return updateResult;
    }

    public async Task<Result<{{ENTITY}}, Failure>>
        GetByIdAsync(int id, CancellationToken cancel = default)
    {
        var result = await repository.GetByIdAsync(id, cancel);
        return result;
    }

    public async Task<Result<PaginatedList<Get{{ENTITY}}Response>, Failure>>
        GetOffsetAsync(int pageIndex, int pageSize, bool noCache = true, 
                       CancellationToken cancel = default)
    {
        var result = await repository.GetOffsetAsync(pageIndex, pageSize, noCache, cancel);
        return result;
    }

    public async Task<Result<CursorPaginatedList<Get{{ENTITY}}Response>, Failure>>
        GetCursorAsync(int pageSize, string nextPageToken, bool noCache = true, 
                       CancellationToken cancel = default)
    {
        var result = await repository.GetCursorAsync(pageSize, nextPageToken, noCache, cancel);
        return result;
    }

    public async Task<Result<bool, Failure>>
        DeleteAsync(int id, CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        // 檢查實體是否存在
        var existingResult = await repository.GetByIdAsync(id, cancel);
        if (existingResult.IsFailure)
        {
            return Result.Failure<bool, Failure>(existingResult.Error);
        }

        var deleteResult = await repository.DeleteAsync(id, cancel);
        return deleteResult;
    }

    private Result<{{ENTITY}}, Failure> ValidateCreateRequest(Create{{ENTITY}}Request request)
    {
        var traceContext = traceContextGetter.Get();
        
        // TODO: 實作建立請求的驗證邏輯
        if (string.IsNullOrEmpty(request.Name))
        {
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Name 不能為空",
                TraceId = traceContext?.TraceId
            });
        }

        // TODO: 加入其他驗證邏輯
        // 例如：重複性檢查、格式驗證等

        return Result.Success<{{ENTITY}}, Failure>(null);
    }

    private Result<{{ENTITY}}, Failure> ValidateUpdateRequest(Update{{ENTITY}}Request request)
    {
        var traceContext = traceContextGetter.Get();
        
        // TODO: 實作更新請求的驗證邏輯
        if (string.IsNullOrEmpty(request.Name))
        {
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Name 不能為空",
                TraceId = traceContext?.TraceId
            });
        }

        return Result.Success<{{ENTITY}}, Failure>(null);
    }

    // 可選：實作特定業務邏輯方法
    // 例如：搜尋、狀態變更、批量操作等

    public async Task<Result<List<{{ENTITY}}>, Failure>>
        SearchAsync(string keyword, CancellationToken cancel = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            var traceContext = traceContextGetter.Get();
            return Result.Failure<List<{{ENTITY}}>, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "搜尋關鍵字不能為空",
                TraceId = traceContext?.TraceId
            });
        }

        var result = await repository.SearchAsync(keyword, cancel);
        return result;
    }
}