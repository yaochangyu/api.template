# Skill: api-dev (API 開發)

## 📋 職責
建立 Controller/Handler/Repository、遵循分層架構、產生 API 程式碼骨架

## 🎯 使用時機
- 實作新的 API 端點
- 建立新的業務功能
- 需要完整的三層架構程式碼

## 📝 工作流程

### 1. 詢問開發流程選擇
```
1️⃣ API 開發流程選擇：
   - ✅ API First (推薦)
     → 先定義 OpenAPI 規格 (doc/openapi.yml)
     → 透過 task codegen-api-server 產生 Controller 骨架
     → 確保 API 契約優先、文件與實作同步
   
   - ✅ Code First
     → 直接實作程式碼
     → 後續手動維護 OpenAPI 規格
```

### 2. OpenAPI 規格狀態（僅 API First）
```
2️⃣ OpenAPI 規格定義狀態：
   - 已定義：doc/openapi.yml 已包含此 API 規格
   - 需要更新：需要修改 doc/openapi.yml 加入新 endpoint
   - 尚未定義：需要從頭建立 OpenAPI 規格
```

### 3. 詢問需要實作的分層
```
3️⃣ 需要實作的分層：
   - [x] Controller (HTTP 請求處理)
   - [x] Handler (業務邏輯)
   - [x] Repository (資料存取)
   - [ ] 僅 Controller + Handler
   - [ ] 僅 Handler + Repository
```

### 4. 產生程式碼骨架

#### Controller 層
使用範本：[controller-template.cs](../assets/controller-template.cs)

```csharp
[ApiController]
[Route("api/[controller]")]
public class MemberController : ControllerBase
{
    private readonly MemberHandler _handler;
    
    [HttpPost]
    [ProducesResponseType(typeof(MemberResponse), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMemberRequest request)
    {
        var result = await _handler.CreateAsync(request);
        
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), 
                new { id = result.Value.Id }, result.Value)
            : StatusCode(500, new { error = result.Error });
    }
}
```

#### Handler 層
使用範本：[handler-template.cs](../assets/handler-template.cs)

```csharp
public class MemberHandler
{
    private readonly MemberRepository _repository;
    private readonly IContextGetter _contextGetter;
    private readonly ILogger<MemberHandler> _logger;
    
    public async Task<Result<MemberResponse>> CreateAsync(
        CreateMemberRequest request)
    {
        var context = _contextGetter.GetContext();
        
        _logger.LogInformation(
            "Creating member, RequestId: {RequestId}",
            context.RequestId);
        
        // 業務邏輯實作...
    }
}
```

#### Repository 層
使用範本：[repository-template.cs](../assets/repository-template.cs)

```csharp
public class MemberRepository
{
    private readonly JobBankDbContext _context;
    private readonly IContextGetter _contextGetter;
    
    public async Task<Member> CreateAsync(Member entity)
    {
        var context = _contextGetter.GetContext();
        
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = context.UserId;
        
        _context.Members.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }
}
```

## 📐 分層架構原則

### Controller 層職責
- ✅ HTTP 請求/回應處理
- ✅ 路由定義
- ✅ 請求驗證
- ✅ HTTP 狀態碼對應
- ❌ **禁止**：業務邏輯、資料庫操作

### Handler 層職責  
- ✅ 核心業務邏輯
- ✅ 流程協調
- ✅ 錯誤處理與結果封裝
- ✅ 呼叫 Repository
- ❌ **禁止**：HTTP 相關處理、直接資料庫操作

### Repository 層職責
- ✅ 資料存取邏輯
- ✅ EF Core 操作
- ✅ 資料庫查詢封裝
- ❌ **禁止**：業務邏輯

## 🏗️ Repository 設計策略

### 需求導向 > 資料表導向

```
❌ 錯誤：資料表導向
MemberRepository → 僅處理 Members 表
OrderRepository → 僅處理 Orders 表
OrderItemRepository → 僅處理 OrderItems 表

✅ 正確：需求導向  
MemberManagementRepository → 處理會員相關的所有資料操作
OrderManagementRepository → 處理訂單+訂單明細+付款等
```

### 選擇建議
```
📊 簡單專案 (< 10 表):
   → 使用資料表導向

📊 複雜專案 (> 10 表):
   → 使用需求導向

📊 混合模式 (推薦):
   → 核心業務用需求導向
   → 簡單主檔用資料表導向
```

## 🔧 程式碼產生工具整合

### API First 流程
```bash
# 1. 定義 OpenAPI 規格
vi doc/openapi.yml

# 2. 產生 Controller 骨架
task codegen-api-server

# 3. 實作 Handler 與 Repository
# (使用此 skill 產生)

# 4. 產生客戶端程式碼
task codegen-api-client
```

### Code First 流程
```bash
# 1. 直接實作程式碼
# (使用此 skill 產生)

# 2. 手動維護 OpenAPI 規格
vi doc/openapi.yml

# 3. 產生客戶端程式碼
task codegen-api-client
```

## 📝 命名規範

### 檔案命名
- Controller: `{Feature}Controller.cs` 或 `{Feature}ControllerImpl.cs`
- Handler: `{Feature}Handler.cs`
- Repository: `{Feature}Repository.cs`

### DTO 命名
- Request: `{Action}{Feature}Request.cs`
- Response: `{Feature}Response.cs`
- 範例: `CreateMemberRequest`, `MemberResponse`

## 🚫 禁止行為
- ❌ 不可在 Controller 中寫業務邏輯
- ❌ 不可在 Handler 中直接使用 HttpContext
- ❌ 不可在 Repository 中處理業務邏輯
- ❌ 不可跳過分層詢問（必須確認用戶需要哪些層）

## ✅ 成功條件
- [x] 產生符合分層架構的程式碼
- [x] 所有類別都有 XML 註解
- [x] 使用 Result Pattern 處理錯誤
- [x] 整合 TraceContext 進行追蹤
- [x] 符合專案命名規範

## 📚 參考文件
- 架構設計指南: [@references/architecture-guide.md](../references/architecture-guide.md)
- TraceContext 指南: [@references/trace-context-guide.md](../references/trace-context-guide.md)

## 💡 使用範例

```bash
# 使用此 skill
@api-dev 建立會員管理的 Controller, Handler, Repository

# AI 詢問範例
> 1️⃣ API 開發流程選擇：
>    a. API First (推薦) - 先定義 OpenAPI 規格
>    b. Code First - 直接實作程式碼
> 
> 2️⃣ 需要實作的分層：
>    a. 完整三層 (Controller + Handler + Repository)
>    b. 僅 Controller + Handler
>    c. 僅 Handler + Repository
>
> 3️⃣ Repository 設計策略：
>    a. 簡單資料表導向 (MemberRepository)
>    b. 需求導向 (MemberManagementRepository)
```

## 🔗 相關 Skills
- `bdd-test` - 配合建立 BDD 測試
- `database-ops` - 資料庫 Migration
- `code-review` - 檢查程式碼品質

## 🔗 相關 Agents
- `feature-dev-agent` - 完整功能開發流程（使用此 skill）
