# Skill: code-review (程式碼審查)

## 📋 職責
檢查程式碼是否符合專案規範、最佳實踐、分層設計原則

## 🎯 使用時機
- 完成功能開發後
- Pull Request 審查前
- 重構程式碼時
- 學習專案規範時

## 📝 工作流程

### 1. 詢問審查範圍
```
1️⃣ 審查範圍：
   - 完整專案審查
   - 特定功能模組 (指定路徑)
   - 特定檔案 (指定檔案清單)
   - 最近的變更 (Git diff)
   
2️⃣ 審查項目：
   - [x] 架構與分層設計
   - [x] 命名規範
   - [x] 錯誤處理
   - [x] 測試覆蓋率
   - [x] 效能問題
   - [x] 安全性問題
   - [ ] 僅檢查特定項目（自選）
```

## 🔍 審查檢查清單

### A. 架構與分層設計

#### Controller 層檢查
```csharp
// ✅ 正確
[ApiController]
[Route("api/[controller]")]
public class MemberController : ControllerBase
{
    private readonly MemberHandler _handler;
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateMemberRequest request)
    {
        var result = await _handler.CreateAsync(request);
        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : StatusCode(500, new { error = result.Error });
    }
}

// ❌ 錯誤：Controller 包含業務邏輯
public async Task<IActionResult> Create(CreateMemberRequest request)
{
    // ❌ 不應在 Controller 直接操作資料庫
    var existing = await _context.Members
        .FirstOrDefaultAsync(m => m.Email == request.Email);
    if (existing != null)
    {
        return Conflict("Email already exists");
    }
    
    // ❌ 不應在 Controller 處理業務邏輯
    var member = new Member { Email = request.Email };
    _context.Members.Add(member);
    await _context.SaveChangesAsync();
}
```

#### Handler 層檢查
```csharp
// ✅ 正確
public class MemberHandler
{
    private readonly IContextGetter _contextGetter;  // ✅ 使用 TraceContext
    private readonly ILogger<MemberHandler> _logger; // ✅ 結構化日誌
    
    public async Task<Result<MemberResponse>> CreateAsync(CreateMemberRequest request)
    {
        try
        {
            var context = _contextGetter.GetContext();
            
            _logger.LogInformation(
                "Creating member, RequestId: {RequestId}",
                context.RequestId);
            
            // ✅ 業務邏輯在 Handler
            var existing = await _repository.GetByEmailAsync(request.Email);
            if (existing != null)
            {
                return Result.Failure<MemberResponse>("Email already exists");
            }
            
            // ✅ 呼叫 Repository 執行資料操作
            var member = await _repository.CreateAsync(entity);
            
            return Result.Success(MapToResponse(member));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            return Result.Failure<MemberResponse>("Failed to create member");
        }
    }
}

// ❌ 錯誤：Handler 直接使用 HttpContext
public async Task<Result<MemberResponse>> CreateAsync(
    CreateMemberRequest request,
    HttpContext httpContext)  // ❌ 不應依賴 HTTP 相關物件
{
    var userId = httpContext.User.FindFirst("sub")?.Value; // ❌ 應使用 TraceContext
}
```

#### Repository 層檢查
```csharp
// ✅ 正確
public class MemberRepository
{
    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _context.Members
            .AsNoTracking()  // ✅ 查詢時使用 AsNoTracking
            .FirstOrDefaultAsync(m => m.Email == email);
    }
    
    public async Task<Member> CreateAsync(Member entity)
    {
        var context = _contextGetter.GetContext();
        
        // ✅ 設定審計欄位
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = context.UserId;
        
        _context.Members.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }
}

// ❌ 錯誤：Repository 包含業務邏輯
public async Task<Member> CreateAsync(Member entity)
{
    // ❌ 業務驗證應在 Handler
    if (string.IsNullOrEmpty(entity.Email))
    {
        throw new ArgumentException("Email is required");
    }
    
    // ❌ 業務規則判斷應在 Handler
    if (entity.Age < 18)
    {
        throw new InvalidOperationException("Must be 18 or older");
    }
}
```

---

### B. 命名規範檢查

```
✅ 檔案命名：
- Controller: MemberController.cs
- Handler: MemberHandler.cs  
- Repository: MemberRepository.cs

✅ DTO 命名：
- Request: CreateMemberRequest.cs
- Response: MemberResponse.cs

✅ 測試檔案命名：
- Feature: MemberRegistration.feature
- Steps: MemberRegistrationSteps.cs
- Unit Test: MemberHandlerTests.cs

❌ 常見錯誤：
- MemberCtrl.cs → MemberController.cs
- MemberBiz.cs → MemberHandler.cs
- MemberDAO.cs → MemberRepository.cs
```

---

### C. 錯誤處理檢查

```csharp
// ✅ 正確：使用 Result Pattern
public async Task<Result<MemberResponse>> GetByIdAsync(int id)
{
    try
    {
        var member = await _repository.GetByIdAsync(id);
        
        if (member == null)
        {
            return Result.Failure<MemberResponse>($"Member {id} not found");
        }
        
        return Result.Success(MapToResponse(member));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting member {Id}", id);
        return Result.Failure<MemberResponse>("Failed to retrieve member");
    }
}

// ❌ 錯誤：拋出例外
public async Task<MemberResponse> GetByIdAsync(int id)
{
    var member = await _repository.GetByIdAsync(id);
    
    if (member == null)
    {
        throw new NotFoundException($"Member {id} not found"); // ❌ 不應拋出
    }
    
    return MapToResponse(member);
}
```

---

### D. 測試覆蓋率檢查

```
檢查項目：
✅ API 端點是否有 BDD 測試 (.feature 檔案)
✅ 核心業務邏輯是否有單元測試
✅ 測試是否使用 Testcontainers (Docker)
❌ 是否有 Controller 單元測試（不應該有）
❌ 是否過度使用 Mock（應優先用 Testcontainers）

測試覆蓋率目標：
- API 端點: 100% (BDD)
- Handler: 80%+
- Repository: 70%+
- Controller: 0% (由 BDD 覆蓋)
```

---

### E. 效能問題檢查

```csharp
// ❌ N+1 Query 問題
public async Task<List<OrderDto>> GetOrdersAsync()
{
    var orders = await _context.Orders.ToListAsync();
    
    foreach (var order in orders)
    {
        // ❌ 每個訂單都查詢一次資料庫
        order.Items = await _context.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();
    }
}

// ✅ 使用 Include 一次查詢
public async Task<List<OrderDto>> GetOrdersAsync()
{
    var orders = await _context.Orders
        .Include(o => o.Items)  // ✅ 使用 Include
        .AsNoTracking()         // ✅ 唯讀查詢使用 AsNoTracking
        .ToListAsync();
}

// ❌ 未使用分頁
public async Task<List<MemberDto>> GetAllAsync()
{
    return await _context.Members.ToListAsync(); // ❌ 可能回傳數萬筆
}

// ✅ 使用分頁
public async Task<PagedResult<MemberDto>> GetPagedAsync(int page, int pageSize)
{
    var query = _context.Members.AsNoTracking();
    var total = await query.CountAsync();
    
    var items = await query
        .OrderByDescending(m => m.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<MemberDto>(items, total, page, pageSize);
}
```

---

### F. 安全性問題檢查

```csharp
// ❌ SQL Injection 風險
public async Task<Member?> GetByEmailAsync(string email)
{
    var sql = $"SELECT * FROM Members WHERE Email = '{email}'"; // ❌ 危險
    return await _context.Members.FromSqlRaw(sql).FirstOrDefaultAsync();
}

// ✅ 使用參數化查詢
public async Task<Member?> GetByEmailAsync(string email)
{
    return await _context.Members
        .FirstOrDefaultAsync(m => m.Email == email); // ✅ 安全
}

// ❌ 敏感資訊記錄
_logger.LogInformation("User login: {Password}", user.Password); // ❌ 記錄密碼

// ✅ 避免記錄敏感資訊
_logger.LogInformation("User login: {UserId}", user.Id); // ✅ 僅記錄 ID
```

## 📊 審查報告格式

```markdown
# 程式碼審查報告

## 審查資訊
- 審查日期: 2025-01-03
- 審查範圍: src/be/JobBank1111.Job.WebAPI/Member/
- 審查項目: 完整審查

## 🔴 嚴重問題 (必須修正)
1. **MemberController.cs:45** - Controller 包含業務邏輯
   - 問題: 直接在 Controller 操作資料庫
   - 建議: 移至 MemberHandler

## 🟡 警告 (建議修正)
1. **MemberHandler.cs:120** - 缺少錯誤處理
   - 問題: 未使用 try-catch
   - 建議: 加入錯誤處理與日誌記錄

## 🟢 通過項目
- ✅ 命名規範符合專案標準
- ✅ 使用 Result Pattern 處理錯誤
- ✅ 整合 TraceContext

## 📈 測試覆蓋率
- API 端點: 100% (5/5 個端點有 BDD 測試)
- Handler: 85%
- Repository: 75%

## 📝 建議
1. 加強異常情境測試
2. 考慮加入快取機制
```

## 🚫 禁止行為
- ❌ 不可忽略安全性問題
- ❌ 不可批准包含業務邏輯的 Controller
- ❌ 不可批准未使用 Result Pattern 的 Handler
- ❌ 不可批准缺少測試的核心功能

## ✅ 成功條件
- [x] 產生完整的審查報告
- [x] 標示所有嚴重問題
- [x] 提供具體修正建議
- [x] 檢查測試覆蓋率

## 📚 參考文件
- 架構設計指南: [@references/architecture-guide.md](../references/architecture-guide.md)
- 測試策略指南: [@references/testing-strategy.md](../references/testing-strategy.md)
- EF Core 最佳實踐: [@references/ef-core-best-practices.md](../references/ef-core-best-practices.md)

## 💡 使用範例

```bash
# 使用此 skill
@code-review 檢查 Member 模組的程式碼品質

# AI 詢問範例
> 1️⃣ 審查範圍：
>    a. 完整 Member 模組
>    b. 僅 MemberController.cs
>    c. 僅最近的變更 (Git diff)
> 
> 2️⃣ 審查項目：
>    - [x] 架構與分層
>    - [x] 命名規範
>    - [x] 錯誤處理
>    - [x] 測試覆蓋率
>    - [x] 效能與安全性
```

## 🔗 相關 Skills
- `api-dev` - API 開發（審查目標）
- `bdd-test` - 測試開發（審查測試覆蓋率）
- `architecture` - 架構設計（提供審查依據）

## 🔗 相關 Agents
- `quality-assurance-agent` - 品質保證流程（使用此 skill）
