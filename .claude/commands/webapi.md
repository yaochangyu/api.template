---
allowed-tools: Read(*), Write(*), Edit(*), Glob(*), Grep(*)
argument-hint: [subcommand] [entity-name]
description: 產生符合 JobBank1111 API 專案規範的程式碼
---

# WebAPI 程式碼產生工具

根據第一個參數 $1 決定要產生的程式碼類型，第二個參數 $2 是實體名稱。

支援的子命令：
- **full [實體名稱]**: 產生完整功能模組（包含所有相關類別）
- **command [實體名稱]**: 產生 Command 類別（基於 MemberCommand 模式）
- handler [實體名稱]: 產生 Handler 類別
- controller [實體名稱]: 產生 Controller 實作類別  
- middleware [中介軟體名稱]: 產生 Middleware 類別
- repository [實體名稱]: 產生 Repository 類別
- request [實體名稱]: 產生 Request/Response 模型
- test [實體名稱]: 產生測試類別

### 🚀 推薦使用 `full` 命令
使用 `/webapi full [實體名稱]` 可以一次產生完整的功能模組，包含：
1. Handler 類別 - 商業邏輯處理
2. Controller 實作類別 - API 端點
3. Repository 類別 - 資料存取層
4. Request/Response 模型 - 資料傳輸物件
5. 測試類別 - 單元與整合測試

例如：`/webapi full Product` 會自動產生所有 Product 相關的檔案。

---

## full 子命令
**一次產生完整功能模組的所有相關檔案**

### 功能特色
- 🔥 **一鍵產生** - 執行單一命令產生所有檔案
- 🏗️ **完整架構** - 自動建立完整的 Clean Architecture 分層結構
- ⚡ **效率提升** - 大幅減少重複操作與手動建檔
- 🔧 **依賴整合** - 自動處理各層之間的依賴關係
- ✅ **測試覆蓋** - 同時產生對應的測試類別

### 使用範例
```
/webapi full Order
/webapi full Customer  
/webapi full Category
```

### 產生內容
執行 `/webapi full [實體名稱]` 會依序產生：

1. **Handler 類別** (`{Entity}Handler.cs`)
   - 商業邏輯處理器
   - Result Pattern 錯誤處理
   - TraceContext 整合
   - 完整 CRUD 與搜尋功能

2. **Controller 實作類別** (`{Entity}ControllerImpl.cs`)
   - API 端點實作
   - HTTP 標頭參數處理
   - 分頁查詢支援
   - 自動錯誤轉換

3. **Repository 類別** (`{Entity}Repository.cs`)
   - Entity Framework Core 整合
   - 快取機制 (Memory + Redis)
   - 分頁查詢實作
   - 異步操作最佳化

4. **Request/Response 模型**
   - `Create{Entity}Request.cs` - 建立請求模型
   - `Update{Entity}Request.cs` - 更新請求模型  
   - `Get{Entity}Response.cs` - 回應模型
   - 分頁回應模型

5. **測試類別**
   - 單元測試 (xUnit + FluentAssertions)
   - 整合測試 (Testcontainers)
   - BDD 情境測試 (Reqnroll)

### 執行順序
為確保依賴關係正確，檔案會依照以下順序產生：
1. Request/Response 模型 → 2. Repository → 3. Handler → 4. Controller → 5. 測試

---

## command 子命令
產生符合專案規範的 Command 類別，基於 MemberCommand 模式

### 功能特色
- 基於 MemberRepository 分析的標準模式
- 使用 Result Pattern 錯誤處理
- 自動整合 TraceContext 追蹤
- 包含結構化日誌記錄
- 自動產生對應的 Request/Response Model

### 使用範例
```
/webapi command Product
/webapi command Order
/webapi command Customer
```

### 產生內容
基於 MemberRepository 的方法分析，會產生以下內容：

**主要 Command 類別：**
- `{Entity}Command.cs` - 主要 Command 類別

**對應的 Request/Response Model：**
- `Insert{Entity}Request.cs` - 插入請求模型
- `Insert{Entity}Response.cs` - 插入回應模型
- `Get{Entity}ByEmailRequest.cs` - Email 查詢請求模型
- `Get{Entity}ByEmailResponse.cs` - Email 查詢回應模型
- `Get{Entity}sOffsetRequest.cs` - Offset 分頁請求模型
- `Get{Entity}sOffsetResponse.cs` - Offset 分頁回應模型
- `Get{Entity}sCursorRequest.cs` - Cursor 分頁請求模型
- `Get{Entity}sCursorResponse.cs` - Cursor 分頁回應模型

**標準方法模板：**
- `Insert{Entity}Async` - 新增實體，包含 Email 重複檢查
- `Get{Entity}ByEmailAsync` - 透過 Email 查詢實體
- `Get{Entity}sOffsetAsync` - Offset 分頁查詢
- `Get{Entity}sCursorAsync` - Cursor 分頁查詢

### 設計模式
遵循 MemberCommand 的設計模式：
- 日誌記錄在方法開始和結束
- 統一的錯誤處理和 Failure 物件建立
- TraceContext 整合追蹤
- 業務邏輯驗證（如重複 Email 檢查）

---

## handler 子命令
產生符合專案規範的 Handler 類別

### 功能特色
- 使用 Result Pattern 錯誤處理
- 自動整合 TraceContext 追蹤
- 包含完整驗證邏輯鏈
- 遵循 Clean Architecture 分層設計
- 自動注入依賴項目

### 使用範例
```
/webapi:handler Product
/webapi:handler Order
/webapi:handler Customer
```

### 產生內容
- Handler 主類別 (`{Entity}Handler.cs`)
- 標準 CRUD 方法模板
- 驗證方法鏈
- 錯誤處理邏輯
- TraceContext 整合

---

## /webapi:controller [實體名稱]
產生 Controller 實作類別

### 功能特色
- 繼承自動產生的 Contract 介面
- HTTP 標頭參數自動處理
- Result 自動轉換為 ActionResult
- 分頁參數標準化處理
- 快取控制標頭支援

### 使用範例
```
/webapi:controller Product
/webapi:controller Order
/webapi:controller Customer
```

### 產生內容
- ControllerImpl 實作類別
- HTTP 標頭參數擷取方法
- 標準 API 端點實作
- 錯誤回應轉換
- 分頁處理邏輯

---

## /webapi:middleware [中介軟體名稱]
產生 Middleware 類別

### 功能特色
- 遵循專案中介軟體管線模式
- 整合 TraceContext 和日誌系統
- 標準錯誤處理和安全防護
- 請求資訊自動擷取
- 效能考量的設計模式

### 使用範例
```
/webapi:middleware Authentication
/webapi:middleware RateLimit
/webapi:middleware Validation
```

### 產生內容
- Middleware 主類別
- InvokeAsync 方法實作
- 日誌整合邏輯
- 錯誤處理機制
- 依賴注入設定

---

## /webapi:repository [實體名稱]
產生 Repository 類別

### 功能特色
- Entity Framework Core 整合
- 快取機制 (Memory + Redis)
- 完整錯誤處理模式
- 分頁查詢支援
- 異步操作最佳化

### 使用範例
```
/webapi:repository Product
/webapi:repository Order
/webapi:repository Customer
```

### 產生內容
- Repository 主類別
- 標準 CRUD 操作
- 快取層整合
- 分頁查詢方法
- 錯誤處理邏輯

---

## /webapi:request [實體名稱]
產生 Request/Response 模型類別

### 功能特色
- 資料驗證屬性
- 自動映射邏輯
- 安全性驗證
- 文檔註解支援

### 使用範例
```
/webapi:request Product
/webapi:request Order
```

### 產生內容
- Create/Update Request 類別
- Response 類別
- 驗證屬性設定
- 映射方法

---

## /webapi:test [實體名稱]
產生測試類別

### 功能特色
- 單元測試 (xUnit)
- 整合測試 (Testcontainers)
- BDD 情境測試 (Reqnroll)
- Mock 物件設定

### 使用範例
```
/webapi:test Product
/webapi:test Order
```

### 產生內容
- Handler 單元測試
- Controller 整合測試
- Repository 測試
- BDD 情境檔案

---

## 使用說明

### 基本使用
1. 在 Claude Code 中直接輸入指令
2. 指令會自動分析專案結構
3. 根據現有模式產生對應程式碼
4. 自動放置到正確的專案位置

### 進階參數
某些指令支援額外參數：
```
/webapi:handler Product --with-cache --with-validation
/webapi:controller Product --async-only
/webapi:test Product --integration-only
```

### 推薦使用方式
**🚀 最佳選擇：使用 full 命令**
```
/webapi full Product  # 一次產生所有相關檔案
```

**🔧 個別建立：分別執行各子命令**  
```
/webapi handler Product
/webapi controller Product  
/webapi repository Product
/webapi request Product
/webapi test Product
```

### 注意事項
- 所有產生的程式碼都遵循 CLAUDE.md 規範
- 自動整合 TraceContext 和日誌系統
- 使用專案既有的依賴注入設定
- 產生前會檢查檔案是否已存在，避免覆蓋