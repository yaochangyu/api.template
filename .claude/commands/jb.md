# /jb: JobBank API Framework Commands

JobBank API Framework 的核心指令系統，提供完整的程式碼生成與專案管理功能。

## 核心實體指令

### /jb:handler [實體名稱]
產生符合 Clean Architecture 的 Handler 類別
- 使用 Result Pattern 回傳類型
- 整合 TraceContext 追蹤
- 包含完整的驗證邏輯與錯誤處理
- 自動注入必要的相依性

**範例:**
```
/jb:handler Product
/jb:handler Member
```

### /jb:controller [實體名稱]  
產生 Controller 實作類別
- 繼承自動產生的 OpenAPI 介面
- HTTP 標頭參數自動處理
- Result 模式轉換為 ActionResult
- 整合 Swagger 文件註解

**範例:**
```
/jb:controller Product
/jb:controller Member
```

### /jb:repository [實體名稱]
產生 Repository 類別
- Entity Framework Core 整合
- 實作快取機制 (L1 + L2)
- 統一的錯誤處理模式
- 支援批次操作與非同步

**範例:**
```
/jb:repository Product
/jb:repository Member
```

### /jb:models [實體名稱]
產生完整的 Request/Response 模型集合
- CreateXxxRequest 模型 (含驗證屬性)
- UpdateXxxRequest 模型
- GetXxxResponse 模型
- ListXxxResponse 模型 (含分頁)
- 符合 OpenAPI 規格註解

**範例:**
```
/jb:models Product
/jb:models Member
```

### /jb:domain [實體名稱]
產生 Domain 層實體與值物件
- Domain Entity 定義
- Value Objects 
- Domain Events
- Business Rules 驗證
- Aggregate Root 模式

**範例:**
```
/jb:domain Product
/jb:domain Member
```

### /jb:test [實體名稱]
產生完整的測試套件
- Handler 單元測試
- Repository 整合測試
- Controller API 測試
- BDD 情境測試 (Reqnroll)
- 測試資料建構器

**範例:**
```
/jb:test Product
/jb:test Member
```

## 基礎設施指令

### /jb:middleware [中介軟體名稱]
產生自訂中介軟體類別
- 遵循專案中介軟體模式
- 包含 TraceContext 整合
- 結構化日誌記錄
- 錯誤處理機制

**範例:**
```
/jb:middleware Authentication
/jb:middleware RateLimit
```

### /jb:migration [遷移名稱]
產生 Entity Framework Migration
- 檢查 Domain 實體變更
- 產生對應的資料庫遷移
- 包含種子資料 (如適用)
- 自動更新資料庫

**範例:**
```
/jb:migration AddProductTable
/jb:migration UpdateMemberSchema
```

## 批次操作指令

### /jb:batch create
批次產生實體的完整功能
```
/jb:batch create entity=Product,Order,Member operations=crud
```
這將產生所有指定實體的：
- Domain 實體
- Request/Response 模型
- Handler 類別
- Controller 類別
- Repository 類別
- 測試套件

### /jb:batch refactor
批次重構現有程式碼
```
/jb:batch refactor pattern=result-pattern target=handlers
```

## 專案管理指令

### /jb:init
初始化 JobBank API Framework
- 建立必要的目錄結構
- 複製程式碼模板
- 設定 NuGet 套件參考
- 更新 Program.cs 設定

### /jb:clean
清理專案並重建
- 清理 bin/obj 目錄
- 重建整個解決方案
- 清除快取檔案
- 重新產生 API 客戶端

### /jb:validate
驗證專案結構與程式碼品質
- 檢查 Clean Architecture 邊界
- 驗證 Result Pattern 使用
- 檢查命名規範
- 分析相依性循環

### /jb:analyze
深度程式碼分析
- 效能瓶頸分析
- 安全漏洞檢測
- 程式碼複雜度分析
- 技術債務評估

### /jb:docs
產生專案文件
- API 文件 (OpenAPI/Swagger)
- 架構文件
- 開發者指南
- 部署文件

## 行為模式指令

### /jb:mode [模式名稱]
切換框架行為模式

#### development
開發模式 - 快速產生程式碼
```
/jb:mode development
```
- 優先程式碼產生速度
- 包含詳細的除錯資訊
- 自動熱重載支援

#### refactoring  
重構模式 - 分析並重構現有程式碼
```
/jb:mode refactoring
```
- 分析現有程式碼結構
- 建議重構機會
- 批次套用改善

#### testing
測試模式 - 專注於測試驗證
```
/jb:mode testing
```
- 優先產生測試程式碼
- 包含完整覆蓋率分析
- 自動執行測試套件

#### documentation
文件模式 - 產生完整文件
```
/jb:mode documentation
```
- 優先產生 API 文件
- 包含架構圖表
- 自動同步程式碼註解

#### production
生產模式 - 最佳化與部署準備
```
/jb:mode production
```
- 程式碼最佳化
- 安全性強化
- 效能調校

## 使用範例

### 建立新的產品管理功能
```
/jb:mode development
/jb:domain Product
/jb:models Product
/jb:handler Product
/jb:controller Product
/jb:repository Product
/jb:test Product
/jb:validate
```

### 重構現有會員系統
```
/jb:mode refactoring
/jb:analyze Member
/jb:batch refactor pattern=result-pattern target=MemberHandler
/jb:test Member
```

### 準備生產部署
```
/jb:mode production
/jb:clean
/jb:validate
/jb:analyze
/jb:docs
```

所有指令都會：
- 遵循專案的 Clean Architecture 原則
- 自動整合 TraceContext 追蹤
- 使用 Result Pattern 錯誤處理
- 包含適當的驗證與測試
- 符合專案命名與程式碼風格規範