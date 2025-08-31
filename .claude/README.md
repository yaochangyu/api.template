# Claude Code WebAPI Framework

專為 JobBank1111 API 專案設計的程式碼產生框架，實作方案一：Command 模式 - Slash Commands。

## 📁 目錄結構

```
.claude/
├── README.md                   # 此說明文件
├── commands/
│   └── webapi.md              # /webapi 系列指令定義
├── templates/
│   ├── handler-template.cs     # Handler 類別模板
│   ├── controller-template.cs  # Controller 實作模板
│   ├── middleware-template.cs  # Middleware 模板
│   └── repository-template.cs  # Repository 模板
└── command-processor.md        # 指令處理邏輯說明
```

## 🚀 快速開始

### 1. 基本使用

在 Claude Code 中直接輸入指令：

```bash
/webapi:handler Product        # 建立 ProductHandler.cs
/webapi:controller Product     # 建立 ProductControllerImpl.cs
/webapi:repository Product     # 建立 ProductRepository.cs
/webapi:middleware RateLimit   # 建立 RateLimitMiddleware.cs
```

### 2. 完整功能建立

建立完整的實體管理功能：

```bash
# 依序執行以下指令
/webapi:handler Order
/webapi:repository Order
/webapi:controller Order
```

產生的檔案結構：
```
src/be/JobBank1111.Job.WebAPI/Order/
├── OrderHandler.cs        # 業務邏輯處理
├── OrderRepository.cs     # 資料存取層
└── OrderControllerImpl.cs # API 控制器實作
```

## 📋 指令說明

### `/webapi:handler [實體名稱]`

**功能**: 產生符合專案規範的 Handler 類別  
**位置**: `src/be/JobBank1111.Job.WebAPI/{實體名稱}/{實體名稱}Handler.cs`

**包含功能**:
- ✅ Result Pattern 錯誤處理
- ✅ TraceContext 追蹤整合
- ✅ 完整 CRUD 操作方法
- ✅ 連續驗證邏輯鏈
- ✅ 依賴注入設定

**範例**:
```csharp
// 產生 ProductHandler.cs，包含：
public class ProductHandler(
    ProductRepository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<ProductHandler> logger)
{
    public async Task<Result<Product, Failure>> CreateAsync(...)
    public async Task<Result<Product, Failure>> UpdateAsync(...)
    public async Task<Result<Product, Failure>> GetByIdAsync(...)
    // ... 其他 CRUD 方法
}
```

### `/webapi:controller [實體名稱]`

**功能**: 產生 Controller 實作類別  
**位置**: `src/be/JobBank1111.Job.WebAPI/{實體名稱}/{實體名稱}ControllerImpl.cs`

**包含功能**:
- ✅ 繼承自動產生的 Contract 介面
- ✅ HTTP 標頭參數自動擷取
- ✅ Result 自動轉換為 ActionResult
- ✅ 分頁參數標準化處理
- ✅ 錯誤回應統一轉換

### `/webapi:repository [實體名稱]`

**功能**: 產生 Repository 類別  
**位置**: `src/be/JobBank1111.Job.WebAPI/{實體名稱}/{實體名稱}Repository.cs`

**包含功能**:
- ✅ Entity Framework Core 整合
- ✅ 多層快取機制 (Memory + Redis)
- ✅ 完整錯誤處理和日誌記錄
- ✅ 分頁查詢 (Offset + Cursor)
- ✅ 搜尋功能和快取失效機制

### `/webapi:middleware [中介軟體名稱]`

**功能**: 產生 Middleware 類別  
**位置**: `src/be/JobBank1111.Job.WebAPI/{中介軟體名稱}Middleware.cs`

**包含功能**:
- ✅ 標準中介軟體管線模式
- ✅ TraceContext 和日誌系統整合
- ✅ 前置/後置處理邏輯框架
- ✅ 錯誤處理和效能記錄
- ✅ 擴充方法自動產生

## 🎯 設計原則

所有產生的程式碼都遵循：

### Clean Architecture 原則
- **分層架構**: Handler (業務邏輯) → Repository (資料存取) → Controller (API 層)
- **依賴反向**: 高層模組不依賴低層模組，都依賴於抽象
- **關注點分離**: 每層專注於自己的職責

### 專案規範 (CLAUDE.md)
- **Result Pattern**: 統一的錯誤處理模式
- **TraceContext**: 完整的請求追蹤機制
- **日誌整合**: 結構化日誌和錯誤記錄
- **快取策略**: 多層快取和失效機制
- **驗證邏輯**: 連續驗證和錯誤回應

### 程式碼品質
- **不可變物件**: 使用 record 類型和 init 屬性
- **異步操作**: 所有 I/O 操作使用 async/await
- **錯誤處理**: 統一的 Failure 物件和狀態碼對應
- **測試友善**: 相依性注入和可測試設計

## 🔧 自訂和擴展

### 修改模板

如需客製化產生的程式碼，可以直接編輯模板檔案：

1. **Handler 模板**: `.claude/templates/handler-template.cs`
2. **Controller 模板**: `.claude/templates/controller-template.cs`
3. **Repository 模板**: `.claude/templates/repository-template.cs`
4. **Middleware 模板**: `.claude/templates/middleware-template.cs`

### 變數系統

模板支援以下變數替換：

| 變數 | 說明 | 範例 |
|------|------|------|
| `{{ENTITY}}` | 實體名稱 (首字母大寫) | Product |
| `{{entity}}` | 實體名稱 (首字母小寫) | product |
| `{{MIDDLEWARE_NAME}}` | 中介軟體名稱 | RateLimit |

### 新增指令

要新增自訂指令：

1. 在 `.claude/commands/webapi.md` 中新增指令定義
2. 建立對應的模板檔案
3. 更新 `.claude/command-processor.md` 中的處理邏輯

## 📝 最佳實務

### 建議的建立順序

1. **先建立實體層**:
   ```bash
   # 確保 EF 實體已存在於 JobBank1111.Job.DB
   ```

2. **建立資料存取層**:
   ```bash
   /webapi:repository Product
   ```

3. **建立業務邏輯層**:
   ```bash
   /webapi:handler Product
   ```

4. **建立 API 層**:
   ```bash
   /webapi:controller Product
   ```

5. **註冊依賴注入**:
   ```csharp
   // 在 Program.cs 中註冊
   builder.Services.AddScoped<ProductRepository>();
   builder.Services.AddScoped<ProductHandler>();
   ```

### 後續工作檢查清單

產生程式碼後，通常需要：

- [ ] 建立或更新 Request/Response 模型類別
- [ ] 更新 OpenAPI 規格 (`doc/openapi.yml`)
- [ ] 執行 `task codegen-api` 重新產生 Contract
- [ ] 更新依賴注入註冊
- [ ] 撰寫單元測試和整合測試
- [ ] 執行 `task api-dev` 測試功能

## 🐛 疑難排解

### 常見問題

**Q: 指令沒有反應？**  
A: 確保在 Claude Code 環境中執行，且指令格式正確。

**Q: 產生的檔案位置不對？**  
A: 檢查目前工作目錄是否在專案根目錄。

**Q: 編譯錯誤？**  
A: 檢查是否已建立對應的 Request/Response 類別和 EF 實體。

**Q: 如何修改產生的程式碼？**  
A: 編輯 `.claude/templates/` 中的對應模板檔案。

### 除錯方式

1. **檢查模板檔案**: 確保模板語法正確
2. **驗證變數替換**: 確認實體名稱符合 C# 命名規範  
3. **查看目標目錄**: 確保有寫入權限
4. **檢查依賴項目**: 確認所需的 using 陳述式和類別存在

## 🎉 總結

這個 WebAPI Framework 提供了：

- **快速開發**: 一個指令建立完整的類別結構
- **一致性**: 所有程式碼遵循相同的模式和規範
- **可維護性**: 標準化的架構便於團隊協作
- **擴展性**: 模板系統支援客製化和擴展

開始使用：`/webapi:handler YourEntityName` 🚀