# Skill: architecture (架構設計)

## 📋 職責
協助設計系統架構、選擇設計模式、提供架構決策建議

## 🎯 使用時機
- 規劃新功能的架構設計
- 選擇 Repository 設計策略
- 決定專案組織方式
- 設計中介軟體架構
- 解決架構相關問題

## 📝 工作流程

### 1. 詢問架構需求
```
1️⃣ 架構諮詢類型：
   - Repository Pattern 設計建議
   - 中介軟體設計建議
   - 專案結構組織建議
   - TraceContext 整合建議
   - 分層架構設計建議
   - 其他架構問題
```

## 🏗️ 架構諮詢類型

### A. Repository Pattern 設計建議

#### 詢問
```
1️⃣ 專案規模：
   - 小型 (< 10 個資料表)
   - 中型 (10-30 個資料表)
   - 大型 (> 30 個資料表)

2️⃣ 團隊規模：
   - 小型團隊 (1-3 人)
   - 中型團隊 (4-10 人)
   - 大型團隊 (> 10 人)

3️⃣ 業務複雜度：
   - 簡單 CRUD 為主
   - 中等複雜度業務邏輯
   - 複雜業務流程與跨表操作

4️⃣ 具體需求描述：
   例如：處理訂單管理，包含訂單、訂單明細、付款、庫存等
```

#### 建議範例
```markdown
## Repository 設計建議

### 專案分析
- 規模：中型 (15 個資料表)
- 團隊：5 人
- 複雜度：中高（訂單處理涉及多表操作）

### 推薦策略：混合模式

#### 核心業務（需求導向）
**OrderManagementRepository**
- 職責：處理訂單相關的所有操作
- 管理範圍：
  - Orders (訂單主檔)
  - OrderItems (訂單明細)
  - OrderPayments (付款記錄)
  - OrderShipments (出貨記錄)

優點：
- 封裝完整訂單業務邏輯
- 減少跨 Repository 呼叫
- 交易邊界清晰

#### 簡單主檔（資料表導向）
**MemberRepository**
- 職責：會員資料 CRUD
- 管理範圍：Members 表

**ProductRepository**
- 職責：產品資料 CRUD
- 管理範圍：Products 表

優點：
- 簡單直觀
- 易於維護

### 實作範例
\```csharp
// 需求導向 Repository
public class OrderManagementRepository
{
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 建立訂單
            var order = new Order { ... };
            _context.Orders.Add(order);
            
            // 建立訂單明細
            foreach (var item in dto.Items)
            {
                _context.OrderItems.Add(new OrderItem { OrderId = order.Id, ... });
            }
            
            // 建立付款記錄
            _context.OrderPayments.Add(new OrderPayment { OrderId = order.Id, ... });
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Result.Success(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
\```
```

---

### B. 中介軟體設計建議

#### 詢問
```
1️⃣ 中介軟體需求：
   - 身分驗證與授權
   - 請求追蹤與日誌
   - 錯誤處理與回應包裝
   - 效能監控
   - 速率限制
   - 其他自訂需求

2️⃣ 整合需求：
   - 需要整合 TraceContext
   - 需要結構化日誌
   - 需要 APM (Application Performance Monitoring)
```

#### 建議範例
```markdown
## 中介軟體執行順序

\```
請求進入
  ↓
1. ExceptionHandlerMiddleware (全域錯誤處理)
  ↓
2. UseAuthentication (身分驗證)
  ↓
3. UseAuthorization (授權)
  ↓
4. TraceContextMiddleware (追蹤上下文)
  ↓
5. RequestLoggingMiddleware (請求日誌)
  ↓
6. PerformanceMonitoringMiddleware (效能監控)
  ↓
Controller → Handler → Repository
  ↓
回應輸出
\```

### 關鍵原則
1. **錯誤處理放最外層**：捕捉所有未處理的例外
2. **驗證在 TraceContext 之前**：確保有用戶資訊可用
3. **日誌在 TraceContext 之後**：可記錄完整追蹤資訊
```

---

### C. 專案結構組織建議

#### 詢問
```
1️⃣ 團隊狀況：
   - 團隊人數
   - 前後端分工方式
   - 開發週期（短期/長期）

2️⃣ 專案特性：
   - 預期規模
   - 維護週期
   - 部署方式
```

#### 建議範例
```markdown
## 專案結構建議

### 情境：小型團隊 (3 人)、快速開發

推薦：**單一專案結構**

\```
JobBank1111.Job.WebAPI/
├── Member/
│   ├── MemberController.cs
│   ├── MemberHandler.cs
│   └── MemberRepository.cs
├── Order/
│   ├── OrderController.cs
│   ├── OrderHandler.cs
│   └── OrderRepository.cs
├── Shared/
│   ├── TraceContext.cs
│   ├── TraceContextMiddleware.cs
│   └── Extensions/
\```

優點：
- ✅ 編譯快速 (~10 秒)
- ✅ 部署簡單（單一 DLL）
- ✅ 適合快速迭代

缺點：
- ⚠️ 難以強制分層隔離
- ⚠️ 專案變大後編譯變慢

---

### 情境：大型團隊 (10+ 人)、長期維護

推薦：**多專案結構**

\```
JobBank1111.Job.WebAPI/         # Controllers
JobBank1111.Job.Handler/        # Business Logic
JobBank1111.Job.Repository/     # Data Access
JobBank1111.Job.Contract/       # DTOs & Interfaces
JobBank1111.Job.DB/             # EF Core Entities
JobBank1111.Infrastructure/     # Cross-cutting
\```

優點：
- ✅ 職責清晰分離
- ✅ 便於團隊分工
- ✅ 易於單元測試

缺點：
- ⚠️ 編譯時間較長 (~30-60 秒)
- ⚠️ 專案間依賴管理
```

---

### D. TraceContext 整合建議

#### 詢問
```
1️⃣ 追蹤需求：
   - 請求追蹤
   - 用戶資訊追蹤
   - 分散式追蹤 (Distributed Tracing)
   - 審計日誌

2️⃣ 整合點：
   - Handler 層
   - Repository 層
   - 自訂中介軟體
```

#### 建議範例
```markdown
## TraceContext 整合架構

### 1. 定義 TraceContext (不可變物件)
\```csharp
public record TraceContext
{
    public string RequestId { get; init; }
    public string UserId { get; init; }
    public string UserName { get; init; }
    public DateTime RequestTime { get; init; }
}
\```

### 2. 中介軟體設定
\```csharp
public class TraceContextMiddleware
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        IContextSetter contextSetter)
    {
        var traceContext = new TraceContext
        {
            RequestId = httpContext.TraceIdentifier,
            UserId = GetUserId(httpContext),
            UserName = GetUserName(httpContext),
            RequestTime = DateTime.UtcNow
        };
        
        contextSetter.SetContext(traceContext);
        await _next(httpContext);
    }
}
\```

### 3. Handler 使用
\```csharp
public class MemberHandler
{
    private readonly IContextGetter _contextGetter;
    
    public async Task<Result> CreateAsync(CreateMemberRequest request)
    {
        var context = _contextGetter.GetContext();
        
        _logger.LogInformation(
            "Creating member, RequestId: {RequestId}, UserId: {UserId}",
            context.RequestId,
            context.UserId);
        
        // 業務邏輯...
    }
}
\```

### 優點
- ✅ 不可變，執行緒安全
- ✅ 解耦 HTTP 依賴
- ✅ 易於測試
```

---

### E. 分層架構設計建議

#### 建議原則
```markdown
## 分層職責定義

### Controller 層
**職責**：
- HTTP 請求/回應處理
- 路由定義
- 請求驗證
- HTTP 狀態碼對應

**禁止**：
- ❌ 業務邏輯
- ❌ 資料庫操作
- ❌ 複雜計算

---

### Handler 層
**職責**：
- 核心業務邏輯
- 流程協調
- 錯誤處理
- 結果封裝

**禁止**：
- ❌ HTTP 相關處理
- ❌ 直接資料庫操作

---

### Repository 層
**職責**：
- 資料存取邏輯
- EF Core 操作
- 查詢封裝

**禁止**：
- ❌ 業務邏輯
- ❌ 業務驗證
```

## 🚫 禁止行為
- ❌ 不可推薦違反分層原則的設計
- ❌ 不可建議在 Controller 寫業務邏輯
- ❌ 不可建議在 Repository 寫業務驗證
- ❌ 不可忽略團隊規模與專案特性就給建議

## ✅ 成功條件
- [x] 提供符合專案特性的建議
- [x] 說明建議的優缺點
- [x] 提供實作範例或參考
- [x] 考量團隊規模與維護性

## 📚 參考文件
- 架構設計指南: [@references/architecture-guide.md](../references/architecture-guide.md)
- TraceContext 指南: [@references/trace-context-guide.md](../references/trace-context-guide.md)

## 💡 使用範例

```bash
# 使用此 skill
@architecture 我需要設計訂單管理的 Repository，請給我建議

# AI 詢問範例
> 1️⃣ 專案規模：
>    a. 小型 (< 10 表)
>    b. 中型 (10-30 表)
>    c. 大型 (> 30 表)
> 
> 2️⃣ 訂單管理涉及哪些資料表？
>    例如：Orders, OrderItems, Payments, Shipments
> 
> 3️⃣ 主要業務流程：
>    例如：建立訂單、付款、出貨、取消訂單
```

## 🔗 相關 Skills
- `api-dev` - 使用架構建議實作 API
- `project-init` - 初始化時提供架構選擇
- `code-review` - 檢查是否符合架構原則

## 🔗 相關 Agents
- `project-setup-agent` - 專案設定（使用此 skill）
- `feature-dev-agent` - 功能開發（使用此 skill）
