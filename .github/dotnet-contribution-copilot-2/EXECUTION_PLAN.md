# GitHub Copilot Skills & Agents 執行計劃書

## 📋 專案目標
根據 CLAUDE.md 檔案內容，建立 GitHub Copilot 的 skills 和 agents，用於協助 .NET Core WebAPI 專案開發。

## 🎯 設計原則
1. **Skills**: 單一職責、可重用、參考外部文件
2. **Agents**: 串接多個 skills、可與用戶互動、工作流程導向
3. **檔案組織**: 清晰的目錄結構、易於維護

## 📁 檔案結構

```
.github/dotnet-contribution-copilot-2/
├── EXECUTION_PLAN.md          # 本執行計劃書
├── skills/                    # Skills 定義
│   ├── project-init.md        # 專案初始化 skill
│   ├── bdd-test.md           # BDD 測試 skill
│   ├── api-dev.md            # API 開發 skill
│   ├── database-ops.md       # 資料庫操作 skill
│   ├── code-review.md        # 程式碼審查 skill
│   └── architecture.md       # 架構設計 skill
├── agents/                    # Agents 定義
│   ├── project-setup-agent.md      # 專案設定 agent
│   ├── feature-dev-agent.md        # 功能開發 agent
│   └── quality-assurance-agent.md  # 品質保證 agent
├── assets/                    # 程式碼範本
│   ├── controller-template.cs
│   ├── handler-template.cs
│   ├── repository-template.cs
│   ├── bdd-feature-template.feature
│   └── bdd-steps-template.cs
└── references/                # 參考文件
    ├── architecture-guide.md
    ├── bdd-workflow.md
    ├── ef-core-best-practices.md
    ├── trace-context-guide.md
    └── testing-strategy.md
```

## 🔧 Skills 列表

### 1. project-init (專案初始化)
- **職責**: 檢測專案狀態、執行初始化對話、配置專案
- **參考**: architecture-guide.md
- **互動**: 需要與用戶互動確認配置選項

### 2. bdd-test (BDD 測試)
- **職責**: 建立 BDD 測試、產生 .feature 檔案、實作測試步驟
- **參考**: bdd-workflow.md, testing-strategy.md
- **範本**: bdd-feature-template.feature, bdd-steps-template.cs

### 3. api-dev (API 開發)
- **職責**: 建立 Controller/Handler/Repository、遵循分層架構
- **參考**: architecture-guide.md
- **範本**: controller-template.cs, handler-template.cs, repository-template.cs

### 4. database-ops (資料庫操作)
- **職責**: EF Core Migration、反向工程、資料庫更新
- **參考**: ef-core-best-practices.md
- **工具**: 使用 Taskfile 執行 EF Core 指令

### 5. code-review (程式碼審查)
- **職責**: 檢查程式碼是否符合專案規範、最佳實踐
- **參考**: architecture-guide.md, ef-core-best-practices.md
- **檢查項目**: 命名規範、分層設計、錯誤處理、測試覆蓋

### 6. architecture (架構設計)
- **職責**: 協助設計系統架構、選擇設計模式
- **參考**: architecture-guide.md, trace-context-guide.md
- **建議**: Repository 設計策略、中介軟體設計

## 🤖 Agents 列表

### 1. project-setup-agent (專案設定代理)
**工作流程**:
1. 使用 `project-init` skill 檢測專案狀態
2. 詢問用戶是否使用 GitHub 範本
3. 詢問資料庫/快取/專案結構配置
4. 產生 `env/.template-config.json`
5. 使用 `architecture` skill 建議專案結構

**互動點**:
- GitHub 範本選擇
- 資料庫類型選擇 (SQL Server/PostgreSQL/MySQL)
- Redis 快取需求
- 專案組織方式 (單一專案 vs 多專案)

### 2. feature-dev-agent (功能開發代理)
**工作流程**:
1. 詢問 API 開發流程 (API First vs Code First)
2. 詢問需要實作的分層 (Controller/Handler/Repository)
3. 使用 `api-dev` skill 產生程式碼骨架
4. 詢問測試需求
5. 使用 `bdd-test` skill 建立測試
6. 使用 `code-review` skill 檢查程式碼

**互動點**:
- API 開發流程選擇
- 分層實作範圍
- 測試策略選擇
- OpenAPI 規格狀態

### 3. quality-assurance-agent (品質保證代理)
**工作流程**:
1. 使用 `code-review` skill 檢查程式碼品質
2. 使用 `bdd-test` skill 確認測試覆蓋率
3. 檢查 Docker 測試環境設定
4. 提供改善建議

**互動點**:
- 選擇檢查範圍 (全專案/特定功能)
- 選擇檢查項目 (命名/架構/測試/效能)

## 📝 參考文件內容對應

### architecture-guide.md
- 分層架構設計
- Repository Pattern 設計哲學
- 專案結構組織方式
- TraceContext 設計模式

### bdd-workflow.md
- BDD 開發循環
- Docker 優先測試策略
- Gherkin 語法規範
- 測試環境設定

### ef-core-best-practices.md
- Migration 管理
- 反向工程流程
- Code First vs Database First
- Taskfile 整合

### trace-context-guide.md
- 不可變物件設計
- 中介軟體實作
- 用戶資訊管理
- 依賴注入模式

### testing-strategy.md
- 測試方法選擇
- Testcontainers 使用
- 測試替身策略
- API 控制器測試指引

## ✅ 實作檢查清單

- [x] 建立目錄結構
- [ ] 建立 6 個 skills
- [ ] 建立 3 個 agents
- [ ] 建立 5 個程式碼範本
- [ ] 建立 5 個參考文件
- [ ] 驗證 skills 與 agents 的整合性
- [ ] 測試互動流程的完整性

## 🚀 使用方式

### 使用 Agent
```bash
# 初始化新專案
@project-setup-agent 請幫我初始化專案

# 開發新功能
@feature-dev-agent 請幫我實作會員註冊功能

# 程式碼審查
@quality-assurance-agent 請檢查目前的程式碼品質
```

### 使用 Skill
```bash
# 單獨使用 skill
@api-dev 建立會員管理的 Controller, Handler, Repository

@bdd-test 為會員註冊功能建立 BDD 測試

@database-ops 建立新的 Migration: AddMemberTable
```

## 📌 注意事項

1. **所有 skills 都必須遵循 CLAUDE.md 的互動原則**
2. **禁止擅自假設用戶選擇，必須明確詢問**
3. **優先使用 Taskfile 執行開發指令**
4. **程式碼範本必須包含完整的錯誤處理與日誌記錄**
5. **所有 API 測試必須使用 BDD 方法**
6. **優先使用 Testcontainers，避免使用 Mock**
