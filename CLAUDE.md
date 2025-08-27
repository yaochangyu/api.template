# CLAUDE.md

此檔案為 Claude Code (claude.ai/code) 在此專案中工作時的指導文件。

## 開發指令

### 建置與執行
- **開發模式執行 API**: `task api-dev` (使用 watch 模式與 --local 參數)
- **建置解決方案**: `dotnet build src/be/JobBank1111.Job.Management.sln`
- **執行單元測試**: `dotnet test src/be/JobBank1111.Job.Test/JobBank1111.Job.Test.csproj`
- **執行整合測試**: `dotnet test src/be/JobBank1111.Job.IntegrationTest/JobBank1111.Job.IntegrationTest.csproj`

### 程式碼產生
- **產生 API 客戶端與伺服器端程式碼**: `task codegen-api`
- **僅產生 API 客戶端程式碼**: `task codegen-api-client`
- **僅產生 API 伺服器端程式碼**: `task codegen-api-server`
- **產生 EF Core 實體**: `task ef-codegen`

### 基礎設施
- **啟動 Redis**: `task redis-start`
- **啟動 Redis 管理介面**: `task redis-admin-start`
- **初始化開發環境**: `task dev-init`

### 文件
- **產生 API 文件**: `task codegen-api-doc`
- **預覽 API 文件**: `task codegen-api-preview`

## 架構概述

這是一個使用 Clean Architecture 模式的 .NET 8.0 Web API 專案，架構如下：

### 核心專案
- **JobBank1111.Job.WebAPI**: 主要的 Web API 應用程式，包含控制器、處理器與中介軟體
- **JobBank1111.Infrastructure**: 跨領域基礎設施服務 (快取、工具、追蹤內容)
- **JobBank1111.Job.DB**: Entity Framework Core 資料存取層，包含自動產生的實體
- **JobBank1111.Job.Contract**: 從 OpenAPI 規格自動產生的 API 客戶端合約

### 測試專案
- **JobBank1111.Job.Test**: 使用 xUnit 的單元測試
- **JobBank1111.Job.IntegrationTest**: 使用 xUnit、Testcontainers 與 Reqnroll (BDD) 的整合測試
- **JobBank1111.Testing.Common**: 共享測試工具與模擬伺服器協助器

### 主要架構模式
- **處理器模式**: 商業邏輯封裝在處理器類別中 (例如 `MemberHandler`)
- **儲存庫模式**: 透過儲存庫類別進行資料存取 (例如 `MemberRepository`)
- **責任鏈模式**: 複雜操作的處理鏈 (例如 `MemberChain`)
- **中介軟體管線**: 用於追蹤內容與日誌記錄的自訂中介軟體
- **相依性注入**: 完整的 DI 容器設定與範圍驗證

### 技術堆疊
- **框架**: ASP.NET Core 8.0 with minimal APIs
- **資料庫**: Entity Framework Core 與 SQL Server
- **快取**: Redis 搭配 `CacheProviderFactory` 的記憶體內快取備援
- **日誌記錄**: Serilog 結構化日誌輸出至控制台、檔案與 Seq
- **測試**: xUnit、FluentAssertions、Testcontainers 整合測試
- **API 文件**: Swagger/OpenAPI 搭配 ReDoc 與 Scalar 檢視器
- **程式碼產生**: 客戶端使用 Refitter，伺服器控制器使用 NSwag

### 設定檔
- 使用 `--local` 參數時從 `env/local.env` 載入環境變數
- `JobBank1111.Job.WebAPI/appsettings.json` 中的應用程式設定
- Redis 與 Seq 日誌伺服器的 Docker Compose 設定
- `Taskfile.yml` 中的任務執行器設定

### 程式碼產生工作流程
專案使用 OpenAPI-first 開發方式：
1. API 規格維護在 `doc/openapi.yml`
2. 使用 Refitter 產生客戶端程式碼至 `JobBank1111.Job.Contract`
3. 使用 NSwag 產生伺服器控制器至 `JobBank1111.Job.WebAPI/Contract`
4. 使用 EF Core 反向工程產生資料庫實體

### 開發工作流程
1. 更新 `doc/openapi.yml` 中的 OpenAPI 規格
2. 執行 `task codegen-api` 重新產生客戶端/伺服器端程式碼
3. 在處理器與儲存庫中實作商業邏輯
4. 執行 `task api-dev` 進行熱重載開發
5. 使用 BDD 情境的整合測試進行測試

## 開發原則

### 不可變物件設計 (Immutable Objects)
- 使用 C# record 類型定義不可變物件，例如 `TraceContext`
- 所有屬性使用 `init` 關鍵字，確保物件在建立後無法修改
- 避免在應用程式各層間傳遞可變狀態

### TraceContext 管理
- **集中式管理**: 所有追蹤內容與使用者資訊統一在 `TraceContextMiddleware` 中處理
- **不可變性**: `TraceContext` 使用 record 定義，包含 `TraceId` 與 `UserId` 等不可變屬性
- **生命週期**: 透過 `AsyncLocal<T>` 機制確保 TraceContext 在整個請求生命週期內可用
- **服務注入**: 使用 `IContextGetter<T>` 與 `IContextSetter<T>` 介面進行依賴注入

### 架構守則
- 業務邏輯層不應直接處理 HTTP 相關邏輯
- 所有跨領域關注點 (如身分驗證、日誌、追蹤) 應在中介軟體層處理
- 使用不可變物件傳遞狀態，避免意外修改
- 透過 DI 容器注入 TraceContext，而非直接傳遞參數

### 用戶資訊管理
- **不可變性原則**: 確保物件的不可變，例如身分驗證後的用戶資訊，存放在 TraceContext
- **集中處理**: 集中在 Middleware 處理，例如 TraceContextMiddleware
- **依賴注入**: 透過 IContextSetter 設定用戶資訊
- **資訊取得**: 透過 IContextGetter 取得用戶資訊

## Result Pattern 與 Failure 物件管理

專案中的方法應套用 Result Pattern，提供明確的成功/失敗處理機制：

### Web API 層
**強制使用** `Result<TSuccess, TFailure>` 作為處理器層回傳類型                                                                             
**映射規則**: 使用 `FailureCodeMapper` 將錯誤代碼映射至 HTTP 狀態碼                                                                        
**統一轉換**: 使用 `ResultActionResult<T>` 與擴充方法 `.ToActionResult()` 統一處理成功/失敗回應

### FailureCode 定義
```csharp
public enum FailureCode
{
    Unauthorized,        // 未授權存取
    DbError,            // 資料庫錯誤
    DuplicateEmail,     // 重複郵件地址
    DbConcurrency,      // 資料庫併發衝突
    ValidationError,    // 驗證錯誤
    InvalidOperation,   // 無效操作
    Timeout,           // 逾時
    InternalServerError, // 內部伺服器錯誤
    Unknown            // 未知錯誤
}
```

### Failure 物件結構
- **Code**: 錯誤代碼，使用 `nameof(FailureCode.*)` 定義錯誤碼
- **Message**: 顯示例外的原始訊息，供開發除錯使用
- **TraceId**: 追蹤識別碼，用於日誌關聯與問題追蹤
- **Exception**: 原始例外物件，不會序列化到客戶端回應
- **Data**: 包含例外類型與時間戳記的結構化資料

### 實作要點
- **回傳類型**: 使用 `Result<TSuccess, TFailure>` 作為回傳類型
- **驗證鏈**: 使用連續驗證模式，遇到失敗時立即回傳
- **例外處理**: 統一捕捉例外並轉換為 `Failure` 物件
- **追蹤資訊**: 在 `Failure` 物件中包含 TraceId 用於日誌追蹤
- **結構化日誌**: 使用結構化日誌記錄錯誤資訊

### 安全回應處理
```csharp
// 不洩露內部實作細節給客戶端
var failure = new Failure
{
    Code = nameof(FailureCode.InternalServerError),
    Message = _env.IsDevelopment() ? ex.Message : "內部伺服器錯誤", // 開發環境顯示詳細訊息
    TraceId = traceContext?.TraceId,
    Data = _env.IsDevelopment() ? new { ExceptionType = ex.GetType().Name } : null
};
```

### 最佳實務
- **不要重複拋出例外**: 處理過的例外不應再次 throw
- **統一錯誤碼**: 使用 `nameof(FailureCode.*)` 定義錯誤碼
- **包含追蹤資訊**: 確保所有 Failure 物件都包含 TraceId
- **結構化資料**: 將相關資料存放在 Failure.Data 中供除錯使用
- **安全回應**: 不洩露內部實作細節給客戶端，根據環境決定訊息詳細程度
- **追蹤整合**: 自動整合 TraceContext 資訊到錯誤回應中

## 中介軟體 (Middleware) 架構與實作指引

專案使用完整的中介軟體管線處理跨領域關注點，包含例外處理、追蹤內容管理、日誌記錄與請求資訊擷取等功能。

### 中介軟體架構與責任
- **ExceptionHandlingMiddleware**: 最外層中介軟體，捕捉所有未處理例外
- **TraceContextMiddleware**: 處理使用者身分驗證與追蹤內容設定
- **LoggerMiddleware**: 記錄請求與回應日誌，例外發生時交由 ExceptionHandlingMiddleware 處理
- **RequestParameterLoggerMiddleware**: 當請求成功完成時記錄請求資訊

### TraceContext 中介軟體責任
- **身分驗證**: 在 `TraceContextMiddleware` 中統一處理使用者身分驗證
- **TraceId 處理**: 從請求標頭擷取或自動產生 TraceId
- **日誌增強**: 自動將 TraceId 與 UserId 附加到結構化日誌中
- **回應標頭**: 自動將 TraceId 加入回應標頭供追蹤使用

### 例外處理流程
ExceptionHandlingMiddleware 專門處理未預期的系統層級例外，不處理業務邏輯錯誤：

1. **捕捉系統例外**: 僅捕捉未處理的系統層級例外（如資料庫連線失敗、記憶體不足等）
2. **記錄完整資訊**: 使用結構化日誌記錄例外詳細資訊與完整請求參數
3. **建立標準回應**: 將系統例外轉換為標準化的 `Failure` 物件回應
4. **設定狀態碼**: 統一設定為 500 Internal Server Error
5. **回傳 JSON**: 序列化 `Failure` 物件為 JSON 格式回傳

**重要原則**：
- **保持整潔**: 不處理業務邏輯錯誤，業務錯誤應在 Handler 層使用 Result Pattern 處理
- **必須記錄**: 每次捕捉例外時，必須記錄錯誤資訊與完整請求參數供除錯使用
- **系統級專用**: 僅處理真正的系統異常，如基礎設施故障、未預期的運行時錯誤等

### 請求資訊記錄機制
專案實作集中式請求資訊記錄機制，分別處理正常請求與例外情況的日誌記錄：

#### RequestInfoExtractor 功能
1. **路由參數**: 擷取 URL 路由中的參數
2. **查詢參數**: 擷取 URL 查詢字串參數
3. **請求標頭**: 擷取 HTTP 標頭，自動排除敏感標頭（如 Authorization、Cookie）
4. **請求本文**: 對於 POST/PUT/PATCH 請求，擷取請求本文內容並嘗試解析 JSON
5. **基本資訊**: 記錄 HTTP 方法、路徑、內容類型、內容長度等基本資訊

#### 日誌記錄策略
- **例外情況**: 在 `ExceptionHandlingMiddleware` 中記錄所有請求資訊，包含例外詳細資訊
- **正常完成**: 在 `RequestParameterLoggerMiddleware` 中記錄請求資訊，僅在沒有例外發生時記錄
- **避免重複**: 透過例外處理流程控制，確保同一請求不會重複記錄資訊

### 日誌記錄原則
- **集中式日誌**: 所有日誌記錄應集中在 Middleware 層處理，避免在 Handler 層重複寫 log
- **結構化日誌**: 使用 Serilog 結構化日誌，統一包含 TraceId 與 UserId
- **請求追蹤**: 記錄請求進入、處理時間、回應狀態等關鍵資訊
- **錯誤日誌**: 統一捕捉並記錄例外與錯誤資訊，包含完整的錯誤堆疊

### 安全考量與敏感資訊過濾
```csharp
// 敏感資訊過濾
private static readonly string[] SensitiveHeaders = 
{
    "Authorization", "Cookie", "X-API-Key", "X-Auth-Token", 
    "Set-Cookie", "Proxy-Authorization"
};

// 確保敏感資訊不會被記錄
var headers = context.Request.Headers
    .Where(h => !SensitiveHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
    .ToDictionary(h => h.Key, h => h.Value.ToString());

// 生產環境隱藏堆疊追蹤
if (_env.IsProduction())
{
    _logger.LogError("例外發生 - TraceId: {TraceId}, Type: {ExceptionType}", 
        traceId, ex.GetType().Name);
}
else
{
    _logger.LogError(ex, "例外發生 - TraceId: {TraceId}", traceId);
}
```

### 錯誤處理原則
#### 分層錯誤處理
- **業務邏輯錯誤**: 在 Handler 層使用 Result Pattern 處理，回傳適當的 HTTP 狀態碼
- **系統層級例外**: 交給 `ExceptionHandlingMiddleware` 處理，統一記錄與回應
- **避免重複**: 避免在其他 Middleware 中攔截錯誤後再次拋出，造成重複記錄

#### 建議做法
```csharp
// ✅ 建議：讓錯誤自然流向 ExceptionHandlingMiddleware
await _next(context);
```

#### 避免的做法
```csharp
// ❌ 避免：攔截錯誤後再次拋出
try
{
    await _next(context);
}
catch (Exception ex)
{
    _logger.LogError(ex, "錯誤發生");
    throw; // 避免這樣做，會造成重複記錄
}
```

### 實作要點
- **簡化設計**: 統一使用 500 Internal Server Error，避免複雜的狀態碼映射邏輯
- **完整日誌**: 記錄例外的完整資訊，包含堆疊追蹤與內容資訊
- **安全回應**: 不洩露內部實作細節給客戶端
- **追蹤整合**: 自動整合 TraceContext 資訊到錯誤回應中
- **JSON 序列化**: 使用專案統一的 JsonSerializerOptions 設定

### 實作細節範例
```csharp
// 使用方式
var requestInfo = await RequestInfoExtractor.ExtractRequestInfoAsync(context, jsonOptions);

// 例外時記錄 (ExceptionHandlingMiddleware)
_logger.LogError(exception, "Unhandled exception - RequestInfo: {@RequestInfo}", requestInfo);

// 正常完成時記錄 (RequestParameterLoggerMiddleware)  
_logger.LogInformation("Request completed - RequestInfo: {@RequestInfo}", requestInfo);
```

### 開發守則與最佳實務
- **系統例外集中處理**: 僅將系統層級未處理例外交由 ExceptionHandlingMiddleware 統一處理
- **業務錯誤分離**: 業務邏輯錯誤應在 Handler 層使用 Result Pattern 處理，不應流到 ExceptionHandlingMiddleware
- **必須記錄請求參數**: ExceptionHandlingMiddleware 捕捉例外時，必須記錄完整請求資訊供除錯
- **保持中介軟體整潔**: ExceptionHandlingMiddleware 專注於系統例外處理，不包含業務邏輯
- **一致性回應**: 確保所有系統例外都轉換為相同格式的 Failure 回應
- **追蹤完整性**: 確保例外處理過程中 TraceId 的連續性與可追蹤性
- **結構化日誌**: 使用 `{@RequestInfo}` 格式記錄結構化資料
- **統一格式**: 所有請求資訊記錄使用相同的資料結構
- **效能考量**: 只有在需要時才擷取請求本文，避免不必要的處理
- **可擴展性**: 透過靜態方法設計，便於在不同中介軟體中重用
- **敏感資訊過濾**: 自動排除 Authorization、Cookie、x-api-key 等敏感標頭
- **錯誤容錯**: 記錄過程中發生錯誤不影響業務邏輯執行

### 特殊情況處理
- 若特定 Middleware 確實需要攔截錯誤處理特殊業務邏輯，可以例外處理
- 但應謹慎評估是否真的必要，優先考慮在業務層處理