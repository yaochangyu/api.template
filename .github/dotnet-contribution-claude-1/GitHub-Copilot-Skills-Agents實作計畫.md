# GitHub Copilot Skills & Agents 實作計畫

## 目標
根據 CLAUDE.md 檔案內容，建立一系列 GitHub Copilot skill 和 agent，協助開發者遵循專案規範進行開發。

## 分析 CLAUDE.md 核心領域

從 CLAUDE.md 分析出以下主要開發領域：

1. **專案初始化** - 專案狀態檢測、配置設定、GitHub 範本套用
2. **API 開發流程** - API First vs Code First、OpenAPI 規格管理
3. **BDD 測試** - 整合測試、測試策略、Docker 測試環境
4. **Repository 設計** - 資料表導向 vs 需求導向設計哲學
5. **錯誤處理** - Result Pattern、Failure 物件、分層錯誤處理
6. **中介軟體** - TraceContext、ExceptionHandling、請求日誌
7. **效能最佳化** - 快取策略、EF Core 查詢最佳化

## Skill 設計規劃

### 核心原則
- 每個 skill 職責單一明確
- 使用 reference 檔案引用詳細規範
- 包含實作範本（template）檔案
- 提供互動式問答引導

### 規劃的 Skills

#### 1. **project-init**
- **職責**：專案初始化與配置
- **能力**：
  - 檢測專案狀態（是否為空白專案）
  - 引導使用者選擇配置（資料庫、快取、專案結構）
  - 套用 GitHub 範本（可選）
  - 產生 `env/.template-config.json`
- **參考檔案**：`references/project-initialization.md`
- **互動流程**：結構化詢問 → 驗證選項 → 執行配置

#### 2. **api-development**
- **職責**：API 開發流程引導
- **能力**：
  - 引導選擇 API First 或 Code First
  - 協助更新 OpenAPI 規格
  - 產生 Server Controller 骨架
  - 產生 Client SDK
- **參考檔案**：`references/api-development-workflow.md`
- **範本檔案**：
  - `assets/controller-template.cs`
  - `assets/openapi-endpoint-template.yml`

#### 3. **bdd-testing**
- **職責**：BDD 測試策略與實作
- **能力**：
  - 詢問測試策略（BDD/單元測試/兩者/暫不測試）
  - 協助撰寫 Gherkin `.feature` 檔案
  - 產生測試步驟實作程式碼
  - 設定 Docker 測試環境
- **參考檔案**：`references/bdd-testing-guide.md`
- **範本檔案**：
  - `assets/feature-template.feature`
  - `assets/test-steps-template.cs`

#### 4. **repository-design**
- **職責**：Repository 設計指導
- **能力**：
  - 分析業務需求複雜度
  - 建議設計策略（資料表導向/需求導向/混合）
  - 提供決策檢查清單
  - 產生 Repository 程式碼
- **參考檔案**：`references/repository-design-philosophy.md`
- **範本檔案**：
  - `assets/repository-table-oriented-template.cs`
  - `assets/repository-domain-oriented-template.cs`

#### 5. **handler**
- **職責**：Handler 業務邏輯層實作
- **能力**：
  - 實作業務邏輯
  - 整合 Repository
  - Result Pattern 錯誤處理
  - CancellationToken 支援
- **參考檔案**：`references/handler-best-practices.md`
- **範本檔案**：`assets/handler-template.cs`

#### 6. **error-handling**
- **職責**：錯誤處理與 Result Pattern
- **能力**：
  - Result Pattern 應用指導
  - Failure 物件建立
  - FailureCode 映射
  - 分層錯誤處理策略
- **參考檔案**：`references/error-handling-guide.md`
- **範本檔案**：`assets/failure-template.cs`

#### 7. **middleware**
- **職責**：中介軟體實作
- **能力**：
  - TraceContext 管理
  - Exception Handling
  - Request Logging
  - 中介軟體管線配置
- **參考檔案**：`references/middleware-architecture.md`
- **範本檔案**：`assets/middleware-template.cs`

#### 8. **ef-core**
- **職責**：EF Core 操作與最佳化
- **能力**：
  - DbContextFactory 使用
  - AsNoTracking 查詢最佳化
  - Migration 管理
  - 反向工程指導
- **參考檔案**：`references/ef-core-best-practices.md`
- **範本檔案**：`assets/dbcontext-usage-template.cs`

## Agent 設計規劃

### 核心原則
- Agent 串接多個 skill 形成完整工作流程
- 具備互動能力，引導使用者決策
- 遵循 CLAUDE.md 的強制互動原則

### 規劃的 Agents

#### 1. **feature-development-agent**
- **職責**：完整功能開發流程
- **工作流程**：
  1. 需求分析
  2. 【互動】API 開發流程選擇（呼叫 `api-development`）
  3. 【互動】測試策略選擇（呼叫 `bdd-testing`）
  4. 撰寫 BDD 情境（如需要）
  5. 實作 Controller（呼叫 `api-development`）
  6. 實作 Handler（呼叫 `handler`）
  7. 實作 Repository（呼叫 `repository-design`）
  8. 實作測試（呼叫 `bdd-testing`）
  9. 執行測試驗證
- **使用的 Skills**：
  - `api-development`
  - `bdd-testing`
  - `handler`
  - `repository-design`
  - `error-handling`

#### 2. **project-setup-agent**
- **職責**：專案初始化與配置
- **工作流程**：
  1. 檢測專案狀態
  2. 【互動】是否使用 GitHub 範本
  3. 【互動】資料庫選擇
  4. 【互動】快取策略
  5. 【互動】專案結構組織
  6. 執行初始化
  7. 產生配置檔案
- **使用的 Skills**：
  - `project-init`

#### 3. **testing-strategy-agent**
- **職責**：測試策略規劃與實作
- **工作流程**：
  1. 【互動】測試需求分析
  2. 【互動】測試範圍確認
  3. 【互動】測試環境設定
  4. 撰寫測試情境
  5. 實作測試步驟
  6. 配置測試基礎設施（Docker）
- **使用的 Skills**：
  - `bdd-testing`

#### 4. **architecture-review-agent**
- **職責**：架構檢視與建議
- **工作流程**：
  1. 分析現有程式碼結構
  2. 檢查是否遵循 CLAUDE.md 規範
  3. 提供改善建議
  4. 識別架構反模式
- **使用的 Skills**：
  - `repository-design`
  - `error-handling`
  - `middleware`
  - `ef-core`

## 檔案結構規劃

```
.github/dotnet-contribution-claude-1/
├── skills/
│   ├── project-init/
│   │   ├── skill.md
│   │   └── references/
│   │       └── project-initialization.md
│   ├── api-development/
│   │   ├── skill.md
│   │   ├── references/
│   │   │   └── api-development-workflow.md
│   │   └── assets/
│   │       ├── controller-template.cs
│   │       └── openapi-endpoint-template.yml
│   ├── bdd-testing/
│   │   ├── skill.md
│   │   ├── references/
│   │   │   └── bdd-testing-guide.md
│   │   └── assets/
│   │       ├── feature-template.feature
│   │       └── test-steps-template.cs
│   ├── repository-design/
│   │   ├── skill.md
│   │   ├── references/
│   │   │   └── repository-design-philosophy.md
│   │   └── assets/
│   │       ├── repository-table-oriented-template.cs
│   │       └── repository-domain-oriented-template.cs
│   ├── handler/
│   │   ├── skill.md
│   │   ├── references/
│   │   │   └── handler-best-practices.md
│   │   └── assets/
│   │       └── handler-template.cs
│   ├── error-handling/
│   │   ├── skill.md
│   │   ├── references/
│   │   │   └── error-handling-guide.md
│   │   └── assets/
│   │       └── failure-template.cs
│   ├── middleware/
│   │   ├── skill.md
│   │   ├── references/
│   │   │   └── middleware-architecture.md
│   │   └── assets/
│   │       └── middleware-template.cs
│   └── ef-core/
│       ├── skill.md
│       ├── references/
│       │   └── ef-core-best-practices.md
│       └── assets/
│           └── dbcontext-usage-template.cs
├── agents/
│   ├── feature-development-agent/
│   │   └── agent.md
│   ├── project-setup-agent/
│   │   └── agent.md
│   ├── testing-strategy-agent/
│   │   └── agent.md
│   └── architecture-review-agent/
│       └── agent.md
└── README.md
```

## 實作步驟

### 階段 1：基礎 Skills 建立（8 個步驟）
- [ ] **步驟 1**：建立 `project-init`
  - 為什麼：專案初始化是開發的第一步，必須優先完成
  - 產出：skill.md + references/project-initialization.md

- [ ] **步驟 2**：建立 `api-development`
  - 為什麼：API 開發是核心流程，影響後續所有開發
  - 產出：skill.md + references + templates

- [ ] **步驟 3**：建立 `handler`
  - 為什麼：Handler 是業務邏輯核心層
  - 產出：skill.md + references + template

- [ ] **步驟 4**：建立 `repository-design`
  - 為什麼：Repository 設計影響資料存取架構
  - 產出：skill.md + references + templates

- [ ] **步驟 5**：建立 `error-handling`
  - 為什麼：錯誤處理是跨層級的核心機制
  - 產出：skill.md + references + template

- [ ] **步驟 6**：建立 `bdd-testing`
  - 為什麼：測試是品質保證的基礎
  - 產出：skill.md + references + templates

- [ ] **步驟 7**：建立 `middleware`
  - 為什麼：中介軟體處理跨領域關注點
  - 產出：skill.md + references + template

- [ ] **步驟 8**：建立 `ef-core`
  - 為什麼：EF Core 是資料存取的核心技術
  - 產出：skill.md + references + template

### 階段 2：Agent 建立（4 個步驟）
- [ ] **步驟 9**：建立 `feature-development-agent`
  - 為什麼：串接完整功能開發流程，最常使用
  - 依賴：需要步驟 2, 3, 4, 5, 6
  - 產出：agent.md

- [ ] **步驟 10**：建立 `project-setup-agent`
  - 為什麼：引導新專案初始化
  - 依賴：需要步驟 1
  - 產出：agent.md

- [ ] **步驟 11**：建立 `testing-strategy-agent`
  - 為什麼：專注於測試策略與實作
  - 依賴：需要步驟 6
  - 產出：agent.md

- [ ] **步驟 12**：建立 `architecture-review-agent`
  - 為什麼：提供架構檢視與建議
  - 依賴：需要步驟 4, 5, 7, 8
  - 產出：agent.md

### 階段 3：文件與整合（1 個步驟）
- [ ] **步驟 13**：建立主 README.md
  - 為什麼：提供整體使用指南與索引
  - 產出：README.md（包含所有 skill 和 agent 的使用說明）

## 預期效益

1. **標準化開發流程**：透過 skill 和 agent 確保開發遵循 CLAUDE.md 規範
2. **提升開發效率**：減少重複性工作，自動產生符合規範的程式碼
3. **降低學習曲線**：新成員透過互動式引導快速上手
4. **品質保證**：內建最佳實踐，減少架構錯誤
5. **可維護性**：單一職責的 skill 設計便於後續維護與擴展

## 注意事項

1. 所有互動必須遵循 CLAUDE.md 的「強制互動原則」
2. 每個 skill 保持職責單一，避免過度複雜
3. 範本檔案必須符合專案實際架構
4. Reference 檔案應從 CLAUDE.md 提取精華，避免重複
5. Agent 的工作流程要清晰，明確標示互動點

## 成功指標

- [ ] 所有 skill 都有完整的 reference 和 template
- [ ] Agent 能正確串接 skill 形成工作流程
- [ ] 互動式問答符合 CLAUDE.md 規範
- [ ] 產生的程式碼符合專案架構
- [ ] 文件清晰易懂，範例完整
