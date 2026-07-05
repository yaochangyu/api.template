---
name: skill-agent-design
description: GitHub Copilot Skill 與 Agent 設計指導技能，協助開發者設計與實作高品質的 Skill 和 Agent，包含架構設計、最佳實踐、工作流程規劃與常見反模式識別。
---

# Skill & Agent Design Skill

## 描述
GitHub Copilot Skill 與 Agent 設計指導技能，協助開發者設計與實作高品質的 Skill 和 Agent。

## 職責
- 指導 Skill 架構設計與實作
- 指導 Agent 角色定位與工具配置
- MCP 整合策略規劃
- 識別設計反模式並提供改善建議
- 工作流程與 Handoffs 規劃

## 核心概念

### Skill vs Agent vs MCP

| 類型 | 位置 | 用途 | 呼叫方式 |
|------|------|------|----------|
| **Skill** | `.claude/skills/` | 專業化功能模組 | `skill` 工具 |
| **Agent** | `.claude/agents/` | 具角色的 AI 助手 | `@agent-name` 或 `task` 工具 |
| **MCP** | `.mcp.json` | 外部工具/服務連接 | 自動整合 |

## Skill 設計工作流程

### 步驟 1: 需求分析
詢問開發者以下問題:
- [ ] 這個 Skill 要解決什麼問題?
- [ ] 目標使用者是誰? (AI / 人類開發者)
- [ ] 需要哪些領域知識?
- [ ] 是否需要與其他 Skill/Agent 協作?

### 步驟 2: 職責定義
確保 Skill 遵循**單一職責原則**:

**✅ 推薦**: 一個 Skill 專注一個領域
```
bdd-testing → 專注 BDD 測試實作
handler → 專注 Handler 業務邏輯
repository-design → 專注 Repository 設計策略
```

**❌ 避免**: 職責過度廣泛
```
fullstack-development → 涵蓋前端、後端、測試、部署...
```

### 步驟 3: 結構設計
標準 Skill 檔案結構:

```
.claude/skills/
└── my-skill/
    ├── skill.md              # 主要 Skill 定義
    ├── references/           # 參考文件 (可選)
    │   └── guide.md
    └── assets/               # 範本檔案 (可選)
        └── template.cs
```

**skill.md frontmatter**:
```markdown
---
name: my-skill
description: 簡短描述 (一句話說明 Skill 用途)
---
```

### 步驟 4: 內容撰寫
標準 Skill 內容結構:

```markdown
# My Skill

## 描述
詳細描述 Skill 的用途與適用情境

## 職責
- 職責 1
- 職責 2
- 職責 3

## 核心原則
列出核心設計原則與最佳實踐

## 工作流程
### 步驟 1: XXX
### 步驟 2: YYY

## 實作範例
提供程式碼範例

## 參考文件
- [相關文件](./references/xxx.md)

## 相關 Skills
- `other-skill` - 關聯說明

## 相關 Agents
- `feature-development` - 使用情境說明
```

## Agent 設計工作流程

### 步驟 1: 角色定位
確定 Agent 的專業角色:

**範例**:
- `architecture-review` → 架構檢視專家
- `testing-strategy` → 測試策略規劃師
- `feature-development` → 功能開發協調者

### 步驟 2: 工具集選擇
根據角色選擇**最小化**工具集:

| 角色類型 | 建議工具 | 範例 |
|---------|---------|------|
| **規劃/分析** | `view`, `grep`, `glob` | `testing-strategy` |
| **實作** | `view`, `grep`, `create`, `edit` | `feature-development` |
| **檢視** | `view`, `grep`, `glob`, `task` | `architecture-review` |
| **協調** | `view`, `grep`, `skill`, `task` | `feature-development` |

**❌ 避免**: 給所有 Agent 完整工具集

### 步驟 3: Handoffs 規劃
設計 Agent 之間的協作流程:

```markdown
---
description: 完整功能開發流程專家
tools: ['view', 'grep', 'create', 'edit', 'skill']
handoffs:
  - label: 選擇 API 開發流程
    agent: api-development
    prompt: 協助選擇 API First 或 Code First 流程
    send: false
  - label: 規劃測試策略
    agent: testing-strategy
    prompt: 為這個功能規劃測試策略
    send: false
---
```

**單向工作流程原則**:
```
需求分析 → API 設計 → 實作 → 測試 → 架構檢視
(避免循環參照)
```

### 步驟 4: 互動流程設計
確保 Agent 在關鍵決策點等待使用者確認:

```markdown
## 互動式決策點

### 決策點 1: 技術選擇
> 請選擇資料庫類型:
> 1. SQL Server
> 2. PostgreSQL
> 3. MySQL

**等待使用者回應後才繼續**

### 決策點 2: 確認配置
顯示配置摘要:
```
即將套用以下配置:
- 資料庫: SQL Server
- 快取: Redis
```
> 確認無誤請輸入 'yes' 繼續
```

## MCP 整合策略

### MCP 配置
**位置**: `.mcp.json`

```json
{
  "mcpServers": {
    "context7-http": {
      "type": "http",
      "url": "https://mcp.context7.com/mcp",
      "headers": {},
      "tools": ["*"]
    }
  }
}
```

### 在 Skill 中整合 MCP
使用 Context7 查詢最新文件:

```markdown
# EF Core 操作技能

## 查詢最新文件
1. 呼叫 `context7-http-resolve-library-id` 解析庫 ID
   - libraryName: "Entity Framework Core"
   - query: "EF Core best practices"

2. 呼叫 `context7-http-query-docs` 查詢文件
   - libraryId: `/microsoft/efcore`
   - query: "DbContext lifecycle management"

## 呼叫次數限制
⚠️ 每個問題最多呼叫 MCP 工具 **3 次**
```

## 設計檢查清單

### Skill 設計檢查
- [ ] 遵循單一職責原則
- [ ] frontmatter 包含 `name` 和 `description`
- [ ] 提供清晰的工作流程步驟
- [ ] 包含實作範例或範本
- [ ] 定義觸發條件與適用情境
- [ ] 參照專案知識庫 (`@CLAUDE.md`, `@docs/...`)
- [ ] 列出相關 Skills 和 Agents

### Agent 設計檢查
- [ ] 清晰的角色定位 (`description`)
- [ ] 最小化工具集 (`tools`)
- [ ] 定義 Handoffs 工作流程 (如適用)
- [ ] 包含互動式決策流程
- [ ] 避免循環參照
- [ ] 輸出結構化報告或檢查清單

### MCP 整合檢查
- [ ] 配置檔案格式正確
- [ ] 限制每個問題最多 3 次呼叫
- [ ] 先 `resolve-library-id` 再 `query-docs`
- [ ] 整合到工作流程中

## 常見反模式識別

### 反模式 1: Skill 職責過度廣泛
**症狀**:
```markdown
# 全能開發 Skill
涵蓋 API、測試、部署、監控、安全...
```

**解決方案**:
拆分為多個專業化 Skill:
- `api-development`
- `bdd-testing`
- `deployment`
- `monitoring`
- `security-review`

### 反模式 2: Agent 未定義工具集
**症狀**:
```markdown
---
description: 測試專家
# ❌ 未定義 tools
---
```

**解決方案**:
```markdown
---
description: 測試專家
tools: ['view', 'grep', 'create', 'edit']
---
```

### 反模式 3: 過度呼叫 MCP
**症狀**:
單一問題呼叫 MCP 超過 5 次

**解決方案**:
限制為最多 3 次:
1. `context7-http-resolve-library-id`
2. `context7-http-query-docs` (第一次)
3. `context7-http-query-docs` (第二次,若需要)

### 反模式 4: Handoffs 循環參照
**症狀**:
```markdown
# Agent A → Agent B → Agent A (循環)
```

**解決方案**:
設計單向工作流程:
```
需求 → 設計 → 實作 → 測試 → 檢視
```

### 反模式 5: 缺乏使用者互動
**症狀**:
Agent 自動執行敏感操作,無使用者確認

**解決方案**:
在關鍵決策點加入確認機制:
```markdown
> 即將執行資料庫遷移...
> 確認無誤請輸入 'yes' 繼續
```

## 實作範例

### 範例 1: 簡單 Skill (無子目錄)
**結構**:
```
.claude/skills/
└── error-handling/
    └── skill.md
```

**skill.md**:
```markdown
---
name: error-handling
description: 錯誤處理與 Result Pattern 技能
---

# Error Handling Skill

## 描述
協助開發者實作統一的錯誤處理機制...

## 職責
- Result Pattern 應用
- Failure 物件建立
- 錯誤碼映射

## 核心原則
1. 業務邏輯不拋出例外
2. 使用 Result<T, Failure> 回傳
3. 保存原始例外到 Failure.Exception
```

### 範例 2: 複雜 Skill (含子目錄)
**結構**:
```
.claude/skills/
└── api-development/
    ├── skill.md
    ├── references/
    │   └── api-development-workflow.md
    └── assets/
        ├── controller-template.cs
        └── openapi-endpoint-template.yml
```

**skill.md**:
```markdown
---
name: api-development
description: API 開發流程引導技能
---

# API Development Skill

## 描述
協助開發者選擇合適的 API 開發流程...

## 職責
- API First vs Code First 流程選擇
- OpenAPI 規格管理
- Controller 程式碼生成

## 參考文件
- [API 開發工作流程](./references/api-development-workflow.md)

## 範本檔案
- [Controller 範本](./assets/controller-template.cs)
- [OpenAPI 端點範本](./assets/openapi-endpoint-template.yml)
```

### 範例 3: Agent 定義
**檔案**: `.claude/agents/architecture-review.agent.md`

```markdown
---
description: 架構檢視與建議專家
tools: ['grep', 'glob', 'view', 'task']
---

# Architecture Review Agent

我是架構檢視與建議專家,分析現有程式碼架構。

## 專業領域
- Repository Pattern 設計評估
- Result Pattern 應用審查
- EF Core 最佳實踐驗證

## 審查流程

### 1. 規範檢查
檢查是否遵循 `@CLAUDE.md` 規範:
- [ ] 使用 Primary Constructor?
- [ ] Repository 使用 `IDbContextFactory<T>`?
- [ ] 讀取查詢使用 `AsNoTracking()`?

### 2. 反模式識別
識別常見反模式:
- Controller 包含業務邏輯
- 直接注入 DbContext
- 未使用 AsNoTracking

### 3. 輸出報告
產生優先級分類的改善建議:

## 架構審查報告

### 🔴 高優先級 (Critical)
1. **問題描述**
   - 位置: `檔案路徑:行號`
   - 影響: 影響描述
   - 建議: 改善建議

### 🟡 中優先級 (Important)
...

### 🟢 低優先級 (Nice to Have)
...
```

## 使用方式

### 設計新的 Skill
```
我需要設計一個處理 XXX 的 Skill
```

本 Skill 會引導您:
1. 分析需求與職責定義
2. 設計檔案結構
3. 撰寫 Skill 內容
4. 驗證設計品質

### 設計新的 Agent
```
我需要設計一個 XXX 專家的 Agent
```

本 Skill 會引導您:
1. 確定角色定位
2. 選擇工具集
3. 規劃 Handoffs
4. 設計互動流程

### 檢視現有設計
```
請檢視 @.claude/skills/my-skill/SKILL.md 的設計品質
```

本 Skill 會:
1. 識別設計反模式
2. 提供改善建議
3. 檢查是否符合最佳實踐

## 相關 Skills
- `project-init` - 專案初始化時可能需要建立 Skill
- `bdd-practices` - BDD 實踐參考

## 相關 Agents
- `project-setup` - 專案初始化協調
- `feature-development` - 功能開發協調

## 核心原則提醒

### Skill 設計三原則
1. **單一職責**: 一個 Skill 專注一個領域
2. **結構化輸出**: 提供清晰的步驟與檢查清單
3. **知識參照**: 明確指出依賴的知識來源

### Agent 設計三原則
1. **角色明確**: 清晰的專業定位
2. **工具最小化**: 只啟用必要工具
3. **互動優先**: 關鍵決策等待使用者確認

### MCP 整合三原則
1. **呼叫限制**: 每個問題最多 3 次
2. **順序正確**: 先 resolve-library-id 再 query-docs
3. **錯誤處理**: MCP 失敗不影響主流程

## 成功指標
- [ ] Skill/Agent 職責清晰明確
- [ ] 提供結構化的工作流程
- [ ] 包含實作範例或範本
- [ ] 定義與其他 Skill/Agent 的關係
- [ ] 遵循最佳實踐,無反模式
- [ ] 文件清晰,易於 AI 與人類理解

## 錯誤處理

### 常見錯誤 1: Skill 名稱不一致
```markdown
# ❌ 錯誤
檔案名: my-skill/skill.md
frontmatter name: mySkill  # 不一致

# ✅ 正確
檔案名: my-skill/skill.md
frontmatter name: my-skill  # 一致
```

### 常見錯誤 2: Agent 缺少 description
```markdown
# ❌ 錯誤
---
tools: ['view', 'grep']
# 缺少 description
---

# ✅ 正確
---
description: 清晰的角色描述
tools: ['view', 'grep']
---
