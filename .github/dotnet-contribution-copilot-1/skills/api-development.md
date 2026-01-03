# Skill: API Development

> API 開發 Skill - 指導完整的 API 開發流程

## Skill 職責

- 引導選擇 API 開發流程（API First vs Code First）
- 協助定義/更新 OpenAPI 規格
- 指導 Controller/Handler/Repository 實作
- 提供程式碼範本與最佳實踐

## 引用文件

- [架構概覽](../references/architecture-overview.md)
- [命名規範](../references/naming-conventions.md)
- [錯誤處理](../references/error-handling.md)
- [Controller 範本](../assets/controller-template.cs)
- [Handler 範本](../assets/handler-template.cs)
- [Repository 範本](../assets/repository-template.cs)

## 執行流程

### 1. 理解使用者需求

收集資訊：
- 功能名稱（例如：會員註冊、產品管理）
- API 端點路徑
- 需要的 CRUD 操作
- 特殊業務邏輯

### 2. 選擇開發流程（強制詢問）

```markdown
請選擇 API 開發流程：

1️⃣ API First（推薦）
   - 先定義 OpenAPI 規格 (doc/openapi.yml)
   - 透過 task codegen-api-server 產生 Controller 骨架
   - 確保 API 契約優先、文件與實作同步
   
2️⃣ Code First
   - 直接實作程式碼
   - 後續手動維護 OpenAPI 規格

請輸入 1 或 2：
```

### 3. API First 流程

#### 步驟 1：OpenAPI 規格狀態詢問

```markdown
doc/openapi.yml 中關於此 API 的規格：

1️⃣ 已定義完整規格
2️⃣ 需要更新/新增規格
3️⃣ 尚未定義

請輸入選項：
```

#### 步驟 2：定義/更新 OpenAPI 規格

協助使用者編輯 `doc/openapi.yml`，包含：
- HTTP 方法與路徑
- Request/Response Schema
- 錯誤回應格式
- 參數驗證規則

#### 步驟 3：產生 Server 程式碼

```powershell
task codegen-api-server
```

### 4. Code First 流程

直接使用範本實作：
1. Controller（使用 controller-template.cs）
2. Handler（使用 handler-template.cs）
3. Repository（使用 repository-template.cs）

### 5. 分層實作指導

#### Controller 層

- 使用主建構函式注入 Handler
- 實作 HTTP 路由
- Result Pattern 回應轉換
- 加入 API 文件註解

#### Handler 層

- 實作業務邏輯
- 呼叫 Repository
- 錯誤處理（Result Pattern）
- 實體到 DTO 映射

#### Repository 層

- 詢問設計策略（資料表導向 vs 需求導向）
- 使用 IDbContextFactory
- 實作 CRUD 操作
- 查詢最佳化

## 輸出成果

- ✅ OpenAPI 規格已定義/更新（API First）
- ✅ Controller/Handler/Repository 已實作
- ✅ 遵循命名規範與最佳實踐
- ✅ 使用 Result Pattern 處理錯誤

---

**Skill 類型**：指導式開發  
**相依 Skills**：testing-strategy（後續執行）  
**適用 Agents**：dotnet-api-developer
