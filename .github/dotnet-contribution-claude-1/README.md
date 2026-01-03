# GitHub Copilot Skills & Agents for ASP.NET Core Web API

本專案提供一系列 GitHub Copilot Skills 和 Agents，協助開發者遵循 CLAUDE.md 規範進行 ASP.NET Core Web API 開發。

## 📦 包含內容

### 🎯 Skills (8 個)

#### 1. [project-init](./skills/project-init/)
**專案初始化與配置**
- 檢測專案狀態
- 互動式配置引導
- GitHub 範本套用
- 產生配置檔案

#### 2. [api-development](./skills/api-development/)
**API 開發流程引導**
- API First vs Code First 選擇
- OpenAPI 規格管理
- Controller 骨架產生
- Client SDK 產生

#### 3. [handler](./skills/handler/)
**Handler 業務邏輯實作**
- 業務邏輯處理
- Result Pattern 整合
- 交易管理
- 錯誤處理

#### 4. [repository-design](./skills/repository-design/)
**Repository 設計指導**
- 設計策略分析
- 資料表導向 vs 需求導向
- 決策檢查清單

#### 5. [error-handling](./skills/error-handling/)
**錯誤處理與 Result Pattern**
- Result Pattern 應用
- Failure 物件建立
- 分層錯誤處理

#### 6. [bdd-testing](./skills/bdd-testing/)
**BDD 測試實作**
- Gherkin 語法撰寫
- 測試步驟實作
- Docker 測試環境設定

#### 7. [middleware](./skills/middleware/)
**中介軟體實作**
- TraceContext 管理
- Exception Handling
- Request Logging

#### 8. [ef-core](./skills/ef-core/)
**EF Core 操作與最佳化**
- DbContextFactory 使用
- 查詢最佳化
- Migration 管理

### 🤖 Agents (4 個)

#### 1. [feature-development-agent](./agents/feature-development-agent/)
**完整功能開發流程**
- 串接 API 開發 → Handler → Repository → 測試
- 互動式引導
- 遵循 CLAUDE.md 規範

#### 2. [project-setup-agent](./agents/project-setup-agent/)
**專案初始化**
- 使用 project-init
- 完整配置流程

#### 3. [testing-strategy-agent](./agents/testing-strategy-agent/)
**測試策略規劃**
- 使用 bdd-testing
- 測試環境設定

#### 4. [architecture-review-agent](./agents/architecture-review-agent/)
**架構檢視與建議**
- 分析現有架構
- 提供改善建議

## 🚀 快速開始

### 使用 Skill

在 GitHub Copilot 中：
```
@workspace 使用 api-development 開發新的 API
```

或直接呼叫：
```
使用 project-init 初始化專案
```

### 使用 Agent

```
@workspace 使用 feature-development-agent 開發會員註冊功能
```

## 📖 詳細文件

每個 Skill 和 Agent 都包含：
- **skill.md / agent.md**：完整說明與使用方式
- **references/**：詳細參考文件
- **assets/**：程式碼範本（.cs、.yml、.feature）

## 🎯 設計原則

1. **職責單一**：每個 Skill 專注於單一領域
2. **互動優先**：遵循 CLAUDE.md 的強制互動原則
3. **範本完整**：提供可直接使用的程式碼範本
4. **文件清晰**：詳細的使用說明與範例

## 📂 檔案結構

```
.github/dotnet-contribution-claude-1/
├── skills/                      # Skills 目錄
│   ├── project-init/
│   │   ├── skill.md            # Skill 定義
│   │   └── references/         # 參考文件
│   ├── api-development/
│   │   ├── skill.md
│   │   ├── references/
│   │   └── assets/             # 程式碼範本
│   └── ...
├── agents/                      # Agents 目錄
│   ├── feature-development-agent/
│   │   └── agent.md
│   └── ...
├── README.md                    # 本檔案
└── GitHub-Copilot-Skills-Agents實作計畫.md
```

## 🔗 相關資源

- [CLAUDE.md](../../../CLAUDE.md) - 專案開發規範
- [實作計畫](./GitHub-Copilot-Skills-Agents實作計畫.md) - 完整實作計畫
- [進度追蹤](./GitHub-Copilot-Skills-Agents實作計畫.Progress.md) - 實作進度

## 📝 版本資訊

- **版本**：1.0.0
- **建立日期**：2026-01-03
- **最後更新**：2026-01-03

## 🎉 成功指標

- ✅ 8 個核心 Skills 全部完成
- ✅ 4 個 Agents 全部完成
- ✅ 所有 Skill 都有範本與參考文件
- ✅ 符合 CLAUDE.md 規範
- ✅ 互動式問答完整

---

**注意**：本專案的 Skills 和 Agents 設計嚴格遵循 CLAUDE.md 的核心互動原則，確保所有需要使用者決策的情境都會明確詢問，不擅自假設。
