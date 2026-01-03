### 執行計畫書.Progress.md
- [x] **建立專案目錄**
  - **任務**: 建立 `.github/dotnet-contribution-gemini-1` 根目錄以及 `agents`, `skills`, `assets`, `references` 子目錄。
  - **理由**: 這是所有產出檔案的存放位置，預先建立好結構有助於後續工作的組織與管理。

#### 步驟 2: 拆解核心原則與指南至 `references`

- [x] **建立 AI 互動規則 (`interaction-rules.md`)**
  - **任務**: 擷取 `CLAUDE.md` 中關於 AI 助理的核心互動原則、強制詢問情境等內容。
  - **理由**: 這是所有 agent 都必須遵守的最高指導原則，將其獨立出來，方便所有 agent 統一引用。

- [x] **建立架構指南 (`architecture-guide.md`)**
  - **任務**: 擷取分層架構、技術堆疊、方案 A/B 比較等內容。
  - **理由**: 將架構設計的知識模組化，供 `architecture` 或 `api-dev` 等 skill 在需要時提供給使用者參考。

- [x] **建立 BDD 工作流程指南 (`bdd-workflow.md`)**
  - **任務**: 擷取 BDD 開發循環、Docker 優先測試策略、API 控制器測試指引等內容。
  - **理由**: 將 BDD 的完整實踐方法標準化，供 `bdd-test` skill 引用。

- [x] **建立 Repository 設計指南 (`repository-pattern-guide.md`)**
  - **任務**: 擷取 Repository 設計哲學、策略選擇、設計決策檢查清單等內容。
  - **理由**: 提供一個獨立的資料存取層設計指南，讓 `api-dev` skill 在實作 Repository 時能引導使用者做出正確決策。

- [x] **建立其他核心實踐指南**
  - **任務**: 擷取 `CLAUDE.md` 的內容，建立 `error-handling-guide.md`（錯誤處理）、`trace-context-guide.md`（追蹤內容管理）等參考文件。
  - **理由**: 將各個重要的最佳實踐拆分為獨立文件，提高 skill 的重用性與靈活性。

- [x] **建立 TraceContext 指南 (`trace-context-guide.md`)**
  - **任務**: 擷取 `CLAUDE.md` 中關於追蹤內容管理 (TraceContext) 的內容。
  - **理由**: 將追蹤內容管理的規範獨立出來，供中介軟體或核心功能開發時參考。

#### 步驟 3: 建立程式碼模板至 `assets`

- [x] **建立 API 相關模板**
  - **任務**: 建立 `controller-template.cs`, `handler-template.cs`, `repository-demand-oriented-template.cs`。
  - **理由**: 為 `api-dev` skill 提供可直接使用的程式碼骨架，加速標準化程式碼的產生。

- [x] **建立 BDD 測試相關模板**
  - **任務**: 建立 `bdd-feature-template.feature`, `bdd-steps-template.cs`。
  - **理由**: 為 `bdd-test` skill 提供 Gherkin 語法和測試步驟的標準範本。

#### 步驟 4: 建立 `skills`

- [x] **建立專案初始化 skill (`project-init.md`)**
  - **職責**: 處理專案初始化的狀態檢測與互動流程。參考 `references/interaction-rules.md`。

- [x] **建立 API 開發 skill (`api-dev.md`)**
  - **職責**: 引導使用者實作 Controller, Handler, Repository。參考 `assets` 中的模板與 `references` 的設計指南。

- [x] **建立 BDD 測試 skill (`bdd-test.md`)**
  - **職責**: 引導使用者撰寫與實作 BDD 測試。參考 `assets` 的 BDD 模板與 `references/bdd-workflow.md`。

- [x] **建立架構決策 skill (`architecture.md`)**
  - **職責**: 專門用於協助使用者在開發初期或重構時，進行重要的架構決策，如專案結構、Repository 模式選擇。
  - **引用**: `references/architecture-guide.md`, `references/repository-pattern-guide.md`。

#### 步驟 5: 建立 `agents`

- [x] **建立專案設定 agent (`project-setup-agent.md`)**
  - **職責**: 作為專案初始化的入口，引導使用者完成全新專案的設定。
  - **工作流程**: 參考 `references/interaction-rules.md`，並主要使用 `project-init.md` skill。

- [x] **建立功能開發 agent (`feature-dev-agent.md`)**
  - **職責**: 引導使用者完成一個完整功能的開發，從設計、實作到測試。
  - **工作流程**: 串接 `architecture.md`, `api-dev.md`, `bdd-test.md` skills。

所有 agents 都已建立。這表示計畫實作完成。
