# 檔案參考完整性檢查報告

本文件記錄 `.github\dotnet-contribution` 目錄下所有檔案的參考關係與完整性。

## ✅ 檔案結構確認

### 已建立的檔案

```
.github\dotnet-contribution\
├── README.md
├── agents\
│   └── dotnet-api.md
└── skills\
    └── api-template-bdd-guide\
        ├── SKILL.md
        ├── assets\
        │   ├── bdd-feature-template.feature  ✅
        │   ├── bdd-steps-template.cs         ✅
        │   ├── handler-template.cs           ✅
        │   └── repository-template.cs        ✅
        └── references\
            ├── cache-strategy.md             ✅
            ├── openapi-codegen.md            ✅
            └── trace-context-design.md       ✅
```

## ✅ SKILL.md 資源檔案參考

`SKILL.md` 在「資源檔案 (Resources)」章節中參考以下檔案，**全部已存在**：

- ✅ `assets/handler-template.cs` - Handler 實作範本（Result Pattern + 快取）
- ✅ `assets/repository-template.cs` - Repository 實作範本（EF Core）
- ✅ `assets/bdd-feature-template.feature` - BDD 情境範本（Gherkin）
- ✅ `assets/bdd-steps-template.cs` - BDD 測試步驟範本（Reqnroll）
- ✅ `references/trace-context-design.md` - TraceContext 設計說明
- ✅ `references/cache-strategy.md` - 多層快取策略詳解
- ✅ `references/openapi-codegen.md` - OpenAPI 程式碼產生工作流程

## ✅ README.md 外部參考

`README.md` 中參考以下外部檔案：

- ✅ `../../CLAUDE.md` - 專案根目錄的 CLAUDE.md（存在）
- ✅ `https://github.com/yaochangyu/api.template` - GitHub 專案範本

## ✅ 專案根目錄參考檔案

文件中提到的專案根目錄檔案，**全部已存在**：

- ✅ `CLAUDE.md` - 專案開發指南
- ✅ `Taskfile.yml` - 開發指令定義
- ✅ `doc/openapi.yml` - OpenAPI 規格檔案
- ✅ `src/be/` - 後端原始碼目錄

## ✅ Agent 定義參考

`dotnet-api.md` 中參考：

- ✅ CLAUDE.md 中的「AI 助理使用規則」
- ✅ 專案檔案結構 (src/be/{Project}.WebAPI/ 等)

## 📝 路徑規範

### 相對路徑規則

從 `.github\dotnet-contribution\README.md` 出發：
- 到專案根目錄：`../../`
- 到 CLAUDE.md：`../../CLAUDE.md`

從 `.github\dotnet-contribution\skills\api-template-bdd-guide\SKILL.md` 出發：
- 到同目錄 assets：`assets/`
- 到同目錄 references：`references/`

### 範例檔案路徑

文件中使用的範例路徑（僅作說明，不需實際存在）：
- `src/be/{Project}.WebAPI/` - 專案範本變數
- `src/be/JobBank1111.Job.WebAPI/` - 具體專案範例

## ✅ 完整性總結

| 檢查項目 | 狀態 |
|---------|------|
| SKILL.md 資源檔案 | ✅ 7/7 檔案存在 |
| README.md 外部參考 | ✅ 全部有效 |
| 專案根目錄參考 | ✅ 全部存在 |
| Agent 定義參考 | ✅ 全部有效 |

**結論**：所有檔案參考完整性檢查通過 ✅

---

*最後更新時間：2026-01-01*
