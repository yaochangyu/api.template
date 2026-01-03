# GitHub Copilot Skills & Agents 實作計畫

## 專案目標
根據 CLAUDE.md 檔案內容，建立一套完整的 GitHub Copilot Skills 和 Agents，協助開發者遵循專案規範進行 ASP.NET Core Web API 開發。

## 檔案組織結構
```
.github/dotnet-contribution-claude-2/
├── skills/                    # Skills 定義檔案
│   ├── project-init/         # 專案初始化 Skill
│   ├── feature-dev/          # 功能開發 Skill
│   ├── bdd-test/             # BDD 測試 Skill
│   ├── repository-design/    # Repository 設計 Skill
│   ├── error-handling/       # 錯誤處理 Skill
│   ├── middleware-setup/     # 中介軟體設定 Skill
│   └── performance-opt/      # 效能最佳化 Skill
├── agents/                   # Agents 定義檔案
│   ├── aspnet-dev-agent/     # ASP.NET Core 開發助理
│   └── test-driven-agent/    # 測試驅動開發助理
├── references/               # 參考文件（被 skills 引用）
│   ├── architecture.md       # 架構設計參考
│   ├── repository-pattern.md # Repository Pattern 參考
│   ├── error-handling.md     # 錯誤處理參考
│   ├── middleware.md         # 中介軟體參考
│   ├── bdd-workflow.md       # BDD 流程參考
│   ├── ef-core-best-practices.md  # EF Core 最佳實踐
│   └── performance-optimization.md # 效能最佳化參考
└── assets/                   # 程式碼範本
    ├── controller-template.cs
    ├── handler-template.cs
    ├── repository-template.cs
    ├── middleware-template.cs
    ├── failure-template.cs
    └── feature-file-template.feature
```

## 實作步驟

### 階段一：參考文件建立
**目的**：建立可被 Skills 重複引用的參考文件，降低重複性並保持單一職責

- [ ] 1.1 建立 `references/architecture.md` - 架構設計參考文件
  - **原因**：集中說明分層架構（Controller → Handler → Repository）、專案組織方式
  - **內容來源**：CLAUDE.md 的「架構概述」章節

- [ ] 1.2 建立 `references/repository-pattern.md` - Repository Pattern 設計參考
  - **原因**：詳細說明資料表導向 vs 需求導向的設計策略與決策清單
  - **內容來源**：CLAUDE.md 的「Repository Pattern 設計哲學」章節

- [ ] 1.3 建立 `references/error-handling.md` - 錯誤處理參考文件
  - **原因**：說明 Result Pattern、Failure 物件結構、分層錯誤處理策略
  - **內容來源**：CLAUDE.md 的「錯誤處理與回應管理」章節

- [ ] 1.4 建立 `references/middleware.md` - 中介軟體參考文件
  - **原因**：說明中介軟體管線順序、職責分離原則
  - **內容來源**：CLAUDE.md 的「中介軟體架構與實作」章節

- [ ] 1.5 建立 `references/bdd-workflow.md` - BDD 流程參考文件
  - **原因**：說明 BDD 開發循環、Docker 測試策略、測試原則
  - **內容來源**：CLAUDE.md 的「BDD 開發流程」章節

- [ ] 1.6 建立 `references/ef-core-best-practices.md` - EF Core 最佳實踐
  - **原因**：說明 DbContextFactory、查詢最佳化、非同步程式設計
  - **內容來源**：CLAUDE.md 的「專案最佳實踐」章節

- [ ] 1.7 建立 `references/performance-optimization.md` - 效能最佳化參考
  - **原因**：說明快取策略、記憶體管理、查詢最佳化
  - **內容來源**：CLAUDE.md 的「效能最佳化與快取策略」章節

### 階段二：程式碼範本建立
**目的**：提供標準化的程式碼範本，確保實作符合專案規範

- [ ] 2.1 建立 `assets/controller-template.cs` - Controller 範本
  - **原因**：提供符合專案規範的 Controller 實作範本（主建構函式注入、Result Pattern 轉換）

- [ ] 2.2 建立 `assets/handler-template.cs` - Handler 範本
  - **原因**：提供業務邏輯處理範本（Result Pattern、CancellationToken 支援）

- [ ] 2.3 建立 `assets/repository-template.cs` - Repository 範本
  - **原因**：提供資料存取層範本（DbContextFactory、非同步操作、錯誤處理）

- [ ] 2.4 建立 `assets/middleware-template.cs` - Middleware 範本
  - **原因**：提供中介軟體實作範本（職責單一、結構化日誌）

- [ ] 2.5 建立 `assets/failure-template.cs` - Failure 物件範本
  - **原因**：提供標準化的錯誤回應物件範本

- [ ] 2.6 建立 `assets/feature-file-template.feature` - BDD Feature 檔案範本
  - **原因**：提供 Gherkin 語法範本（Scenario、Given-When-Then）

### 階段三：Skills 建立（單一職責）
**目的**：建立專注於單一職責的 Skills，透過參考文件實現知識重用

- [ ] 3.1 建立 `skills/project-init/skill.md` - 專案初始化 Skill
  - **職責**：專案狀態檢測、GitHub 範本套用、配置檔案管理
  - **參考文件**：`references/architecture.md`
  - **互動流程**：詢問資料庫類型、Redis 需求、專案結構組織

- [ ] 3.2 建立 `skills/feature-dev/skill.md` - 功能開發 Skill
  - **職責**：引導使用者選擇 API First 或 Code First 流程
  - **參考文件**：`references/architecture.md`
  - **互動流程**：詢問 API 開發流程、OpenAPI 狀態、實作分層

- [ ] 3.3 建立 `skills/bdd-test/skill.md` - BDD 測試 Skill
  - **職責**：協助撰寫 BDD 測試情境、測試步驟實作
  - **參考文件**：`references/bdd-workflow.md`
  - **使用範本**：`assets/feature-file-template.feature`
  - **互動流程**：詢問測試需求、測試範圍、測試環境

- [ ] 3.4 建立 `skills/repository-design/skill.md` - Repository 設計 Skill
  - **職責**：根據業務需求設計最適合的 Repository 策略
  - **參考文件**：`references/repository-pattern.md`, `references/ef-core-best-practices.md`
  - **使用範本**：`assets/repository-template.cs`
  - **互動流程**：評估複雜度、選擇設計策略

- [ ] 3.5 建立 `skills/error-handling/skill.md` - 錯誤處理 Skill
  - **職責**：實作 Result Pattern、Failure 物件、錯誤映射
  - **參考文件**：`references/error-handling.md`
  - **使用範本**：`assets/failure-template.cs`, `assets/handler-template.cs`

- [ ] 3.6 建立 `skills/middleware-setup/skill.md` - 中介軟體設定 Skill
  - **職責**：設定中介軟體管線、職責分離
  - **參考文件**：`references/middleware.md`
  - **使用範本**：`assets/middleware-template.cs`

- [ ] 3.7 建立 `skills/performance-opt/skill.md` - 效能最佳化 Skill
  - **職責**：快取策略、查詢最佳化、記憶體管理
  - **參考文件**：`references/performance-optimization.md`, `references/ef-core-best-practices.md`
  - **互動流程**：詢問優化面向

### 階段四：Agents 建立（工作流程串接）
**目的**：串接 Skills 形成完整的開發工作流程，提供互動式開發體驗

- [ ] 4.1 建立 `agents/aspnet-dev-agent/agent.md` - ASP.NET Core 開發助理
  - **職責**：主要開發助理，根據使用者需求串接適當的 Skills
  - **工作流程**：
    1. 偵測專案狀態 → 呼叫 `project-init` skill（如需要）
    2. 詢問開發需求 → 呼叫 `feature-dev` skill
    3. 詢問測試策略 → 呼叫 `bdd-test` skill（如需要）
    4. 設計 Repository → 呼叫 `repository-design` skill
    5. 實作錯誤處理 → 呼叫 `error-handling` skill
    6. 設定中介軟體 → 呼叫 `middleware-setup` skill（如需要）
  - **互動特點**：
    - 遵循強制互動原則（不擅自假設）
    - 分階段詢問（每次最多 3-4 個問題）
    - 提供清晰的選項說明

- [ ] 4.2 建立 `agents/test-driven-agent/agent.md` - 測試驅動開發助理
  - **職責**：專注於 BDD 測試驅動開發流程
  - **工作流程**：
    1. 詢問測試需求 → 呼叫 `bdd-test` skill
    2. 撰寫 Feature 檔案
    3. 引導實作測試步驟
    4. 引導實作功能（先測試後實作）
  - **互動特點**：
    - 強制先寫測試後實作
    - 確保使用 Docker 測試環境
    - 禁止直接測試 Controller

### 階段五：測試與文件完善
**目的**：確保 Skills 和 Agents 可正常運作，並提供使用說明

- [ ] 5.1 建立 `README.md` - 總覽說明文件
  - **內容**：Skills 和 Agents 的使用說明、快速開始指南

- [ ] 5.2 建立使用範例文件
  - **內容**：實際使用場景示範

## 預期成果

1. **7 個 Skills**：各自專注於單一職責，可重複使用
2. **2 個 Agents**：串接 Skills，提供完整的開發工作流程
3. **7 個參考文件**：可被多個 Skills 引用，避免重複
4. **6 個程式碼範本**：確保實作符合專案規範
5. **完整文件**：使用說明、範例

## 注意事項

1. **強制互動原則**：所有 Skills 和 Agents 都必須遵循 CLAUDE.md 的互動原則
2. **單一職責**：每個 Skill 只專注於一個特定任務
3. **知識重用**：透過參考文件避免重複內容
4. **範本驅動**：提供標準化的程式碼範本
5. **工作流程**：Agents 負責串接 Skills，形成完整流程
