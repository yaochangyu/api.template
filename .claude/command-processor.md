# WebAPI Command Processor

這是 Claude Code 的指令處理邏輯，定義如何回應 `/webapi:*` 系列指令。

## 指令處理規則

當用戶輸入以下指令時，Claude Code 應該：

### /webapi:handler [實體名稱]

1. **取得參考實作**: 透過 FileResolver 取得真實 Handler 範例
   ```bash
   node .claude/skills/shared/FileResolver.js get-content \
     JobBank1111.Job.WebAPI/Member/MemberHandler.cs
   ```
2. **替換變數**: 
   - `{{ENTITY}}` → 實體名稱（首字母大寫）
   - `{{entity}}` → 實體名稱（首字母小寫）
3. **確定目標路徑**: `src/be/JobBank1111.Job.WebAPI/{實體名稱}/{實體名稱}Handler.cs`
4. **檢查檔案存在性**: 如果檔案已存在，詢問是否覆蓋
5. **建立目錄**: 如果目標目錄不存在，自動建立
6. **寫入檔案**: 根據參考實作與需求生成檔案

**範例**:
```
用戶輸入: /webapi:handler Product
結果: 建立 src/be/JobBank1111.Job.WebAPI/Product/ProductHandler.cs
```

### /webapi:controller [實體名稱]

1. **取得參考實作**: 透過 FileResolver 取得真實 Controller 範例
   ```bash
   node .claude/skills/shared/FileResolver.js get-content \
     JobBank1111.Job.WebAPI/Member/MemberV1ControllerImpl.cs
   ```
2. **替換變數**: 
   - `{{ENTITY}}` → 實體名稱（首字母大寫）
   - `{{entity}}` → 實體名稱（首字母小寫）
3. **確定目標路徑**: `src/be/JobBank1111.Job.WebAPI/{實體名稱}/{實體名稱}V1ControllerImpl.cs`
4. **處理邏輯**: 根據參考實作與需求生成檔案

**範例**:
```
用戶輸入: /webapi:controller Product
結果: 建立 src/be/JobBank1111.Job.WebAPI/Product/ProductControllerImpl.cs
```

### /webapi:middleware [中介軟體名稱]

1. **取得參考實作**: 透過 FileResolver 取得真實 Middleware 範例
   ```bash
   node .claude/skills/shared/FileResolver.js get-content \
     JobBank1111.Job.WebAPI/TraceContextMiddleware.cs
   ```
2. **替換變數**: 
   - `{{MIDDLEWARE_NAME}}` → 中介軟體名稱
3. **確定目標路徑**: `src/be/JobBank1111.Job.WebAPI/{中介軟體名稱}Middleware.cs`
4. **處理邏輯**: 根據參考實作與需求生成檔案

**範例**:
```
用戶輸入: /webapi:middleware RateLimit
結果: 建立 src/be/JobBank1111.Job.WebAPI/RateLimitMiddleware.cs
```

### /webapi:repository [實體名稱]

1. **取得參考實作**: 透過 FileResolver 取得真實 Repository 範例
   ```bash
   node .claude/skills/shared/FileResolver.js get-content \
     JobBank1111.Job.WebAPI/Member/MemberRepository.cs
   ```
2. **替換變數**: 
   - `{{ENTITY}}` → 實體名稱（首字母大寫）
   - `{{entity}}` → 實體名稱（首字母小寫）
3. **確定目標路徑**: `src/be/JobBank1111.Job.WebAPI/{實體名稱}/{實體名稱}Repository.cs`
4. **處理邏輯**: 根據參考實作與需求生成檔案

**範例**:
```
用戶輸入: /webapi:repository Product
結果: 建立 src/be/JobBank1111.Job.WebAPI/Product/ProductRepository.cs
```

## 進階處理邏輯

### 變數替換規則

```javascript
const variableReplacements = {
    '{{ENTITY}}': entityName,                    // Product
    '{{entity}}': entityName.toLowerCase(),     // product
    '{{MIDDLEWARE_NAME}}': middlewareName,      // RateLimit
    '{{NAMESPACE}}': 'JobBank1111.Job.WebAPI'   // 固定命名空間
};
```

### 檔案路徑生成

```javascript
function generateFilePath(type, name) {
    const basePath = 'src/be/JobBank1111.Job.WebAPI';
    
    switch(type) {
        case 'handler':
            return `${basePath}/${name}/${name}Handler.cs`;
        case 'controller':
            return `${basePath}/${name}/${name}ControllerImpl.cs`;
        case 'middleware':
            return `${basePath}/${name}Middleware.cs`;
        case 'repository':
            return `${basePath}/${name}/${name}Repository.cs`;
        default:
            throw new Error(`Unknown type: ${type}`);
    }
}
```

### 錯誤處理

1. **無效的實體名稱**: 檢查名稱是否符合 C# 命名規範
2. **檔案已存在**: 提供選項讓用戶決定是否覆蓋
3. **權限問題**: 確保有寫入檔案的權限
4. **模板不存在**: 檢查模板檔案是否存在

### 後續處理

執行指令後，Claude Code 應該：

1. **顯示成功訊息**: 
   ```
   ✅ 成功建立 ProductHandler.cs
   📁 位置: src/be/JobBank1111.Job.WebAPI/Product/ProductHandler.cs
   ```

2. **提供後續建議**:
   ```
   💡 建議接下來執行：
   - /webapi:repository Product （建立對應的 Repository）
   - /webapi:controller Product （建立對應的 Controller）
   ```

3. **檢查依賴項目**: 提醒用戶可能需要：
   - 建立 Request/Response 模型
   - 更新 DI 容器註冊
   - 新增 Entity Framework 實體
   - 更新 OpenAPI 規格

## 實作提示

### Claude Code 實作方式

```markdown
當用戶輸入 `/webapi:handler Product` 時，我應該：

1. 透過 FileResolver 取得參考實作：
   ```bash
   node .claude/skills/shared/FileResolver.js get-content \
     JobBank1111.Job.WebAPI/Member/MemberHandler.cs
   ```
2. 參考真實代碼，根據實體需求調整邏輯
3. 檢查目標目錄 `src/be/JobBank1111.Job.WebAPI/Product/` 是否存在
4. 如果不存在，建立目錄
5. 根據參考實作與需求生成 `src/be/JobBank1111.Job.WebAPI/Product/ProductHandler.cs`
6. 回覆成功訊息和後續建議
```

### 組合指令處理

當用戶要建立完整功能時，可以提供快速指令：

```
/webapi:all Product
```

這會依序執行：
1. `/webapi:handler Product`
2. `/webapi:repository Product`  
3. `/webapi:controller Product`

### 參數支援

未來可以支援額外參數：

```
/webapi:handler Product --with-cache --async-only
```

參數說明：
- `--with-cache`: 加入快取相關程式碼
- `--async-only`: 只產生異步方法
- `--no-validation`: 跳過驗證邏輯
- `--minimal`: 產生最簡版本

這個處理器確保所有產生的程式碼都遵循專案的 Clean Architecture 原則和 CLAUDE.md 中定義的規範。