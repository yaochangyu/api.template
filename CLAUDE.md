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

### 中介軟體責任
- **身分驗證**: 在 `TraceContextMiddleware` 中統一處理使用者身分驗證
- **TraceId 處理**: 從請求標頭擷取或自動產生 TraceId
- **日誌增強**: 自動將 TraceId 與 UserId 附加到結構化日誌中
- **回應標頭**: 自動將 TraceId 加入回應標頭供追蹤使用

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