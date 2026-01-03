# 架構設計指南

## 分層架構設計

### Controller → Handler → Repository 模式

#### Controller 層職責
- HTTP 請求/回應處理
- 路由定義
- 請求驗證
- HTTP 狀態碼對應
- **禁止**: 業務邏輯、資料庫操作

#### Handler 層職責
- 核心業務邏輯
- 流程協調
- 錯誤處理與結果封裝
- 呼叫 Repository 取得/儲存資料
- **禁止**: HTTP 相關處理、直接資料庫操作

#### Repository 層職責
- 資料存取邏輯
- EF Core 操作
- 資料庫查詢封裝
- **禁止**: 業務邏輯

## 專案組織方式

### 方案 A：單一專案結構
```
JobBank1111.Job.WebAPI/
├── Member/
│   ├── MemberController.cs
│   ├── MemberHandler.cs
│   └── MemberRepository.cs
├── Order/
│   ├── OrderController.cs
│   ├── OrderHandler.cs
│   └── OrderRepository.cs
```

**適用場景**:
- 小型團隊（3 人以下）
- 快速開發需求
- 專案規模較小

**優點**:
- 編譯快速
- 部署簡單
- 適合快速迭代

**缺點**:
- 程式碼耦合度較高
- 難以實現嚴格的分層隔離

### 方案 B：多專案結構
```
JobBank1111.Job.WebAPI/        # Controllers
JobBank1111.Job.Handler/       # Business Logic
JobBank1111.Job.Repository/    # Data Access
JobBank1111.Job.Contract/      # DTOs & Interfaces
```

**適用場景**:
- 大型團隊
- 明確分工需求
- 長期維護專案

**優點**:
- 職責清晰分離
- 便於團隊協作
- 易於單元測試

**缺點**:
- 專案結構較複雜
- 編譯時間較長

## Repository Pattern 設計哲學

### 核心原則：需求導向 > 資料表導向

#### ❌ 錯誤：資料表導向
```csharp
// 每個資料表一個 Repository
public class MemberRepository { }
public class OrderRepository { }
public class OrderItemRepository { }

// 問題：業務邏輯分散、跨表操作複雜
```

#### ✅ 正確：需求導向
```csharp
// 以業務需求劃分 Repository
public class MemberManagementRepository 
{
    // 處理會員相關的所有資料操作
    // 包含會員資料、會員等級、會員積分等
}

public class OrderManagementRepository 
{
    // 處理訂單相關的所有資料操作
    // 包含訂單、訂單明細、付款資訊等
}
```

### 設計策略選擇

#### 策略 A：簡單資料表導向
**適用時機**:
- 專案規模小（< 10 個資料表）
- 業務邏輯簡單
- 快速開發優先

#### 策略 B：業務需求導向（推薦）
**適用時機**:
- 專案規模中大（> 10 個資料表）
- 複雜業務邏輯
- 需要跨表操作
- 長期維護

#### 策略 C：混合模式（本專案採用）
**適用時機**:
- 核心業務用需求導向
- 簡單主檔用資料表導向
- 根據複雜度靈活調整

## TraceContext 設計模式

### 不可變物件設計
```csharp
public record TraceContext
{
    public string RequestId { get; init; }
    public string UserId { get; init; }
    public string UserName { get; init; }
    public DateTime RequestTime { get; init; }
}
```

### 依賴注入模式
```csharp
// 在 Middleware 中設定
public class TraceContextMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceContext = new TraceContext
        {
            RequestId = context.TraceIdentifier,
            UserId = GetUserId(context),
            UserName = GetUserName(context),
            RequestTime = DateTime.UtcNow
        };
        
        contextSetter.SetContext(traceContext);
        await _next(context);
    }
}

// 在 Handler/Repository 中注入使用
public class MemberHandler
{
    private readonly IContextGetter _contextGetter;
    
    public MemberHandler(IContextGetter contextGetter)
    {
        _contextGetter = contextGetter;
    }
    
    public async Task<Result<Member>> GetCurrentMember()
    {
        var context = _contextGetter.GetContext();
        // 使用 context.UserId 取得當前用戶資訊
    }
}
```

## 命名規範

### 檔案命名
- Controller: `{Feature}Controller.cs` 或 `{Feature}ControllerImpl.cs`
- Handler: `{Feature}Handler.cs`
- Repository: `{Feature}Repository.cs`

### DTO 命名
- Request: `{Action}{Feature}Request.cs`
- Response: `{Feature}Response.cs`
- 範例: `CreateMemberRequest.cs`, `MemberResponse.cs`

## 核心原則

1. **單一職責**: 每個類別只負責一件事
2. **依賴注入**: 透過建構子注入相依性
3. **不可變物件**: 使用 record 類型定義 DTO 與 TraceContext
4. **錯誤處理**: 使用 Result Pattern，不拋出例外
5. **分層隔離**: 業務邏輯不依賴 HTTP、資料庫實作細節

## 參考檔案位置

- Controller 範例: `src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs`
- Handler 範例: `src/be/JobBank1111.Job.WebAPI/Member/MemberHandler.cs`
- Repository 範例: `src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs`
- TraceContext: `src/be/JobBank1111.Job.WebAPI/TraceContext.cs`
- Middleware: `src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs`
