## 🤖 AI 助手互動規則

### 強制詢問情境

以下情境中，AI 助手**必須使用 AskUserQuestion 工具**先詢問使用者，不得擅自決定：

#### 1. 專案初始化與配置

**觸發條件**：
- 使用者提到「初始化」、「建立專案」、「設定專案」
- 使用者執行或詢問 `task dev-init`

**必須詢問**（使用 AskUserQuestion）：
```json
{
  "questions": [
    {
      "question": "是否需要 Redis 快取？",
      "header": "快取需求",
      "options": [
        {"label": "是", "description": "使用 Redis 作為分散式快取（生產環境推薦）"},
        {"label": "否", "description": "僅使用記憶體快取（開發測試用）"}
      ],
      "multiSelect": false
    },
    {
      "question": "請選擇資料庫類型",
      "header": "資料庫",
      "options": [
        {"label": "SQL Server", "description": "企業應用首選，.NET 生態系完整整合"},
        {"label": "PostgreSQL", "description": "開源、輕量、功能完整"},
        {"label": "MySQL", "description": "開源、廣泛支援、社群資源豐富"}
      ],
      "multiSelect": false
    },
    {
      "question": "請選擇專案結構組織方式",
      "header": "專案結構",
      "options": [
        {"label": "單一專案", "description": "適合 3 人以下團隊，結構簡單快速開發"},
        {"label": "多專案", "description": "適合大型團隊，層級清晰分離易於協作"}
      ],
      "multiSelect": false
    }
  ]
}
```

#### 2. 資料庫相關操作

**觸發條件**：
- 使用者提到「migration」、「遷移」、「scaffold」、「反向工程」
- 使用者要求產生實體或資料庫變更

**必須詢問**：
```json
{
  "questions": [
    {
      "question": "請選擇資料庫開發模式",
      "header": "開發模式",
      "options": [
        {"label": "Code First", "description": "從程式碼定義資料模型，自動產生 Migration"},
        {"label": "Database First", "description": "從現有資料庫反向工程產生實體"}
      ],
      "multiSelect": false
    }
  ]
}
```

**如果選擇 Code First**，再詢問：
```json
{
  "questions": [
    {
      "question": "Migration 名稱（描述性命名）",
      "header": "Migration"
    },
    {
      "question": "是否立即套用到資料庫？",
      "header": "套用",
      "options": [
        {"label": "是", "description": "執行 ef database update"},
        {"label": "否", "description": "僅建立 Migration 檔案"}
      ],
      "multiSelect": false
    }
  ]
}
```

**如果選擇 Database First**，再詢問：
```json
{
  "questions": [
    {
      "question": "請選擇要產生實體的資料表範圍",
      "header": "資料表範圍",
      "options": [
        {"label": "所有資料表", "description": "產生資料庫中所有資料表的實體"},
        {"label": "特定資料表", "description": "僅產生指定的資料表（需提供資料表名稱）"}
      ],
      "multiSelect": false
    }
  ]
}
```

#### 3. 功能實作

**觸發條件**：
- 使用者要求「實作」、「新增功能」、「開發」新的 API 或功能

**必須詢問**：
```json
{
  "questions": [
    {
      "question": "是否已定義 OpenAPI 規格？",
      "header": "API 規格",
      "options": [
        {"label": "是", "description": "已在 doc/openapi.yml 中定義"},
        {"label": "否", "description": "需要協助定義 API 規格"}
      ],
      "multiSelect": false
    },
    {
      "question": "此功能需要哪些層級？",
      "header": "分層架構",
      "options": [
        {"label": "Controller", "description": "HTTP 請求處理"},
        {"label": "Handler", "description": "業務邏輯處理"},
        {"label": "Repository", "description": "資料存取"}
      ],
      "multiSelect": true
    },
    {
      "question": "是否需要同時建立 BDD 測試？",
      "header": "測試",
      "options": [
        {"label": "是", "description": "建立 .feature 檔案與測試步驟"},
        {"label": "否", "description": "稍後再補測試"}
      ],
      "multiSelect": false
    }
  ]
}
```

#### 4. 效能最佳化

**觸發條件**：
- 使用者提到「快取」、「效能優化」、「慢」、「加速"

**必須詢問**：
```json
{
  "questions": [
    {
      "question": "請選擇要優化的面向",
      "header": "優化面向",
      "options": [
        {"label": "資料庫查詢", "description": "優化 EF Core 查詢與索引"},
        {"label": "快取策略", "description": "實作或調整快取機制"},
        {"label": "非同步處理", "description": "改善非同步程式碼"},
        {"label": "記憶體使用", "description": "減少記憶體配置"}
      ],
      "multiSelect": true
    }
  ]
}
```

### 禁止的行為 ❌

1. **不得擅自使用預設值**
   - 即使文件標註「預設」，仍須詢問確認
   - 例外：使用者明確指定（如「使用 SQL Server」）

2. **不得跳過詢問步驟**
   - 所有標註「必須詢問」的情境都要執行
   - 不可因「加速開發」而省略互動

3. **不得一次詢問過多問題**
   - 單次最多 3-4 個問題
   - 複雜流程分階段詢問

### 正確的互動流程範例 ✅

**使用者**: 「幫我初始化專案」

**AI 回應**:
```
我將協助您初始化專案。首先，讓我了解您的需求：

[使用 AskUserQuestion 工具詢問快取、資料庫、專案結構]

等待使用者回覆...

收到回覆後：
- 根據選擇生成對應的配置檔案
- 更新 docker-compose.yml
- 設定 appsettings.json
- 建立 env/local.env

完成後回報：
✅ 專案已初始化完成
- 資料庫: [使用者選擇]
- 快取: [使用者選擇]  
- 結構: [使用者選擇]
```

上述內容，加入至 @yaochangyu/api.template/files/CLAUDE.md