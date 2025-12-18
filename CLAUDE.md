# CLAUDE.md

此檔案為 Claude Code (claude.ai/code) 在此專案中工作時的指導文件。
接下來的回覆、文件描述，均使用台灣用語的繁體中文

**📝 注意**: 本文件著重於概念、原則和規範說明。具體程式碼實作請參考專案內的實際檔案。

## 目錄 (Table of Contents)

### 核心指引
- [AI 助理使用規則](#ai-助理使用規則)
  - [專案狀態檢測機制](#專案狀態檢測機制)
- [開發指令](#開發指令)
- [架構概述](#架構概述)
  - [分層架構](#分層架構)
  - [技術堆疊](#技術堆疊)

### 開發實踐
- [BDD 開發流程](#bdd-開發流程-行為驅動開發)
- [核心開發原則](#核心開發原則)
- [專案最佳實踐](#專案最佳實踐-best-practices)

### 技術深入
- [追蹤內容管理 (TraceContext)](#追蹤內容管理-tracecontext)
- [錯誤處理與回應管理](#錯誤處理與回應管理)
- [中介軟體架構與實作](#中介軟體架構與實作)
- [效能最佳化與快取策略](#效能最佳化與快取策略)
- [API 設計與安全性](#api-設計與安全性強化)

### 營運與部署
- [監控與可觀測性](#監控與可觀測性)
- [容器化與部署](#容器化與部署最佳實務)

---

## AI 助理使用規則

### 核心互動原則

AI 助理在與使用者互動時，必須遵循以下核心原則：

1. **強制互動確認**
   - **Claude CLI**: 使用 `AskUserQuestion` 工具進行結構化詢問
   - **GitHub Copilot CLI / Cursor / 其他 AI**: 使用結構化的文字列表詢問
   - 在所有需要使用者決策的情境下，都必須明確詢問，不得擅自執行
   - 提供清晰的選項說明，幫助使用者做出明智選擇
   - 在執行任何有風險或需要使用者決策的操作前，你**必須**先向我提問，並等待我的確認。以結構化的清單（例如 1️⃣, 2️⃣, 3️⃣ 或 a, b, c）提

2. **不得擅自假設**
   - 即使文件標註「預設」值，仍須詢問使用者確認
   - 例外：使用者已在對話中明確指定（如「使用 SQL Server」）
   - 所有 AI 助理都必須遵守此原則

3. **分階段互動**
   - 單次詢問最多 3-4 個問題，避免資訊過載
   - 複雜流程應分階段進行，根據前一階段的回答決定後續問題

4. **完整性優先**
   - 必須收集所有必要資訊後才開始執行
   - 不可因「加速開發」而省略必要的互動步驟

### 專案狀態檢測機制

當 AI 助理首次接觸此專案時，**必須優先檢測專案狀態**：

#### 檢測條件（滿足以下任一條件視為空白專案）
1. **不存在** `env/.template-config.json` 配置檔案
2. **不存在** `.sln` 解決方案檔案
3. **不存在** `src/` 目錄或該目錄為空
4. **不存在** `appsettings.json` 或 `docker-compose.yml`

#### 檢測流程
```mermaid
graph TD
    A[AI 助理啟動] --> B{檢查 env/.template-config.json}
    B -->|存在| C{驗證專案結構完整性}
    B -->|不存在| D[觸發初始化對話]
    C -->|完整| E[正常工作模式]
    C -->|不完整| D
    D --> T{是否使用 GitHub 範本\nhttps://github.com/yaochangyu/api.template}
    T -->|是| U[git clone 範本到工作目錄]
    U --> V[刪除 .git（移除 Git 歷史/遠端設定）]
    V --> F[詢問配置]
    T -->|否| F
    F --> G[根據回答修改/產生專案結構]
    G --> H[儲存配置到 env/.template-config.json]
    H --> E
```

#### GitHub 範本套用規則（初始化時）

當專案狀態檢測判定為「空白專案」時，初始化對話的第一個問題必須先詢問：

- 是否要使用 https://github.com/yaochangyu/api.template 作為專案範本

若使用者選擇「是」，AI 助理必須遵循：

1. **安全檢查（不得擅自覆蓋）**：
   - 僅在「工作目錄為空」或使用者已明確同意覆蓋/清空時，才可執行 clone。
   - 若工作目錄非空，必須先詢問使用者要「改用子資料夾」或「取消」。
2. **使用 git clone 下載範本**：
   - Windows PowerShell 範例（在空目錄中）：`git clone https://github.com/yaochangyu/api.template .`
3. **刪除 Git 相關資料**：
   - 刪除 `.git/` 目錄（移除歷史與遠端設定）。
   - Windows PowerShell 範例：`Remove-Item -Recurse -Force .git`
4. **接著才進入本專案的互動式配置**（資料庫/快取/專案結構等），並依照互動結果修改專案內容與寫入 `env/.template-config.json`。

#### 配置檔案格式（env/.template-config.json）
```json
{
  "database": {
    "type": "SQL Server",
    "version": "2022",
    "useEfCore": true
  },
  "cache": {
    "useRedis": true,
    "version": "7-alpine"
  },
  "projectOrganization": "single-project",
  "createdAt": "2025-12-15T14:22:22.741Z",
  "createdBy": "Claude CLI"
}
```

### 強制詢問情境

#### 1. 專案初始化與配置
- 資料庫類型選擇
- Redis 快取需求
- 專案結構組織方式（單一專案 vs 多專案）
- 是否使用 GitHub 範本（https://github.com/yaochangyu/api.template）

#### 2. 資料庫相關操作
- Code First vs Database First 模式選擇
- Migration 名稱與套用策略
- 資料表範圍選擇

#### 3. 功能實作

當使用者要求實作新功能時，**必須優先詢問**：

**a) API 開發流程選擇**
- ✅ **API First（推薦）**：先定義 OpenAPI 規格 (doc/openapi.yml)，再透過 `task codegen-api-server` 產生 Controller 骨架，確保 API 契約優先、文件與實作同步
- ✅ **Code First**：直接實作程式碼，後續手動維護 OpenAPI 規格或透過程式碼註解產生文件

**b) OpenAPI 規格定義狀態**（僅當選擇 API First 時詢問）
- 已定義：doc/openapi.yml 已包含此 API 規格定義
- 需要更新：需要修改 doc/openapi.yml 加入新的 endpoint
- 尚未定義：需要從頭建立 OpenAPI 規格

**c) 需要實作的分層**
- Controller：HTTP 請求處理與路由
- Handler：業務邏輯處理與流程協調
- Repository：資料存取與資料庫操作

**d) 測試需求與範圍**（詳見下方測試策略詢問）

#### 4. 測試策略詢問
當實作新功能或修改現有功能時，**必須詢問**使用者：

**a) 是否需要實作測試？**
- ✅ 是，需要實作完整測試（BDD 整合測試 + 單元測試）
- ✅ 是，僅需要 BDD 整合測試
- ✅ 是，僅需要單元測試
- ❌ 否，暫不實作測試（例如：快速原型、POC 驗證）

**b) 如果需要測試，測試範圍為何？**
- 新增功能的完整測試
- 僅測試核心業務邏輯
- 僅測試關鍵路徑（Happy Path）
- 包含異常情境與邊界條件

**c) BDD 測試情境**（如果選擇 BDD 測試）
- 是否已有 `.feature` 檔案？
- 需要新增哪些情境（Given-When-Then）？
- 是否需要 AI 協助撰寫 Gherkin 語法？

**d) 測試資料準備策略**
- 使用 Docker 容器（資料庫、Redis）
- 使用固定測試資料（Seed Data）
- 每次測試動態產生資料
- 測試後是否需要清理資料？

**e) 測試方法選擇**
- ✅ **API 端點測試必須使用 BDD 測試方法**（透過 Reqnroll 實作 .feature 檔案）
- ✅ **測試替身優先順序**：
  1. 優先使用 Testcontainers（Docker 容器）作為資料庫、Redis 的測試替身
  2. 僅在無法使用 Testcontainers 時才考慮使用 Mock（例如：第三方 API、外部服務）
- ✅ **禁止對 Controller 進行單元測試**：所有 API 測試必須透過完整的 Web API 管線執行

**測試決策範例**：
```markdown
使用者請求：「實作會員註冊功能」

AI 應詢問：
1. 是否需要實作測試？
   - [ ] 完整測試（BDD + 單元測試）
   - [ ] 僅 BDD 整合測試
   - [ ] 僅單元測試
   - [ ] 暫不實作

2. 如果需要 BDD 測試，情境包含：
   - [ ] 成功註冊新會員
   - [ ] 重複 Email 註冊失敗
   - [ ] 無效 Email 格式驗證
   - [ ] 必填欄位驗證

3. 測試環境：
   - [ ] 使用 Testcontainers (Docker SQL Server 容器)
   - [ ] 使用 Testcontainers (Docker Redis 容器)
   - [ ] 每次測試後清理資料
   
4. 測試方法：
   - [x] API 端點使用 BDD 測試（.feature 檔案）
   - [x] 優先使用 Testcontainers 作為測試替身
   - [ ] 僅在必要時使用 Mock（例如：外部 API）
```

#### 5. 效能最佳化
- 優化面向選擇（資料庫查詢/快取策略/非同步處理/記憶體使用）

### 禁止的行為 ❌
1. **擅自使用預設值** - 必須明確詢問使用者選擇
2. **跳過詢問步驟** - 即使有推薦選項,仍須確認
3. **一次詢問過多問題** - 每次最多 3-4 個問題
4. **提供不明確的選項** - 必須加入說明
5. **假設測試需求** - 不可假設使用者需要或不需要測試，必須明確詢問
6. **跳過測試實作詢問** - 實作新功能時必須詢問測試策略

---

## 開發指令

### Taskfile 使用原則
- **優先使用 Taskfile**: 所有重複執行的開發指令應盡可能透過 `task` 命令執行
- **命令集中管理**: 複雜的多步驟指令應寫入 `Taskfile.yml`
- **提醒與建議**: 在建議執行長指令時，應提醒用戶「建議將此命令添加到 Taskfile.yml」

### 常用指令
- **開發模式執行 API**: `task api-dev`
- **建置解決方案**: `task build`
- **執行單元測試**: `task test-unit`
- **執行整合測試**: `task test-integration`
- **產生 API 程式碼**: `task codegen-api`
- **從資料庫反向工程產生實體**: `task ef-codegen`
- **建立新的 Migration**: `task ef-migration-add NAME=<MigrationName>`
- **更新資料庫**: `task ef-database-update`

**重要**: EF Core 相關指令必須透過 Taskfile 執行，不應直接執行 `dotnet ef` 指令。

---

## 架構概述

### 核心專案
- **JobBank1111.Job.WebAPI**: 主要的 Web API 應用程式
- **JobBank1111.Infrastructure**: 跨領域基礎設施服務
- **JobBank1111.Job.DB**: Entity Framework Core 資料存取層
- **JobBank1111.Job.Contract**: 從 OpenAPI 規格自動產生的 API 客戶端合約

### 分層架構

#### 分層模式（Controller → Handler → Repository）
- **Controller 層**: HTTP 請求/回應、路由、請求驗證、HTTP 狀態碼對應
- **Handler 層**: 核心業務邏輯、流程協調、錯誤處理與結果封裝
- **Repository 層**: 資料存取邏輯、EF Core 操作、資料庫查詢封裝

#### 組織方式

**方案 A：單一專案結構**
- 所有功能層都在 `JobBank1111.Job.WebAPI` 專案內
- 適合小型團隊（3 人以下）、快速開發
- 優點：編譯快速、部署簡單
- 缺點：程式碼耦合度較高

**方案 B：多專案結構**
- Controller、Handler、Repository 各自獨立專案
- 適合大型團隊、明確分工、長期維護
- 優點：職責清晰分離、便於團隊協作
- 缺點：專案結構較複雜、編譯時間較長

**📝 實作參考**:
- Controller 範例：[src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs)
- Handler 範例：[src/be/JobBank1111.Job.WebAPI/Member/MemberHandler.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberHandler.cs)
- Repository 範例：[src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs)

### 技術堆疊
- **框架**: ASP.NET Core 8.0
- **資料庫**: Entity Framework Core 與 SQL Server
- **快取**: Redis 搭配記憶體內快取備援
- **錯誤處理**: CSharpFunctionalExtensions 3.1.0 (Result Pattern)
- **驗證**: FluentValidation 11.10.0
- **日誌記錄**: Serilog 結構化日誌
- **測試**: xUnit 2.9.2、Testcontainers 3.10.0、Reqnroll.xUnit 2.1.1 (BDD)
- **API 文件**: Swagger/OpenAPI 搭配 ReDoc 與 Scalar 檢視器

### 程式碼產生工作流程
1. API 規格維護在 `doc/openapi.yml`
2. 使用 Refitter 產生客戶端程式碼至 `JobBank1111.Job.Contract`
3. 使用 NSwag 產生伺服器控制器至 `JobBank1111.Job.WebAPI/Contract`
4. 使用 EF Core 反向工程產生資料庫實體至 `JobBank1111.Job.DB`

**重要規範**: 
- 所有自動產生的程式碼都放在 `AutoGenerated` 資料夾中，不可手動編輯
- EF Core 反向工程與 Migration 必須透過 Taskfile 執行

---

## BDD 開發流程 (行為驅動開發)

專案採用 BDD 開發模式，使用 Docker 容器作為測試替身，確保需求、測試與實作的一致性。

### BDD 開發循環

#### 1. 需求分析階段
使用 Gherkin 語法定義功能情境，參考：[src/be/JobBank1111.Job.IntegrationTest/_01_Demo/](src/be/JobBank1111.Job.IntegrationTest/_01_Demo/) 目錄下的 `.feature` 檔案。

#### 2. 測試實作階段
使用 Reqnroll 與真實 Docker 服務實作測試步驟，參考測試步驟實作檔案。

#### 3. Docker 測試環境
完全基於 Docker 的測試環境，避免使用 Mock。包含：
- SQL Server 容器
- Redis 容器
- Seq 日誌容器

📝 **測試環境設定參考**: [src/be/JobBank1111.Job.IntegrationTest/TestServer.cs](src/be/JobBank1111.Job.IntegrationTest/TestServer.cs)

### Docker 優先測試策略

#### 核心原則
- **真實環境**: 使用 Docker 容器提供真實的資料庫、快取、訊息佇列等服務
- **避免 Mock**: 只有在無法使用 Docker 替身的外部服務才考慮 Mock
- **隔離測試**: 每個測試使用獨立的資料，測試完成後自動清理
- **並行執行**: 利用 Docker 容器的隔離特性支援測試並行執行

📝 **測試輔助工具參考**: [src/be/JobBank1111.Job.IntegrationTest/TestAssistant.cs](src/be/JobBank1111.Job.IntegrationTest/TestAssistant.cs)

### API 控制器測試指引

#### 核心原則
- **BDD 優先**: 所有控制器功能必須優先使用 BDD 情境測試
- **禁止單獨測試控制器**: 不應直接實例化控制器進行單元測試
- **強制使用 WebApplicationFactory**: 所有測試必須透過完整的 Web API 管線與 Docker 測試環境
- **情境驅動開發**: 從使用者行為情境出發

---

## 核心開發原則

### 不可變物件設計 (Immutable Objects)
- 使用 C# record 類型定義不可變物件，例如 `TraceContext`
- 所有屬性使用 `init` 關鍵字
- 避免在應用程式各層間傳遞可變狀態

📝 **TraceContext 實作參考**: [src/be/JobBank1111.Job.WebAPI/TraceContext.cs](src/be/JobBank1111.Job.WebAPI/TraceContext.cs)

### 架構守則
- 業務邏輯層不應直接處理 HTTP 相關邏輯
- 所有跨領域關注點（如身分驗證、日誌、追蹤）應在中介軟體層處理
- 使用不可變物件傳遞狀態
- 透過 DI 容器注入 TraceContext

### 用戶資訊管理
- **不可變性原則**: 確保物件的不可變，身分驗證後的用戶資訊存放在 TraceContext
- **集中處理**: 集中在 TraceContextMiddleware 處理
- **依賴注入**: 透過 IContextSetter 設定用戶資訊，透過 IContextGetter 取得

📝 **中介軟體實作參考**: [src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs](src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs)

---

## 專案最佳實踐 (Best Practices)

### 1. 程式碼組織與命名規範

#### 命名規範
- **Handler**: `{Feature}Handler.cs`
- **Repository**: `{Feature}Repository.cs`
- **Controller**: `{Feature}Controller.cs` 或 `{Feature}ControllerImpl.cs`
- **Request/Response DTO**: `{Action}{Feature}Request.cs` / `{Feature}Response.cs`

### 2. Repository Pattern 設計哲學

#### 核心原則：以需求為導向，而非資料表

**❌ 錯誤的思維：資料表導向**
```
資料表: Members, Orders, OrderItems
Repository: MemberRepository, OrderRepository, OrderItemRepository
問題: 業務邏輯分散、跨表操作複雜、難以維護
```

**✅ 正確的思維：需求導向**
```
業務需求: 會員管理、訂單處理、庫存管理
Repository: MemberRepository, OrderManagementRepository, InventoryRepository
優點: 封裝完整業務邏輯、減少跨層呼叫、更易維護
```

#### 設計策略選擇

**策略 A：簡單資料表導向（適合小型專案）**
- 專案規模小（< 10 個資料表）
- 業務邏輯簡單
- 團隊人數少（1-3 人）
- 快速開發優先
- **範例**: `MemberRepository` 對應 `Members` 資料表

**策略 B：業務需求導向（推薦用於中大型專案）**
- 專案規模中等以上（> 10 個資料表）
- 複雜業務邏輯
- 需要跨表操作
- 長期維護考量
- **範例**: `OrderManagementRepository` 處理訂單、訂單明細、付款等相關操作

**策略 C：混合模式（實務常見）**
- 核心業務使用需求導向（如訂單處理）
- 簡單主檔使用資料表導向（如會員、產品）
- 根據複雜度靈活調整
- **本專案採用此策略**

#### 實務範例對比

**資料表導向 Repository**
```csharp
// ❌ 問題：業務邏輯分散在多個 Repository 和 Handler
public class OrderRepository { /* 只處理 Orders 表 */ }
public class OrderItemRepository { /* 只處理 OrderItems 表 */ }
public class PaymentRepository { /* 只處理 Payments 表 */ }

// Handler 需要協調多個 Repository
public class OrderHandler(
    OrderRepository orderRepo,
    OrderItemRepository itemRepo,
    PaymentRepository paymentRepo)
{
    public async Task<Result> CreateOrder(...)
    {
        // 複雜的跨 Repository 協調邏輯
        await orderRepo.InsertAsync(...);
        await itemRepo.BulkInsertAsync(...);
        await paymentRepo.InsertAsync(...);
    }
}
```

**需求導向 Repository**
```csharp
// ✅ 優勢：封裝完整的業務操作
public class OrderManagementRepository
{
    public async Task<Result<OrderDetail>> CreateCompleteOrderAsync(
        CreateOrderRequest request, 
        CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);
        
        try
        {
            // 1. 建立訂單主檔
            var order = new Order { ... };
            dbContext.Orders.Add(order);
            
            // 2. 建立訂單明細
            var items = request.Items.Select(i => new OrderItem { ... });
            dbContext.OrderItems.AddRange(items);
            
            // 3. 建立付款記錄
            var payment = new Payment { ... };
            dbContext.Payments.Add(payment);
            
            // 4. 更新庫存
            foreach (var item in request.Items)
            {
                var product = await dbContext.Products.FindAsync(item.ProductId);
                product.Stock -= item.Quantity;
            }
            
            await dbContext.SaveChangesAsync(cancel);
            await transaction.CommitAsync(cancel);
            
            return Result.Success<OrderDetail, Failure>(orderDetail);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancel);
            return Result.Failure<OrderDetail, Failure>(new Failure { ... });
        }
    }
    
    public async Task<Result<OrderDetail>> GetOrderDetailAsync(Guid orderId, CancellationToken cancel = default)
    {
        // 一次查詢取得完整訂單資訊（訂單 + 明細 + 付款）
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        
        var orderDetail = await dbContext.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDetail
            {
                Order = o,
                Items = o.OrderItems.ToList(),
                Payment = o.Payment
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancel);
            
        return Result.Success<OrderDetail, Failure>(orderDetail);
    }
}

// Handler 變得非常簡潔
public class OrderHandler(OrderManagementRepository orderRepo)
{
    public async Task<Result<OrderDetail>> CreateOrder(CreateOrderRequest request, CancellationToken cancel)
    {
        // 直接呼叫 Repository 的業務方法
        return await orderRepo.CreateCompleteOrderAsync(request, cancel);
    }
}
```

#### 命名規範建議

**資料表導向命名**
- `{TableName}Repository` - 例如：`MemberRepository`, `ProductRepository`
- 適用於簡單 CRUD 操作

**需求導向命名**
- `{BusinessDomain}Repository` - 例如：`OrderManagementRepository`, `InventoryRepository`
- `{AggregateRoot}Repository` - 例如：`ShoppingCartRepository`, `UserAccountRepository`
- 適用於複雜業務邏輯

#### 設計決策檢查清單

在設計 Repository 時，應詢問自己：

**✅ 需求導向的判斷標準**
- [ ] 此業務操作涉及 3 個以上資料表？
- [ ] 操作需要交易一致性保證？
- [ ] 業務邏輯複雜，需要多步驟協調？
- [ ] 多個 API 端點共用此業務邏輯？
- [ ] 未來可能擴展更多相關功能？

**如果以上有 2 個以上為「是」，建議使用需求導向 Repository**

**❌ 資料表導向的適用場景**
- [ ] 僅單一資料表的簡單 CRUD
- [ ] 無複雜業務邏輯
- [ ] 不需要跨表操作
- [ ] 查詢條件簡單明確

#### 本專案的實作策略

本專案採用**混合模式**：
- **簡單主檔**：使用資料表導向（如 `MemberRepository`）
- **複雜業務**：視需求採用業務導向（如未來的訂單管理）
- **靈活調整**：根據實際需求演進

**重要原則**: 
- 設計初期可以從簡單的資料表導向開始
- 當發現業務邏輯分散、難以維護時，重構為需求導向
- 不要過度設計，根據實際複雜度調整

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs)

### 3. 依賴注入最佳實踐

#### 主建構函式注入 (Primary Constructor)
使用 C# 12 的主建構函式簡化依賴注入，直接使用參數名稱，無需宣告欄位。

#### DbContextFactory 模式
使用 `IDbContextFactory<T>` 而非直接注入 `DbContext`，避免生命週期問題。

### 4. 非同步程式設計最佳實踐

#### 核心原則
- 所有 I/O 操作都必須使用 async/await
- 所有非同步方法都應支援 CancellationToken
- 避免使用 `.Result` 或 `.Wait()`（死鎖風險）

### 5. EF Core 查詢最佳化
- 使用 `AsNoTracking()` 提升唯讀查詢效能
- 使用 `Include` 或 `Join` 避免 N+1 查詢問題
- 適當使用分頁查詢

### 6. 快取策略最佳實踐

#### 快取鍵命名規範
- 使用冒號分隔命名空間：`{feature}:{operation}:{parameters}`
- 範例：`members:page:0:10`, `member:email:test@example.com`

📝 **快取實作參考**: [src/be/JobBank1111.Infrastructure/Caching/](src/be/JobBank1111.Infrastructure/Caching/)

### 7. 日誌記錄最佳實踐

#### 集中式日誌策略
**核心原則**: 日誌記錄集中在 Middleware 層，業務邏輯層不記錄錯誤日誌，只回傳 Failure。

#### 結構化日誌格式
使用 Serilog 的結構化日誌，自動包含 TraceId。

📝 **中介軟體實作參考**: 
- [src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs](src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs)
- [src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs](src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs)

### 8. 安全最佳實踐

#### 機敏設定管理
**核心原則**: 不要在 `appsettings.json` 儲存機密。

- ❌ **禁止**: 在 `appsettings.json` 放入連線字串、金鑰、權杖
- ✅ **改用**: 環境變數、.NET User Secrets（本機）、Docker Secrets（容器）、雲端祕密管家

### 9. 程式碼產生與維護

**核心原則**: 所有自動產生的程式碼都放在 `AutoGenerated` 資料夾中，不可手動編輯。

### 10. 開發工作流程

#### 標準開發流程
```
1. 需求分析
   ↓
2. 【互動】選擇 API 開發流程
   - API First（推薦）：先定義 OpenAPI 規格，再產生 server code
   - Code First：直接實作程式碼
   ↓
3. 【互動】詢問測試策略與範圍
   - 是否需要測試？
   - 測試類型（BDD/單元測試/兩者）
   - 測試範圍與情境
   ↓
4. 撰寫 BDD 情境 (.feature 檔案) - 如果需要 BDD 測試
   ↓
5a. API First 流程:
   ├→ 更新 OpenAPI 規格 (doc/openapi.yml)
   ├→ 產生 Server 程式碼 (task codegen-api-server)
   └→ 產生 Client 程式碼 (task codegen-api-client)
   ↓
5b. Code First 流程:
   └→ 直接實作程式碼（後續手動更新 OpenAPI）
   ↓
6. 實作 Handler 業務邏輯
   ↓
7. 實作 Repository 資料存取
   ↓
8. 實作 BDD 測試步驟 - 如果需要 BDD 測試
   ↓
9. 實作單元測試 - 如果需要單元測試
   ↓
10. 執行測試 (task test-integration / task test-unit)
   ↓
11. 手動測試 (Scalar UI)
   ↓
12. Code Review 與合併
```

**重要提醒**：
- 步驟 2 的 API 開發流程選擇是**強制性**的，不可跳過
- 步驟 3 的測試策略詢問是**強制性**的，不可跳過
- 根據使用者的選擇決定執行 5a 或 5b
- 根據使用者的測試選擇決定是否執行步驟 4、8、9、10
- 如果使用者選擇「暫不實作測試」，應跳過測試相關步驟，但需在 Code Review 時提醒

#### API First 開發流程詳解

**核心理念**：先定義 API 契約（OpenAPI 規格），再產生程式碼骨架，確保：
- ✅ API 文件與實作 100% 同步
- ✅ 前後端可以並行開發（基於相同契約）
- ✅ 減少溝通成本與理解偏差
- ✅ 自動產生 Client SDK

**完整流程範例**：

```mermaid
graph TD
    A[需求分析] --> B[定義 OpenAPI 規格]
    B --> C[產生 Server Controller 骨架]
    C --> D[實作 Handler 業務邏輯]
    D --> E[實作 Repository 資料存取]
    E --> F[執行 BDD 測試]
    F --> G{測試通過?}
    G -->|否| D
    G -->|是| H[產生 Client SDK]
    H --> I[前端整合]
    
    style B fill:#90EE90
    style C fill:#87CEEB
    style H fill:#FFB6C1
```

**步驟 1: 定義 OpenAPI 規格**

📝 **專案 OpenAPI 規格檔案**：[doc/openapi.yml](doc/openapi.yml)

在現有規格中新增或修改 API 端點定義，包含：
- HTTP 方法與路徑
- 請求/回應的 Schema 定義
- 錯誤回應格式
- 參數驗證規則

**步驟 2: 產生 Server Controller 骨架**

執行命令產生 Controller 介面：
```bash
task codegen-api-server
```

產生位置：`JobBank1111.Job.WebAPI/Contract/AutoGenerated/`

**步驟 3: 實作 Controller**

📝 **Controller 實作參考**：
- [src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs)

實作自動產生的介面，整合 Handler 業務邏輯，處理 Result Pattern 回應轉換。

**步驟 4: 產生 Client SDK（前端使用）**

執行命令產生 Client：
```bash
task codegen-api-client
```

產生位置：`JobBank1111.Job.Contract/AutoGenerated/`

前端專案可直接引用自動產生的強型別 Client，享受完整的 IntelliSense 與編譯時檢查。

**API First vs Code First 對比**：

| 比較項目 | API First（推薦） | Code First |
|---------|------------------|-----------|
| **文件同步** | ✅ 自動 100% 同步 | ❌ 需手動維護 |
| **前後端協作** | ✅ 可並行開發 | ⚠️ 需等後端完成 |
| **契約保證** | ✅ 編譯時檢查 | ❌ 執行時才發現 |
| **Client SDK** | ✅ 自動產生 | ❌ 需手動實作 |
| **開發速度** | ⚠️ 需先設計 API | ✅ 快速啟動 |
| **適用場景** | 中大型專案、團隊協作 | 小型專案、快速原型 |

**何時選擇 API First**：
- ✅ 前後端分離且團隊並行開發
- ✅ 需要提供 Client SDK 給第三方
- ✅ API 穩定性要求高
- ✅ 多個客戶端（Web、Mobile、Desktop）

**何時選擇 Code First**：
- ✅ 快速原型驗證
- ✅ 內部小型專案
- ✅ API 結構仍在快速變動中
- ✅ 單人開發或小團隊

### 11. 常見錯誤與陷阱

#### ❌ 禁止的模式
1. **直接測試 Controller** - 必須透過 BDD 情境測試
2. **不使用 Result Pattern** - 不要拋出業務邏輯例外
3. **未保存原始例外** - 必須將例外寫入 `Failure.Exception`
4. **忘記傳遞 CancellationToken** - 所有非同步方法都應支援
5. **過度設計 Repository** - 從簡單開始，需要時再重構為需求導向
6. **Repository 中實作業務規則** - 複雜業務邏輯應在 Handler 層處理

---

## 追蹤內容管理 (TraceContext)

### 集中式管理架構
- **統一處理點**: 所有追蹤內容與使用者資訊統一在 `TraceContextMiddleware` 中處理
- **不可變性**: `TraceContext` 使用 record 定義，包含 `TraceId` 與 `UserId` 等不可變屬性
- **身分驗證整合**: 在 `TraceContextMiddleware` 中統一處理使用者身分驗證

### 生命週期與服務注入
- **生命週期**: 透過 `AsyncLocal<T>` 機制確保 TraceContext 在整個請求生命週期內可用
- **服務注入**: 使用 `IContextGetter<T>` 與 `IContextSetter<T>` 介面進行依賴注入
- **TraceId 處理**: 從請求標頭擷取或自動產生 TraceId
- **回應標頭**: 自動將 TraceId 加入回應標頭供追蹤使用

📝 **實作參考**: 
- [src/be/JobBank1111.Job.WebAPI/TraceContext.cs](src/be/JobBank1111.Job.WebAPI/TraceContext.cs)
- [src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs](src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs)

---

## 錯誤處理與回應管理

### Result Pattern 設計

#### 核心架構
- **Result 套件**: 使用 `CSharpFunctionalExtensions` 3.1.0 套件
- **應用範圍**: Repository 層和 Handler 層必須使用 `Result<TSuccess, TFailure>` 作為回傳類型
- **映射規則**: 使用 `FailureCodeMapper` 將錯誤代碼映射至 HTTP 狀態碼

#### FailureCode 列舉
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

#### Failure 物件結構
- **Code**: 錯誤代碼
- **Message**: 例外的原始訊息
- **TraceId**: 追蹤識別碼
- **Exception**: 原始例外物件（不序列化到客戶端）
- **Data**: 結構化資料

📝 **實作參考**: 
- [src/be/JobBank1111.Job.WebAPI/Failure.cs](src/be/JobBank1111.Job.WebAPI/Failure.cs)
- [src/be/JobBank1111.Job.WebAPI/FailureCode.cs](src/be/JobBank1111.Job.WebAPI/FailureCode.cs)
- [src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs](src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs)

### 分層錯誤處理策略

#### 業務邏輯錯誤處理 (Handler 層)
- 使用 Result Pattern 處理預期的業務邏輯錯誤
- 回傳適當的 HTTP 狀態碼 (400, 401, 404, 409 等)
- 不應讓業務邏輯錯誤流到系統例外處理層

#### 系統層級例外處理 (ExceptionHandlingMiddleware)
- 僅捕捉未處理的系統層級例外
- 使用結構化日誌記錄例外詳細資訊
- 將系統例外轉換為標準化的 `Failure` 物件回應
- 統一設定為 500 Internal Server Error

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs](src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs)

### 錯誤處理最佳實務原則
- **不要重複拋出例外**: 處理過的例外不應再次 throw
- **統一錯誤碼**: 使用 `nameof(FailureCode.*)` 定義錯誤碼
- **例外封裝規則**: 所有捕捉到的例外都必須寫入 `Failure.Exception` 屬性
- **包含追蹤資訊**: 確保所有 Failure 物件都包含 TraceId
- **安全回應**: 不洩露內部實作細節給客戶端
- **分離關注點**: 業務錯誤與系統例外分別在不同層級處理
- **載體日誌職責**: 業務邏輯層不記錄錯誤日誌，由 Middleware 記錄

---

## 中介軟體架構與實作

### 中介軟體管線架構與職責

#### 管線順序與責任劃分
1. **MeasurementMiddleware**: 最外層，度量與計時，包覆整體請求耗時
2. **ExceptionHandlingMiddleware**: 捕捉未處理的系統層級例外，統一回應格式
3. **TraceContextMiddleware**: 設定追蹤內容與身分資訊（如 TraceId、UserId）
4. **RequestParameterLoggerMiddleware**: 在管線尾端於成功完成時記錄請求參數

🧩 程式碼為準（Program.cs）
```csharp
// 管線順序：Measurement → ExceptionHandling → TraceContext → RequestParameterLogger
app.UseMiddleware<MeasurementMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TraceContextMiddleware>();
app.UseMiddleware<RequestParameterLoggerMiddleware>();
```

#### 職責分離原則
- **例外處理**: 僅在 `ExceptionHandlingMiddleware` 捕捉系統例外
- **追蹤管理**: 所有 TraceContext 相關處理集中在 `TraceContextMiddleware`
- **日誌記錄**: 分別在例外情況和正常完成時記錄，避免重複
- **請求資訊**: 使用 `RequestInfoExtractor` 統一擷取請求參數

📝 **實作參考**: 
- [src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs](src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs)
- [src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs](src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs)
- [src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs](src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs)
- [src/be/JobBank1111.Job.WebAPI/RequestInfoExtractor.cs](src/be/JobBank1111.Job.WebAPI/RequestInfoExtractor.cs)

### RequestInfoExtractor 功能
1. **路由參數**: 擷取 URL 路由中的參數
2. **查詢參數**: 擷取 URL 查詢字串參數
3. **請求標頭**: 擷取 HTTP 標頭，自動排除敏感標頭
4. **請求本文**: 對於 POST/PUT/PATCH 請求，擷取請求本文內容並嘗試解析 JSON
5. **基本資訊**: 記錄 HTTP 方法、路徑、內容類型、內容長度等

### 中介軟體最佳實務原則
- **專一職責**: 每個中介軟體專注於單一關注點
- **避免重複**: 透過管線設計避免重複處理和記錄
- **統一格式**: 所有請求資訊記錄使用相同的資料結構
- **效能考量**: 只有在需要時才擷取請求本文
- **錯誤容錯**: 記錄過程中發生錯誤不影響業務邏輯執行

---

## 效能最佳化與快取策略

### 快取架構設計

#### 多層快取策略
- **L1 快取 (記憶體內快取)**: 使用 `IMemoryCache` 存放頻繁存取的小型資料
- **L2 快取 (分散式快取)**: 使用 Redis 作為分散式快取，支援多實例共用
- **快取備援**: 當 Redis 不可用時，自動降級至記憶體快取
- **快取預熱**: 應用程式啟動時預載常用資料

📝 **快取實作參考**: [src/be/JobBank1111.Infrastructure/Caching/](src/be/JobBank1111.Infrastructure/Caching/)

#### 快取失效與管理策略
- **時間過期 (TTL)**: 設定合理的快取過期時間
- **版本控制**: 使用版本號管理快取一致性
- **標籤快取**: 支援批次清除相關快取項目
- **事件驅動**: 資料異動時主動清除對應快取

### ASP.NET Core 效能最佳化

#### 核心原則
- **連線池**: 使用 `AddDbContextPool` 重用 DbContext 實例
- **查詢最佳化**: 使用 `AsNoTracking()` 避免不必要的異動追蹤
- **批次操作**: 使用 `BulkInsert` / `BulkUpdate` 處理大量資料
- **非同步程式設計**: 使用 `ConfigureAwait(false)` 避免死鎖

### 記憶體管理與垃圾收集
- **物件池**: 使用 `ObjectPool<T>` 重用昂貴物件
- **Span<T> 與 Memory<T>**: 減少記憶體配置的現代化 API
- **字串最佳化**: 使用 `StringBuilder` 與字串插值最佳化

---

## API 設計與安全性強化

### RESTful API 設計原則

#### API 版本控制策略
支援 URL 路徑版本控制與標頭版本控制。

#### 內容協商與媒體類型
- **Accept 標頭處理**: 支援多種回應格式 (JSON, XML)
- **內容壓縮**: 自動 Gzip/Brotli 壓縮
- **API 文件**: 整合 Swagger/OpenAPI 3.0 規格

📝 **API 規格參考**: [doc/openapi.yml](doc/openapi.yml)

### API 安全性防護

#### 輸入驗證與清理
使用 FluentValidation 或 DataAnnotations 進行模型驗證，防止 SQL Injection、XSS 等攻擊。

#### CORS 與跨來源安全
根據環境設定不同的 CORS 政策，生產環境限制允許的來源。

#### HTTPS 強制與安全標頭
- HTTPS 重新導向與 HSTS
- 安全標頭：X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, CSP

#### API 限流與頻率控制
使用 AspNetCoreRateLimit 套件實作限流機制，防止 DDoS 攻擊。

---

## 監控與可觀測性

### 健康檢查 (Health Checks)

#### 多層健康檢查架構
- **自我檢查**: API 服務本身狀態
- **資料庫檢查**: SQL Server 連線與查詢
- **快取檢查**: Redis 連線狀態
- **外部服務檢查**: 第三方 API 可用性

端點：
- `/health` - 完整健康檢查
- `/health/ready` - 就緒檢查（資料庫、快取）
- `/health/live` - 存活檢查（API 服務）

### OpenTelemetry 整合

#### 分散式追蹤設定
支援 Jaeger、Prometheus 等監控系統整合，提供分散式追蹤能力。

### 效能計數器與度量

#### 自訂度量收集
收集業務指標（如會員建立數、登入次數）與效能指標（如操作持續時間）。

### 日誌聚合與分析

#### Seq 結構化日誌設定
使用 Serilog 輸出結構化日誌到 Seq，支援日誌查詢與分析。

---

## 容器化與部署最佳實務

### Docker 容器化

#### 多階段建置
使用多階段 Dockerfile 減少映像大小，分離建置環境與執行環境。

#### 安全性考量
- 使用非 root 使用者執行
- 最小化映像大小
- 定期更新基礎映像

📝 **Docker 配置參考**: [docker-compose.yml](docker-compose.yml)

### CI/CD 管線

支援 GitHub Actions、Azure DevOps 等 CI/CD 工具，自動化測試、建置與部署流程。

### 生產環境設定管理

#### 環境變數與機密管理
- 開發環境：.NET User Secrets、`env/local.env`
- 容器環境：Docker/K8s Secrets
- 雲端環境：Azure Key Vault 等祕密管家

#### Kubernetes 部署
支援 Kubernetes 部署，包含 Deployment、Service、HPA 等資源配置。

---

## 附錄：快速參考

### 重要檔案路徑

#### Controller 層
- [src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs)

#### Handler 層
- [src/be/JobBank1111.Job.WebAPI/Member/MemberHandler.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberHandler.cs)

#### Repository 層
- [src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs](src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs)

#### 中介軟體
- [src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs](src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs)
- [src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs](src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs)
- [src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs](src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs)

#### 錯誤處理
- [src/be/JobBank1111.Job.WebAPI/Failure.cs](src/be/JobBank1111.Job.WebAPI/Failure.cs)
- [src/be/JobBank1111.Job.WebAPI/FailureCode.cs](src/be/JobBank1111.Job.WebAPI/FailureCode.cs)
- [src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs](src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs)

#### 追蹤管理
- [src/be/JobBank1111.Job.WebAPI/TraceContext.cs](src/be/JobBank1111.Job.WebAPI/TraceContext.cs)

#### 快取
- [src/be/JobBank1111.Infrastructure/Caching/](src/be/JobBank1111.Infrastructure/Caching/)

#### 測試
- [src/be/JobBank1111.Job.IntegrationTest/](src/be/JobBank1111.Job.IntegrationTest/) - BDD 整合測試
- [src/be/JobBank1111.Job.Test/](src/be/JobBank1111.Job.Test/) - 單元測試

### 最佳實踐檢查清單

#### 開發前
- [ ] 檢查專案狀態（是否需要初始化）
- [ ] 確認技術選型（資料庫、快取、專案結構）
- [ ] **【強制互動】詢問測試策略與範圍**
- [ ] 撰寫 BDD 情境 (.feature 檔案) - 如果需要
- [ ] 更新 OpenAPI 規格

#### 開發中
- [ ] 使用主建構函式注入
- [ ] 使用 Result Pattern 處理錯誤
- [ ] 所有 I/O 操作使用 async/await
- [ ] 傳遞 CancellationToken
- [ ] 使用 DbContextFactory 而非直接注入 DbContext
- [ ] 保存原始例外到 Failure.Exception
- [ ] 使用結構化日誌格式

#### 測試（依測試策略執行）
- [ ] **確認已詢問使用者測試需求**
- [ ] 透過 BDD 情境測試 API（如果需要 BDD 測試）
- [ ] 實作單元測試（如果需要單元測試）
- [ ] 使用 Docker 測試環境
- [ ] 避免直接測試 Controller
- [ ] 確保測試資料清理
- [ ] 測試涵蓋核心業務邏輯與異常情境

#### 部署前
- [ ] 檢查機密未存放在 appsettings.json
- [ ] 所有測試通過（如果有實作測試）
- [ ] 無編譯警告
- [ ] 文件已更新
- [ ] Code Review 完成

---

**文件版本**: 2.1 (新增測試互動機制)
**最後更新**: 2025-12-16
