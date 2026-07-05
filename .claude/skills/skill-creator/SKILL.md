---
name: skill-creator
description: Skill 與 Agent 建立指導專家，協助開發者依據 GitHub Copilot 最佳實踐建立高品質的 custom agent 和 skill 定義檔案，包含 YAML frontmatter、MCP 整合與互動式工作流程設計。
---

# Skill Creator - Custom Agent & Skill 建立最佳實踐指南

## 描述
Skill Creator 是專門協助建立 GitHub Copilot Custom Agent 與 Skill 的指導技能，提供結構化的建立流程、檔案風格範本、MCP 整合指導與互動式設計模式。

## 職責
- 引導建立符合規範的 skill/agent 定義檔案
- 提供 YAML frontmatter 配置範本
- 協助設計互動式工作流程
- 整合 MCP Server 工具
- 建立清晰的職責與能力說明

## 核心概念

### Agent vs Skill 差異

| 比較項目 | Custom Agent | Skill |
|---------|-------------|-------|
| **檔案位置** | `.claude/agents/` 或 `.claude/agents/` | `.claude/skills/` |
| **檔案副檔名** | `.agent.md` 或 `.md` | `skill.md` |
| **啟用方式** | `@agent-name` | 通常由 agent 或 workspace 呼叫 |
| **主要用途** | 獨立的專業助理，具備特定領域知識 | 可重複使用的技能模組，供 agent 組合使用 |
| **工具配置** | 可在 YAML 中配置 MCP 工具 | 通常繼承環境的工具配置 |
| **複雜度** | 較高，需處理完整對話流程 | 較低，專注於特定技能 |

### 何時建立 Agent vs Skill

**建立 Agent 的時機**：
- 需要完整的對話流程與決策能力
- 包含多個步驟的複雜工作流程
- 需要協調多個 skills 的執行
- 例如：`feature-development-agent`、`testing-strategy-agent`

**建立 Skill 的時機**：
- 提供單一職責的專業能力
- 可被多個 agents 重複使用
- 專注於特定技術領域或工具
- 例如：`handler`、`bdd-testing`、`api-development`

## 檔案結構規範

### YAML Frontmatter 完整屬性

```yaml
---
name: skill-or-agent-name           # 必填：唯一識別名稱（小寫、連字號分隔）
description: 簡短描述此 skill/agent 的用途與核心能力  # 必填：一句話說明
tools: ["read", "edit", "bash"]     # 可選：指定可用工具，省略則使用全部
target: vscode                      # 可選：指定環境（vscode 或 github-copilot）
model: claude-3.5-sonnet            # 可選：指定 AI 模型（僅限 IDE 環境）
mcp-servers:                        # 可選：MCP Server 配置（組織/企業級 agent）
  server-name:
    type: local                     # local | http | sse
    command: node
    args: ["path/to/server.js"]
    tools: ["*"]                    # "*" 或特定工具列表
    env:
      API_KEY: $SECRET_NAME
---
```

### Markdown 內容架構

```markdown
# Skill/Agent 名稱

## 描述
詳細說明此 skill/agent 的用途、目標與適用場景

## 職責
- 職責 1：具體說明
- 職責 2：具體說明
- 職責 3：具體說明

## 能力
### 1. 核心能力 A
具體描述與範例

### 2. 核心能力 B
具體描述與範例

## 使用方式
### 在 GitHub Copilot 中使用
具體的呼叫範例

### 直接呼叫（如果是 Agent）
@agent-name 任務描述

## 互動流程（可選）
使用 Mermaid 圖表或文字描述互動流程

## 核心模式/原則
關鍵的設計模式或必須遵循的原則

## 範本檔案（可選）
相關的程式碼範本或參考資料

## 注意事項
### 🔒 核心原則
不可妥協的關鍵原則

### 📋 最佳實踐
建議遵循的做法

### ✅ 成功指標
檢查清單形式的驗收標準

## 錯誤處理（可選）
常見錯誤與解決方案

## 相關 Skills（可選）
其他相關的 skills 連結

## 相關 Agents（可選）
會使用此 skill 的 agents
```

## 建立流程

### 步驟 1：需求分析
**互動問題**：
```
我將協助您建立新的 Skill/Agent，請回答以下問題：

1️⃣ 您想建立什麼？
   A. Custom Agent（獨立的專業助理）
   B. Skill（可重複使用的技能模組）

2️⃣ 主要用途是什麼？（一句話描述）

3️⃣ 核心職責有哪些？（列出 3-5 項）

4️⃣ 需要哪些工具支援？
   A. 所有可用工具（預設）
   B. 特定工具（read, edit, bash, search 等）
   C. 需要 MCP Server 整合

5️⃣ 是否需要互動式決策流程？
   A. 是，需要引導使用者做決策
   B. 否，直接執行任務
```

### 步驟 2：YAML Frontmatter 設計

**必填欄位**：
```yaml
---
name: descriptive-name        # 描述性名稱，使用 kebab-case
description: 簡潔的單行描述   # 說明核心用途與能力
---
```

**工具配置範例**：
```yaml
# 範例 1：使用所有工具（預設行為）
---
name: full-stack-developer
description: 全端開發專家
# tools 省略 = 所有工具
---

# 範例 2：限制特定工具
---
name: test-specialist
description: 測試專家，只分析與撰寫測試程式碼
tools: ["read", "edit", "search"]  # 不能執行 bash 命令
---

# 範例 3：包含 MCP 工具
---
name: api-developer
description: API 開發專家，整合 OpenAPI 工具
tools: ["read", "edit", "bash", "context7-http/query-docs"]
---
```

**MCP Server 整合（組織級 Agent）**：
```yaml
---
name: documentation-agent
description: 文件管理專家，整合 Context7 知識庫
tools: ["read", "edit", "context7-http/*"]
mcp-servers:
  context7-http:
    type: http
    url: https://mcp.context7.com/mcp
    headers: {}
    tools: ["*"]
---
```

### 步驟 3：職責與能力定義

**職責（Responsibilities）**：
- 使用簡潔的條列式
- 每項職責對應一個明確的行動
- 3-7 項為佳

**範例**：
```markdown
## 職責
- 分析現有專案架構與程式碼結構
- 識別架構反模式與潛在問題
- 提供符合 Clean Architecture 的重構建議
- 產生架構改善計畫與實作步驟
- 驗證重構後的程式碼是否符合規範
```

**能力（Capabilities）**：
- 分段說明不同面向的專業能力
- 包含具體範例或程式碼片段
- 說明如何整合其他工具或技術

**範例**：
```markdown
## 能力

### 1. 架構分析
- 掃描專案資料夾結構
- 識別分層架構違規（Controller 直接呼叫 DbContext）
- 檢測循環相依問題
- 分析命名空間組織是否符合領域劃分

### 2. 重構建議
- 提供分層架構調整建議
- 推薦適當的設計模式（Repository, Result Pattern）
- 產生重構檢查清單與優先順序排序
```

### 步驟 4：互動式工作流程設計

**何時需要互動式流程**：
- 需要使用者做決策（選擇技術方案、設計策略）
- 多步驟的複雜流程
- 條件式分支執行

**流程圖範例（使用 Mermaid）**：
```markdown
## 互動流程

\`\`\`mermaid
graph TD
    A[啟動 Skill] --> B{詢問開發流程}
    B -->|API First| C[檢查 OpenAPI 規格]
    B -->|Code First| D[直接實作程式碼]
    C -->|已定義| E[產生 Server Controller]
    C -->|需更新| F[協助更新規格]
    F --> E
    E --> G[提供實作範本]
    D --> G
    G --> H[詢問是否產生 Client SDK]
    H -->|是| I[執行 codegen-api-client]
    H -->|否| J[完成]
    I --> J
\`\`\`
```

**問答範例**：
```markdown
## 互動問答範例

### 問題 1：選擇開發流程
\`\`\`
請選擇 API 開發流程：

1️⃣ API First（推薦）
   ✅ 優點：文件與實作同步、前後端並行開發
   ⚠️ 需要：先設計 OpenAPI 規格
   
   適用場景：
   - 團隊協作、前後端分離專案
   - 需要提供 Client SDK

2️⃣ Code First
   ✅ 優點：快速開發、直接實作
   ⚠️ 需要：手動維護 API 文件
   
   適用場景：
   - 快速原型開發
   - 小型專案或單人開發
\`\`\`

### 問題 2：確認設計策略
\`\`\`
根據您的需求分析，建議使用：

📌 **需求導向 Repository 設計**

原因：
✓ 涉及 4 個資料表（Orders, OrderItems, Inventory, Payments）
✓ 需要交易一致性保證
✓ 包含複雜的多步驟業務流程

是否確認使用此策略？
[ ] 是，使用需求導向
[ ] 否，我想使用資料表導向
[ ] 需要更多說明
\`\`\`
```

### 步驟 5：核心模式與原則

**核心原則格式**：
```markdown
## 核心原則

### 🔒 不可妥協的原則
1. **強制詢問**：不得擅自假設開發流程，必須明確詢問使用者
2. **Result Pattern 必用**：所有業務邏輯必須使用 Result<T, Failure>
3. **安全優先**：所有敏感操作需使用者確認

### 📋 最佳實踐
1. **職責分離**：Controller 不包含業務邏輯
2. **錯誤處理**：由 Middleware 統一記錄，Handler 不記錄
3. **命名一致性**：遵循專案命名規範（CreateXxxRequest, XxxResponse）

### ✅ 成功指標
- [ ] 所有互動問題都已回答
- [ ] 產生的程式碼通過編譯
- [ ] 符合專案架構規範
- [ ] 測試覆蓋率達標
```

### 步驟 6：範本與參考文件

**範本檔案組織**：
```
.claude/skills/your-skill/
├── skill.md                    # 主要定義檔案
├── assets/                     # 程式碼範本
│   ├── template-1.cs
│   ├── template-2.yml
│   └── workflow-diagram.mmd
└── references/                 # 參考文件
    ├── best-practices.md
    └── detailed-guide.md
```

**範本引用範例**：
```markdown
## 範本檔案

### Controller 實作範本
完整範本請參考：`assets/controller-template.cs`

### OpenAPI 端點範本
YAML 規格範本：`assets/openapi-endpoint-template.yml`

## 參考文件
- [API 開發工作流程詳解](./references/api-development-workflow.md)
- [Result Pattern 完整指南](./references/result-pattern-guide.md)
```

## MCP Server 整合指南

### MCP Server 類型

#### 1. Local MCP Server
```yaml
mcp-servers:
  local-analyzer:
    type: local
    command: node
    args: ["./tools/code-analyzer/server.js"]
    tools: ["analyze-architecture", "detect-anti-patterns"]
    env:
      LOG_LEVEL: info
```

#### 2. HTTP MCP Server
```yaml
mcp-servers:
  context7-http:
    type: http
    url: https://mcp.context7.com/mcp
    headers:
      Authorization: Bearer ${CONTEXT7_API_KEY}
    tools: ["query-docs", "resolve-library-id"]
```

#### 3. SSE (Server-Sent Events) MCP Server
```yaml
mcp-servers:
  streaming-analyzer:
    type: sse
    url: https://api.example.com/mcp/sse
    tools: ["stream-analysis"]
```

### MCP 工具使用範例

**在 tools 屬性中引用 MCP 工具**：
```yaml
---
name: documentation-expert
description: 文件專家，整合 Context7 知識庫查詢
tools: 
  - read
  - edit
  - context7-http/query-docs          # MCP 工具格式：server-name/tool-name
  - context7-http/resolve-library-id
---

# Documentation Expert

當您需要查詢最新的函式庫文件時，我會：
1. 使用 `context7-http/resolve-library-id` 找到正確的函式庫 ID
2. 使用 `context7-http/query-docs` 查詢相關文件
3. 整理並提供最佳實踐建議
```

### MCP Server 配置位置

**Repository 層級（推薦）**：
```
.mcp.json         # GitHub Copilot
.claude/mcp-config.json          # Claude Desktop
```

**組織層級 Agent（GitHub）**：
```yaml
# 直接在 agent profile 中配置
---
name: org-level-agent
description: 組織級別的 agent
mcp-servers:
  custom-mcp:
    type: local
    command: npx
    args: ["-y", "@org/custom-mcp-server"]
    tools: ["*"]
---
```

## 實戰範例

### 範例 1：Skill - 單一職責技能

```markdown
---
name: error-handling
description: Result Pattern 錯誤處理技能，協助開發者正確使用 Result<T, Failure> 模式處理業務邏輯錯誤。
tools: ["read", "edit"]
---

# Error Handling Skill

## 描述
Result Pattern 錯誤處理技能，專注於協助開發者在 Handler 和 Repository 層正確實作錯誤處理機制。

## 職責
- 指導正確使用 Result Pattern
- 協助建立 Failure 物件
- 避免拋出業務邏輯例外
- 確保原始例外被保存

## 能力

### 1. Result Pattern 實作
確保所有業務邏輯方法回傳 `Result<TSuccess, Failure>` 而非拋出例外。

正確範例：
\`\`\`csharp
public async Task<Result<Member, Failure>> CreateMemberAsync(
    CreateMemberRequest request,
    CancellationToken cancellationToken = default)
{
    // 業務驗證
    if (await _repository.EmailExistsAsync(request.Email, cancellationToken))
    {
        return Result.Failure<Member, Failure>(new Failure
        {
            Code = nameof(FailureCode.DuplicateEmail),
            Message = "此 Email 已被註冊",
            TraceId = _traceContext.TraceId
        });
    }
    
    // ... 正常流程
}
\`\`\`
```

### 範例 2：Agent - 完整工作流程助理

```markdown
---
name: feature-development
description: 完整功能開發流程專家，串接多個 Skills 引導使用者完成從需求分析到測試驗證的完整開發週期。
tools: ["read", "edit", "bash", "search"]
---

# Feature Development Agent

## 描述
完整功能開發流程專家，我會協調多個專業 skills 引導您完成功能開發的每個階段。

## 工作流程

\`\`\`mermaid
graph TD
    A[需求分析] --> B{API 開發流程選擇}
    B -->|API First| C[OpenAPI 規格設計]
    B -->|Code First| D[直接實作]
    C --> E[產生 Controller 骨架]
    D --> F[Repository 設計策略]
    E --> F
    F --> G[Handler 業務邏輯實作]
    G --> H[Controller 實作]
    H --> I[測試策略規劃]
\`\`\`
```

## 最佳實踐總結

### ✅ Skill 設計原則
1. **單一職責**：一個 skill 專注一個明確的能力領域
2. **可組合性**：設計為可被多個 agents 重複使用
3. **清晰介面**：明確定義輸入期待與輸出格式
4. **範本驅動**：提供程式碼範本加速開發

### ✅ Agent 設計原則
1. **完整流程**：涵蓋從開始到結束的完整工作流程
2. **互動決策**：關鍵決策點必須詢問使用者
3. **Skill 協調**：善用現有 skills，避免重複實作
4. **狀態追蹤**：記錄流程進度，支援中斷續作

### ✅ YAML Frontmatter 原則
1. **name**：使用描述性的 kebab-case 命名
2. **description**：一句話說明，< 120 字元
3. **tools**：僅列出必要工具，預設為全部
4. **model**：除非有特殊需求，否則省略（使用預設）

### ✅ Markdown 內容原則
1. **結構清晰**：使用標準章節（描述、職責、能力、使用方式）
2. **範例豐富**：提供具體的程式碼範例與對話範例
3. **視覺化**：使用 Mermaid 圖表、表格、核取方塊增強可讀性
4. **參考完整**：連結相關 skills、agents、範本檔案

### ✅ MCP 整合原則
1. **環境配置優先**：盡可能在 `.mcp.json` 配置
2. **工具明確引用**：使用 `server-name/tool-name` 格式
3. **環境變數安全**：敏感資訊使用環境變數，不寫死在配置中
4. **錯誤處理**：說明 MCP 工具失敗時的降級策略

## 檔案位置與命名

### Skill 檔案組織
```
.claude/skills/
├── skill-name/
│   ├── skill.md              # 主要定義（必須）
│   ├── assets/               # 範本檔案（可選）
│   └── references/           # 參考文件（可選）
```

### Agent 檔案組織
```
.claude/agents/
├── agent-name.agent.md       # 方式 1：單檔案
或
├── agent-name/
│   └── agent.md              # 方式 2：資料夾組織
```

## 測試與驗證

### Skill/Agent 品質檢查清單

**YAML Frontmatter**：
- [ ] `name` 使用 kebab-case
- [ ] `description` < 120 字元且清楚描述用途
- [ ] `tools` 列表正確（如有指定）
- [ ] MCP Server 配置正確（如有使用）

**Markdown 內容**：
- [ ] 包含「描述」、「職責」、「能力」章節
- [ ] 提供具體的使用範例
- [ ] 包含互動流程說明（如適用）
- [ ] 列出相關 Skills/Agents（如有）

**互動設計**：
- [ ] 決策點都有明確的問題與選項
- [ ] 提供使用情境與範例對話
- [ ] 說明預期的互動流程

## 參考資源

### 官方文件
- [GitHub Copilot Custom Agents 官方文件](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
- [MCP Server 配置參考](https://docs.github.com/en/copilot/reference/custom-agents-configuration)

### 專案內範例
- `.claude/skills/api-development/` - 複雜互動流程範例
- `.claude/skills/handler/` - 單一職責 Skill 範例
- `.claude/agents-workflow-guide.md` - Agent 工作流程設計
