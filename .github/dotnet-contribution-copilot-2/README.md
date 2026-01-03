# GitHub Copilot Skills & Agents 使用指南

## 📚 目錄結構

```
.github/dotnet-contribution-copilot-2/
├── README.md                          # 本檔案
├── EXECUTION_PLAN.md                  # 執行計劃書
├── skills/                            # Skills 定義
│   ├── project-init.md                # 專案初始化
│   ├── bdd-test.md                    # BDD 測試開發
│   ├── api-dev.md                     # API 開發
│   ├── database-ops.md                # 資料庫操作
│   ├── code-review.md                 # 程式碼審查
│   └── architecture.md                # 架構設計
├── agents/                            # Agents 定義
│   ├── project-setup-agent.md         # 專案設定代理
│   ├── feature-dev-agent.md           # 功能開發代理
│   └── quality-assurance-agent.md     # 品質保證代理
├── assets/                            # 程式碼範本
│   ├── controller-template.cs
│   ├── handler-template.cs
│   ├── repository-template.cs
│   ├── bdd-feature-template.feature
│   └── bdd-steps-template.cs
└── references/                        # 參考文件
    ├── architecture-guide.md
    ├── bdd-workflow.md
    ├── ef-core-best-practices.md
    ├── trace-context-guide.md
    └── testing-strategy.md
```

## 🤖 Agents 快速參考

### 1. project-setup-agent (專案設定代理)
**用途**: 初始化新專案或重新配置

```bash
@project-setup-agent 請幫我初始化專案
```

**流程**:
1. 檢測專案狀態
2. 詢問是否使用 GitHub 範本
3. 配置資料庫、快取、專案結構
4. 產生 `env/.template-config.json`

**適合**:
- 首次使用此專案範本
- 建立新的 .NET Core WebAPI 專案

---

### 2. feature-dev-agent (功能開發代理)
**用途**: 完整的功能開發流程

```bash
@feature-dev-agent 實作會員註冊功能
```

**流程**:
1. 需求確認
2. 選擇 API First 或 Code First
3. 產生 Controller/Handler/Repository
4. 建立 BDD 測試
5. 程式碼審查
6. 執行測試驗證

**適合**:
- 實作新的業務功能
- 建立新的 API 端點

---

### 3. quality-assurance-agent (品質保證代理)
**用途**: 程式碼品質檢查與測試分析

```bash
@quality-assurance-agent 檢查 Member 模組的程式碼品質
```

**流程**:
1. 選擇檢查範圍
2. 程式碼審查
3. 測試覆蓋率分析
4. 效能與安全性檢查
5. 產生品質報告
6. 提供改善建議

**適合**:
- Pull Request 提交前
- 定期品質檢查
- 重構前評估

---

## 🔧 Skills 快速參考

### 1. project-init
**職責**: 專案初始化

```bash
@project-init 檢測專案狀態並初始化
```

**功能**:
- 檢測專案狀態
- 詢問配置選項
- 產生配置檔案

---

### 2. bdd-test
**職責**: BDD 測試開發

```bash
@bdd-test 為會員註冊功能建立 BDD 測試
```

**功能**:
- 產生 .feature 檔案
- 實作測試步驟
- 設定 Docker 測試環境

---

### 3. api-dev
**職責**: API 開發

```bash
@api-dev 建立會員管理的 Controller, Handler, Repository
```

**功能**:
- 產生 Controller 程式碼
- 產生 Handler 程式碼
- 產生 Repository 程式碼
- 支援 API First 與 Code First

---

### 4. database-ops
**職責**: 資料庫操作

```bash
@database-ops 建立新的 Migration: AddMemberTable
```

**功能**:
- 建立 Migration
- 套用 Migration
- 反向工程 (Database First)
- 產生 SQL 腳本

---

### 5. code-review
**職責**: 程式碼審查

```bash
@code-review 檢查 Member 模組的程式碼品質
```

**功能**:
- 檢查分層架構
- 檢查命名規範
- 檢查錯誤處理
- 檢查測試覆蓋率
- 檢查效能與安全性

---

### 6. architecture
**職責**: 架構設計建議

```bash
@architecture 我需要設計訂單管理的 Repository，請給我建議
```

**功能**:
- Repository Pattern 設計建議
- 中介軟體設計建議
- 專案結構組織建議
- TraceContext 整合建議

---

## 💡 使用情境範例

### 情境 1: 從零開始建立專案

```bash
# Step 1: 初始化專案
@project-setup-agent 請幫我初始化專案

# Step 2: 開發第一個功能
@feature-dev-agent 實作會員註冊功能

# Step 3: 檢查程式碼品質
@quality-assurance-agent 檢查程式碼品質
```

---

### 情境 2: 在現有專案新增功能

```bash
# Step 1: 開發新功能
@feature-dev-agent 實作訂單管理功能

# Step 2: 建立資料庫 Migration
@database-ops 建立 Migration: CreateOrderTables

# Step 3: 品質檢查
@quality-assurance-agent 檢查 Order 模組
```

---

### 情境 3: 單獨使用 Skill

```bash
# 僅建立 API 程式碼（不含測試）
@api-dev 建立產品管理的 Controller, Handler, Repository

# 僅建立 BDD 測試
@bdd-test 為產品管理建立 BDD 測試

# 僅執行程式碼審查
@code-review 檢查 Product 模組
```

---

### 情境 4: 架構設計諮詢

```bash
# 詢問 Repository 設計
@architecture 訂單管理涉及訂單、訂單明細、付款等，應該如何設計 Repository？

# 詢問中介軟體設計
@architecture 我需要加入請求追蹤功能，應該如何設計中介軟體？

# 詢問專案結構
@architecture 5 人團隊，應該用單一專案還是多專案結構？
```

---

## 🎯 工作流程建議

### 新專案開發流程

```mermaid
graph TD
    A[開始] --> B[@project-setup-agent<br/>初始化專案]
    B --> C[@feature-dev-agent<br/>開發第一個功能]
    C --> D[@quality-assurance-agent<br/>品質檢查]
    D --> E{通過?}
    E -->|是| F[部署測試環境]
    E -->|否| G[修正問題]
    G --> D
    F --> H[@feature-dev-agent<br/>開發下一個功能]
    H --> I[@quality-assurance-agent<br/>品質檢查]
    I --> J{通過?}
    J -->|是| K[合併程式碼]
    J -->|否| L[修正問題]
    L --> I
    K --> M[重複開發流程]
```

### 功能開發流程

```
1. 需求分析
   ↓
2. @feature-dev-agent 開始功能開發
   ├─ 選擇 API First / Code First
   ├─ 產生 Controller/Handler/Repository
   ├─ 建立 BDD 測試
   ├─ 建立 Migration (如需要)
   └─ 執行測試驗證
   ↓
3. @quality-assurance-agent 品質檢查
   ├─ 程式碼審查
   ├─ 測試覆蓋率分析
   └─ 效能與安全性檢查
   ↓
4. 修正問題（如有）
   ↓
5. 提交 Pull Request
```

---

## 📋 開發檢查清單

### 功能開發前
- [ ] 需求明確定義
- [ ] 選擇 API First 或 Code First
- [ ] 確認需要的分層 (Controller/Handler/Repository)
- [ ] 確認是否需要 Migration

### 功能開發中
- [ ] 遵循分層架構原則
- [ ] 使用 Result Pattern 處理錯誤
- [ ] 整合 TraceContext
- [ ] 加入結構化日誌
- [ ] 撰寫 BDD 測試

### 功能開發後
- [ ] 程式碼審查通過
- [ ] 測試覆蓋率 > 80%
- [ ] 所有 API 端點有 BDD 測試
- [ ] 無嚴重安全性問題
- [ ] 無明顯效能問題

---

## 🚀 快速開始

### 1. 首次使用

```bash
# 初始化專案
@project-setup-agent 請幫我初始化專案

# 閱讀專案文件
cat CLAUDE.md

# 查看可用的開發指令
cat Taskfile.yml
```

### 2. 開發第一個功能

```bash
# 使用功能開發代理
@feature-dev-agent 實作會員註冊功能

# 或分步驟使用 Skills
@api-dev 建立會員管理 API
@bdd-test 建立會員註冊測試
@database-ops 建立 Member 資料表 Migration
```

### 3. 檢查程式碼品質

```bash
# 完整品質檢查
@quality-assurance-agent 檢查程式碼品質

# 或單獨審查
@code-review 檢查 Member 模組
```

---

## 📚 延伸閱讀

- **執行計劃書**: [EXECUTION_PLAN.md](EXECUTION_PLAN.md)
- **架構設計指南**: [references/architecture-guide.md](references/architecture-guide.md)
- **BDD 工作流程**: [references/bdd-workflow.md](references/bdd-workflow.md)
- **測試策略**: [references/testing-strategy.md](references/testing-strategy.md)
- **EF Core 最佳實踐**: [references/ef-core-best-practices.md](references/ef-core-best-practices.md)
- **TraceContext 指南**: [references/trace-context-guide.md](references/trace-context-guide.md)

---

## ❓ 常見問題

### Q1: Agent 和 Skill 有什麼區別？
**A**: 
- **Agent**: 完整的工作流程，會與用戶進行多輪互動，串接多個 Skills
- **Skill**: 單一職責的功能，可獨立使用或被 Agent 呼叫

### Q2: 何時使用 Agent？何時使用 Skill？
**A**:
- 使用 **Agent**: 需要完整流程引導（如：初始化專案、開發新功能）
- 使用 **Skill**: 僅需要單一功能（如：只建立 BDD 測試、只審查程式碼）

### Q3: 可以自訂 Skills 或 Agents 嗎？
**A**: 可以！參考現有的 Skill/Agent 定義檔案，建立自己的 .md 檔案即可。

### Q4: 如何貢獻或回報問題？
**A**: 請在 GitHub 專案中提出 Issue 或 Pull Request。

---

## 📞 支援

如有問題或建議，請：
1. 查看專案文件：[CLAUDE.md](../../CLAUDE.md)
2. 查看執行計劃：[EXECUTION_PLAN.md](EXECUTION_PLAN.md)
3. 提出 Issue（如適用）

---

**版本**: 1.0  
**最後更新**: 2025-01-03  
**維護者**: GitHub Copilot CLI Team
