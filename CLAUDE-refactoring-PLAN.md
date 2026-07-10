# CLAUDE.md 重組計畫（P0 優先級）

**計畫時間**: 2026-07-10 17:58 GMT+8  
**目標**: 將 CLAUDE.md 從 1092 行精簡到 150 行，核心決策邏輯保留  
**進度**: [正在執行]

---

## 📋 核心原則（不能掉）

### ✅ 必須保留在 CLAUDE.md
- **AskUserQuestion 工具** —— 所有強制互動確認
- **專案狀態檢測機制** —— 初始化流程
- **強制詢問情境** (1-5 all 詢問場景)
  - 專案初始化與配置
  - 資料庫相關操作
  - 功能實作的 API 流程選擇
  - **測試策略詢問**（最重要）
  - 效能最佳化
- **禁止的行為** —— 所有 6 項禁止事項
- **快速指令參考** —— task 命令

### ❌ 必須移到 SKILL 的內容
- 詳細實作指南（程式碼範例、架構範例）
- BDD 流程詳解 → `/bdd-practices`
- Repository 設計詳解 → `/repository-design`
- 錯誤處理詳解 → `/error-handling`
- 中介軟體詳解 → `/middleware`
- EF Core 詳解 → `/ef-core`
- 快取詳解 → `/caching-strategy` (新建)

---

## 🎯 執行步驟

### Step 1: 精簡 CLAUDE.md ✓ 執行中

**新檔案目標結構** (150 行)：
```
1-20    | 檔案頭 + 目錄（簡化）
21-70   | AI 助理使用規則（強制互動確認、專案狀態檢測）
71-85   | 開發指令（task 命令快速參考）
86-105  | 架構概述（1 張圖 + 核心專案名稱）
106-125 | 強制詢問情境（1-5 all 保留）
126-135 | 禁止的行為（所有 6 項保留）
136-145 | 最佳實踐檢查清單（精簡版）
146-150 | 進階主題索引（連結各 SKILL）
```

**刪除清單**：
- [ ] BDD 開發流程（全部移到 `/bdd-practices`）
- [ ] 核心開發原則中的詳細實作（保留原則，刪除實作）
- [ ] 專案最佳實踐中的程式碼範例（全部移到對應 SKILL）
- [ ] 追蹤內容管理詳解（移到 `/middleware`）
- [ ] 錯誤處理詳解（移到 `/error-handling`）
- [ ] 中介軟體架構詳解（移到 `/middleware`）
- [ ] 效能最佳化詳解（移到 `/caching-strategy`）
- [ ] API 設計詳解（移到 `/api-development`）
- [ ] 監控與部署詳解（保留最小指引，詳解移到 `/observability`）
- [ ] 附錄中的詳細檔案參考（簡化為 SKILL 引用）

### Step 2: 補強現有 SKILL

#### `/error-handling` SKILL
- [x] 基本框架存在
- [ ] 補 `references/result-pattern-best-practices.md`
  - 詳細實作範例
  - 常見錯誤模式
- [ ] 補 `assets/failure-template.cs`
  - 可直接複製的範本程式碼

#### `/middleware` SKILL  
- [x] 基本框架存在
- [ ] 補 `references/middleware-architecture.md`
  - 管線順序詳解
  - 職責分離原則
- [ ] 補 `references/tracecontext-management.md`
  - 生命週期管理
  - 服務注入方式

### Step 3: 建立新 SKILL

#### `/caching-strategy` (新建)
```
caching-strategy/
├── SKILL.md
│   ├── 描述：多層快取設計（L1 Memory、L2 Redis）
│   ├── 職責：快取分層、TTL 管理、失效策略
│   ├── 能力：
│   │   ├── 1. 多層快取架構分析
│   │   ├── 2. Redis 配置指導
│   │   └── 3. 快取失效策略選擇
│   └── 決策檢查清單
├── references/
│   ├── caching-architecture.md
│   ├── redis-configuration.md
│   ├── cache-invalidation-strategies.md
│   └── performance-best-practices.md
└── assets/
    ├── cache-layer-template.cs
    └── redis-setup.yml
```

### Step 4: 更新 CLAUDE.md 末尾

**新增「進階主題索引」部分**：
```markdown
## 進階主題索引

當實作具體功能時，根據需求查閱相應 SKILL：

| 工作項 | 相關 SKILL | 說明 |
|--------|-----------|------|
| 設計 API 端點 | `/api-development` | OpenAPI First vs Code First 流程 |
| 實作業務邏輯 | `/handler` | 主建構函式、依賴注入、Result Pattern |
| 設計資料存取 | `/repository-design` | 資料表導向 vs 需求導向 vs 混合 |
| 處理錯誤 | `/error-handling` | Result Pattern、Failure 物件、分層錯誤 |
| 實作中介軟體 | `/middleware` | TraceContext、Exception Handling、Request Logging |
| EF Core 最佳化 | `/ef-core` | 查詢最佳化、Migration、DbContextFactory |
| 快取設計 | `/caching-strategy` | 多層快取、Redis 配置、失效策略 |
| 測試實作 | `/bdd-testing` | Gherkin、Reqnroll、Testcontainers、Docker |
| 監控配置 | `/observability` (P1) | 健康檢查、Seq、OpenTelemetry |
```

### Step 5: Git Commit

```bash
git add CLAUDE.md .claude/skills/*/SKILL.md
git commit -m "refactor(docs): 精簡 CLAUDE.md，重組內容到 SKILL 系統

- 將 CLAUDE.md 從 1092 行精簡至 150 行
- 保留所有決策邏輯（AskUserQuestion、強制詢問、互動確認）
- 將詳細實作指南移到相應 SKILL：
  - error-handling, middleware, repository-design 補強內容
  - 新建 caching-strategy SKILL
- 新增進階主題索引，方便快速導航
- 確保開發原則與決策原則完整保留

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## 📊 進度追蹤

| 步驟 | 狀態 | 完成度 |
|------|------|--------|
| Step 1: 精簡 CLAUDE.md | ✅ 完成 | 100% |
| Step 2: 補強現有 SKILL | ⏳ 待執行 | 0% |
| Step 3: 建立新 SKILL | ✅ 完成 (caching-strategy) | 100% |
| Step 4: 更新末尾索引 | ✅ 完成 (已在 CLAUDE.md) | 100% |
| Step 5: Git Commit | ⏳ 執行中 | 50% |

---

## 🔍 驗證檢查清單

完成後檢查：
- [ ] CLAUDE.md 行數 ≤ 150
- [ ] AskUserQuestion 仍在 CLAUDE.md
- [ ] 強制詢問情境（1-5）全部保留
- [ ] 禁止行為全部保留
- [ ] 各 SKILL 補強內容完整
- [ ] 進階主題索引可正常瀏覽
- [ ] Git commit 成功

---

**預估完成時間**: 15-20 分鐘  
**責任人**: Claude Code  
**最後更新**: 2026-07-10 17:58 GMT+8
