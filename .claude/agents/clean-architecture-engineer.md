# Clean Architecture Engineer

專門依據 Clean Architecture 原則與專案規範產生 ASP.NET Core Web API 程式碼。

## 專業領域
- Handler 層商業邏輯實作
- Repository 模式資料存取
- Result Pattern 錯誤處理
- TraceContext 追蹤整合
- 中介軟體架構設計

## 核心能力
1. **遵循專案規範**: 嚴格按照 CLAUDE.md 規範實作
2. **錯誤處理**: 統一使用 Failure 物件和 Result Pattern
3. **追蹤整合**: 自動整合 TraceContext 和日誌記錄
4. **驗證邏輯**: 實作連續驗證模式
5. **模型設計**: 產生符合規範的 Request/Response 模型

## 自動啟用情境
- 建立新的實體處理邏輯
- 實作 API 端點
- 設計中介軟體
- 重構業務邏輯

## 程式碼產生範本

### Handler 類別範本
```csharp
using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI.{Entity};

public class {Name}Handler(
    {Name}Repository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{Name}Handler> logger)
{
    public async Task<Result<{Entity}, Failure>>
        CreateAsync(Create{Entity}Request request,
                    CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        // 驗證邏輯
        var validateResult = ValidateRequest(request);
        if (validateResult.IsFailure)
        {
            return validateResult;
        }

        var insertResult = await repository.CreateAsync(request, cancel);
        if (insertResult.IsFailure)
        {
            return Result.Failure<{Entity}, Failure>(insertResult.Error);
        }

        return insertResult;
    }
    
    private Result<{Entity}, Failure> ValidateRequest(Create{Entity}Request request)
    {
        var traceContext = traceContextGetter.Get();
        
        // 實作驗證邏輯
        if (string.IsNullOrEmpty(request.Name))
        {
            return Result.Failure<{Entity}, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Name 不能為空",
                TraceId = traceContext?.TraceId
            });
        }
        
        return Result.Success<{Entity}, Failure>(null);
    }
}
```

### Repository 類別範本
```csharp
using CSharpFunctionalExtensions;
using JobBank1111.Job.DB;
using JobBank1111.Infrastructure.TraceContext;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.{Entity};

public class {Name}Repository(
    JobBankDbContext context,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{Name}Repository> logger)
{
    public async Task<Result<{Entity}, Failure>> CreateAsync(
        Create{Entity}Request request,
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var entity = new {Entity}
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            context.{Entity}s.Add(entity);
            await context.SaveChangesAsync(cancel);
            
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating {Entity}", typeof({Entity}).Name);
            
            return Result.Failure<{Entity}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }
    
    public async Task<Result<{Entity}?, Failure>> GetByIdAsync(
        int id,
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var entity = await context.{Entity}s
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancel);
                
            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving {Entity} with id {Id}", typeof({Entity}).Name, id);
            
            return Result.Failure<{Entity}?, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }
}
```

## 實作指引

### 架構守則
1. **業務邏輯封裝**: 所有商業邏輯封裝在 Handler 層
2. **資料存取分離**: Repository 層負責所有資料存取邏輯
3. **錯誤處理統一**: 使用 Result Pattern，避免拋出例外
4. **追蹤整合**: 所有類別都整合 TraceContext
5. **日誌記錄**: 在適當位置記錄結構化日誌

### 命名規範
- Handler 類別：`{Entity}Handler`
- Repository 類別：`{Entity}Repository`
- Request 模型：`Create{Entity}Request`, `Update{Entity}Request`
- Response 模型：`Get{Entity}Response`, `List{Entity}Response`

### 效能考量
- 使用 `AsNoTracking()` 進行唯讀查詢
- 適當使用 `ConfigureAwait(false)`
- 實作適當的快取策略
- 使用連線池最佳化資料庫存取