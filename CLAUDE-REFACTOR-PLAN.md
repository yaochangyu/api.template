# CLAUDE.md 系統重構計畫

**計畫時間**: 2026-07-10 22:40 GMT+8  
**目標**: 分離開發規則、決策邏輯、AI 助理規則到獨立檔案，實現延遲載入  
**進度**: [計畫中]

---

## 📋 核心目標

將 CLAUDE.md（255 行）拆分成三個檔案：
- **DEVELOPMENT-RULES.md** — 開發規則（所有人需要）
- **DECISION-FRAMEWORK.md** — 決策邏輯（特定工作流需要）
- **CLAUDE.md** — AI 助理規則（我的行為規則）

---

## 📊 執行步驟

### Step 1: 提取開發規則到 DEVELOPMENT-RULES.md
**狀態**: ⏳

**內容來源**（從 CLAUDE.md 提取）：
- [ ] 強制詢問情境（1-5）— 第 111-161 行
- [ ] 禁止的行為（6 項）— 第 164-171 行
- [ ] 開發指令 — 第 50-66 行
- [ ] 架構概述 — 第 70-107 行
- [ ] 最佳實踐檢查清單 — 第 220-249 行

**檔案結構**：
```
.claude/DEVELOPMENT-RULES.md
├── 開發指令（Taskfile）
├── 架構概述
├── 強制詢問情境
├── 禁止的行為
└── 最佳實踐檢查清單
```

**行數目標**: 250-300 行

---

### Step 2: 提取決策邏輯到 DECISION-FRAMEWORK.md
**狀態**: ⏳

**內容來源**（從 CLAUDE.md 提取）：
- [ ] 強制詢問情境中的決策部分（3.3a, 3.3b, 3.3c, 4.4a-4.4e）
- [ ] 資料庫選擇決策
- [ ] API 開發流程決策
- [ ] 測試策略決策
- [ ] 快取策略決策

**檔案結構**：
```
.claude/DECISION-FRAMEWORK.md
├── API 開發流程（API First vs Code First）
├── 資料庫選擇決策
├── 測試策略決策樹
├── 快取策略決策
└── 效能優化決策
```

**行數目標**: 150-200 行

---

### Step 3: 重構 CLAUDE.md
**狀態**: ⏳

**保留內容**：
- [ ] AI 助理使用規則（第 19-38 行）
- [ ] 專案狀態檢測機制（第 39-46 行）
- [ ] 計畫書生命週期管理（第 194-217 行）
- [ ] 進階主題索引（第 175-191 行，更新為指向新檔案）

**刪除內容**：
- ❌ 開發指令（移到 DEVELOPMENT-RULES.md）
- ❌ 架構概述（移到 DEVELOPMENT-RULES.md）
- ❌ 強制詢問情境（移到 DEVELOPMENT-RULES.md + DECISION-FRAMEWORK.md）
- ❌ 禁止的行為（移到 DEVELOPMENT-RULES.md）
- ❌ 最佳實踐檢查清單（移到 DEVELOPMENT-RULES.md）

**新增**：
- ✅ 指向 DEVELOPMENT-RULES.md 的連結
- ✅ 指向 DECISION-FRAMEWORK.md 的連結

**行數目標**: 120-150 行（精簡 40-50%）

---

### Step 4: 更新 .claude/README.md
**狀態**: ⏳

**新增內容**：
```markdown
## 📚 三層文檔系統

1. **CLAUDE.md** — AI 助理行為規則
   - 適合：Claude Code 使用者
   
2. **DEVELOPMENT-RULES.md** — 開發規則
   - 適合：所有開發者 + SKILL 使用者
   - 強制詢問、禁止行為、最佳實踐
   
3. **DECISION-FRAMEWORK.md** — 決策邏輯
   - 適合：特定工作流（API 開發、測試規劃、DB 選擇）
```

---

### Step 5: 更新 SKILL.md 參考
**狀態**: ⏳

**需要更新的 SKILL**（16 個）：
- [ ] `/api-development` — 需要決策邏輯
- [ ] `/bdd-testing` — 只需開發規則
- [ ] `/bdd-practices` — （已刪除，跳過）
- [ ] `/caching-strategy` — 需要決策邏輯
- [ ] `/ef-core` — 只需開發規則
- [ ] `/error-handling` — 只需開發規則
- [ ] `/handler` — 只需開發規則
- [ ] `/middleware` — 只需開發規則
- [ ] `/observability` — 只需開發規則
- [ ] `/project-init` — 需要決策邏輯
- [ ] `/repository-design` — 需要決策邏輯
- [ ] `/security-*` 系列（6 個）— 只需開發規則

**新增到 SKILL.md 頂部**：
```markdown
### ⚠️ 前置條件
本 SKILL 須搭配閱讀：
- [開發規則](../../DEVELOPMENT-RULES.md)
- [決策框架](../../DECISION-FRAMEWORK.md#...)（若需要）
```

---

### Step 6: Git Commit + 推送
**狀態**: ⏳

- [ ] 建立新檔案（DEVELOPMENT-RULES.md、DECISION-FRAMEWORK.md）
- [ ] 重構 CLAUDE.md
- [ ] 更新 .claude/README.md
- [ ] 更新 16 個 SKILL.md
- [ ] Git commit（不含 Co-authored-by）
- [ ] git push origin main

---

## 📈 完成條件

- [ ] DEVELOPMENT-RULES.md 已建立（250-300 行）
- [ ] DECISION-FRAMEWORK.md 已建立（150-200 行）
- [ ] CLAUDE.md 精簡到 120-150 行
- [ ] 三個檔案間的超連結完整
- [ ] 所有 SKILL.md 都指向相應規則
- [ ] 無內容遺失或重複
- [ ] Git 已 commit 並推送

---

## 🎯 預估工時

- Step 1-2: 1 小時（提取規則和決策）
- Step 3: 30 分鐘（重構 CLAUDE.md）
- Step 4-5: 30 分鐘（更新文檔和 SKILL）
- Step 6: 15 分鐘（提交推送）

**總計**: 2.25 小時

---

**開始時間**: 待執行  
**預期完成**: 待確認

