# SKILL 共用工具

## FileResolver - 混合檔案路徑解析器

### 概述

`FileResolver` 是一個統一檔案路徑解析工具，支援混合模式：
- ✅ **本地優先** - 開發者快速迭代（無網路延遲）
- ✅ **GitHub 備援** - 獨立用戶無需下載整個專案
- ✅ **本地快取** - 離線環境正常使用

### 使用場景

#### 場景 A：開發者（已下載整個專案）
```javascript
const FileResolver = require('./FileResolver');
const resolver = new FileResolver();

// 自動使用本地檔案
const content = await resolver.getFileContent('dotnet-project-template/doc/openapi.yml');
```
→ 優先使用本地檔案，速度快

#### 場景 B：獨立用戶（只安裝 SKILL）
```javascript
const FileResolver = require('./FileResolver');
const resolver = new FileResolver();

// 自動從 GitHub 下載，並快取
const content = await resolver.getFileContent('dotnet-project-template/doc/openapi.yml');
```
→ 自動從 GitHub 下載，並快取以備後用

#### 場景 C：離線環境
```javascript
const FileResolver = require('./FileResolver');
const resolver = new FileResolver({ offlineMode: true });

// 使用本地快取或本地檔案
const content = await resolver.getFileContent('dotnet-project-template/doc/openapi.yml');
```
→ 只使用本地資源，不發起網路請求

### API 文件

#### 建構函式

```javascript
new FileResolver(options)
```

**參數：**

| 參數 | 型別 | 預設值 | 說明 |
|------|------|--------|------|
| `projectRoot` | string | `process.cwd()` | 專案根目錄 |
| `githubRepo` | string | `yaochangyu/api.template` | GitHub 倉庫（格式：owner/repo） |
| `githubBranch` | string | `main` | GitHub 分支 |
| `cacheDir` | string | `~/.cache/skill-resolver` | 快取目錄 |
| `cacheTTL` | number | `86400000` (24h) | 快取有效期（毫秒） |
| `retryAttempts` | number | `3` | GitHub 下載重試次數 |
| `retryDelay` | number | `1000` | 重試延遲（毫秒），使用指數退避 |
| `timeout` | number | `10000` | 網路請求超時（毫秒） |
| `offlineMode` | boolean | `false` | 離線模式（只使用本地資源） |

#### 方法

##### `getFileContent(relativePath)`

取得檔案內容，自動選擇最優源。

**參數：**
- `relativePath` (string) - 相對路徑（例如：`dotnet-project-template/doc/openapi.yml`）

**返回：** Promise<string> - 檔案內容

**例子：**
```javascript
const resolver = new FileResolver();

try {
  const content = await resolver.getFileContent('dotnet-project-template/doc/openapi.yml');
  console.log('OpenAPI 規格:', content);
} catch (error) {
  console.error('取得檔案失敗:', error.message);
}
```

**解析順序：**
1. ✓ 本地檔案存在 → 立即使用（無延遲）
2. ✓ 本地快取存在 → 使用快取（無網路）
3. ✓ 從 GitHub 下載 → 自動儲存到快取
4. ✗ 全部失敗 → 拋出錯誤

---

##### `getFilePath(relativePath)`

取得檔案的本地路徑（如果存在）。

**參數：**
- `relativePath` (string) - 相對路徑

**返回：** string|null - 本地路徑，如果檔案不存在則返回 null

**例子：**
```javascript
const resolver = new FileResolver();

const localPath = resolver.getFilePath('dotnet-project-template/doc/openapi.yml');
if (localPath) {
  console.log('本地檔案位置:', localPath);
} else {
  console.log('本地未找到該檔案');
}
```

**檢查順序：**
1. `{projectRoot}/dotnet-project-template/doc/openapi.yml`
2. `{projectRoot}/dotnet-project-template/dotnet-project-template/doc/openapi.yml`
3. `{projectRoot}/openapi.yml`

---

##### `getGithubUrl(relativePath)`

構造 GitHub Raw URL。

**參數：**
- `relativePath` (string) - 相對路徑

**返回：** string - GitHub Raw URL

**例子：**
```javascript
const resolver = new FileResolver();

const url = resolver.getGithubUrl('dotnet-project-template/doc/openapi.yml');
console.log(url);
// 輸出: https://raw.githubusercontent.com/yaochangyu/api.template/main/dotnet-project-template/doc/openapi.yml
```

---

##### `clearCache()`

清空所有本地快取。

**返回：** Promise<void>

**例子：**
```javascript
const resolver = new FileResolver();

await resolver.clearCache();
console.log('快取已清空');
```

---

##### `listCache()`

列出快取中的所有檔案。

**返回：** Promise<Array> - 快取檔案列表

**例子：**
```javascript
const resolver = new FileResolver();

const files = await resolver.listCache();
files.forEach(file => {
  console.log(`${file.name} - ${file.size} bytes (${file.age}ms old)`);
});
```

### 配置檔案（可選）

在 SKILL 目錄中建立 `.resolver-config.json`：

```json
{
  "projectRoot": "/path/to/project",
  "githubRepo": "yaochangyu/api.template",
  "githubBranch": "main",
  "cacheDir": "~/.cache/skill-resolver",
  "cacheTTL": 86400000,
  "retryAttempts": 3,
  "retryDelay": 1000,
  "timeout": 10000,
  "offlineMode": false
}
```

### 常見問題

#### Q: 為什麼有些檔案下載失敗？
**A:** 檢查：
1. GitHub 倉庫地址是否正確（預設：`yaochangyu/api.template`）
2. 檔案在 GitHub 上是否存在
3. 網路連線是否正常
4. 檔案路徑是否正確

#### Q: 如何強制重新下載檔案？
**A:** 清空快取後重新取得：
```javascript
const resolver = new FileResolver();
await resolver.clearCache();
const content = await resolver.getFileContent('your/file/path');
```

#### Q: 快取會無限增長嗎？
**A:** 不會。快取檔案有 24 小時的 TTL（可配置），超過期限會自動清除。

#### Q: 離線環境如何使用？
**A:** 啟用離線模式：
```javascript
const resolver = new FileResolver({ offlineMode: true });
// 只會使用本地檔案或快取，不發起網路請求
```

#### Q: 可以自訂 GitHub 倉庫嗎？
**A:** 可以，在建構函式中指定：
```javascript
const resolver = new FileResolver({
  githubRepo: 'your-org/your-repo',
  githubBranch: 'develop'
});
```

### 最佳實踐

1. **開發環境中使用專案根目錄**
   ```javascript
   const resolver = new FileResolver({ projectRoot: '/path/to/api.template' });
   ```

2. **生產環境自動偵測**
   ```javascript
   const resolver = new FileResolver(); // 使用預設值
   ```

3. **快取管理**
   ```javascript
   // 定期清空過期快取（例如：每週一次）
   if (shouldClearCache()) {
     await resolver.clearCache();
   }
   ```

4. **錯誤處理**
   ```javascript
   try {
     const content = await resolver.getFileContent('your/file/path');
   } catch (error) {
     console.error('無法取得檔案:', error.message);
     // 提供後備方案或提示使用者
   }
   ```

### 整合範例

在 SKILL 中使用 FileResolver：

```javascript
const FileResolver = require('./shared/FileResolver');

async function loadOpenAPISpec() {
  const resolver = new FileResolver();
  
  try {
    const spec = await resolver.getFileContent('dotnet-project-template/doc/openapi.yml');
    return YAML.parse(spec);
  } catch (error) {
    console.error('無法加載 OpenAPI 規格:', error.message);
    throw error;
  }
}

// 在 SKILL 中使用
const spec = await loadOpenAPISpec();
```

### 技術細節

#### 快取鍵計算
快取鍵使用 SHA256 雜湊，由倉庫名稱和檔案路徑組成：
```javascript
const key = `${githubRepo}:${relativePath}`;
const cacheKey = SHA256(key);
```

#### 重試策略
使用指數退避重試：
```
第 1 次：立即重試
第 2 次：延遲 1000ms（1秒）
第 3 次：延遲 2000ms（2秒）
第 4 次：延遲 4000ms（4秒）
```

#### 超時處理
所有網路請求都有 10 秒的超時限制，可在建構函式中自訂。

### 支援與回饋

如有任何問題或建議，請通過以下方式反饋：
- GitHub Issues
- 專案文檔
- SKILL 中心
