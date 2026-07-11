⏹️ **Status**: COMPLETED (2026-07-11 10:50 GMT+8)

此計畫已全部執行完成，已封存至 .archive/
詳見 Git commit: fb6873d

---

# API First 與 Code First 文檔同步計畫

**計畫時間**: 2026-07-11 10:43 GMT+8  
**完成時間**: 2026-07-11 10:50 GMT+8  
**進度**: [✅ 已完成]  
**優先級**: P2（確保文檔一致性）  
**關聯**: api-development-refactor.plan.md（已完成）、.archive/api-development-refactor.plan.completed.md

---

## 背景

在 `/api-development` SKILL 調整後，我們有兩個生產 Controller 範例：
- **MemberV1ControllerImpl.cs** — API First 方式（實作介面 IMemberV1Controller）
- **MemberController.cs** — Code First 方式（直接繼承 ControllerBase）

目前問題：並非所有文檔都清楚區分這兩個檔案的差異和使用時機，導致新人可能被引導到錯誤的參考範例。

---

## 現況分析

### ✅ 已正確區分的檔案
- `.claude/skills/api-development/SKILL.md` — 明確區分 V1 (API First) 和 V2 (Code First)
- `.claude/skills/api-development/references/api-development-workflow.md` — 提供兩種方式的範例

### ❌ 需要更新的檔案

#### 1. `.claude/README.md`（第 184-186 行）
**現況**：
```markdown
# 取得 Controller 實作
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/Member/MemberV1ControllerImpl.cs
```

**問題**：只引用 API First 的 V1，不適合 Code First 使用者

**調整內容**：
```markdown
# 取得 Controller 實作

## API First（規格優先）
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/Member/MemberV1ControllerImpl.cs

## Code First（實作優先）
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/Member/MemberController.cs

👉 **如何選擇?** 參考 `.claude/skills/api-development/SKILL.md` 的「問題 2.5」
```

---

#### 2. ~~`.claude/command-processor.md`~~ 已刪除

**已知事實**：舊命令系統（`.claude/command-processor.md`、`.claude/commands/`）已於前置階段完全移除。

**調整方案**：改為在 SKILL 系統內補充 API First / Code First 的雙軌指導

**調整對象**：`.claude/skills/api-development/SKILL.md`（已在 api-development-refactor 計畫中完成）

**驗證狀態**：✅ `/api-development` SKILL 已清楚區分 API First 和 Code First，包含：
- 問題 2.5 — 開發方式確認
- 子章節 — API First vs Code First 對比與範例
- FileResolver 參考 — 指向 MemberV1ControllerImpl.cs 和 MemberController.cs

---

### ⚠️ 需要強化說明的 SKILL

#### 3. `/handler` SKILL
**現況**（第 59-61 行）：
```markdown
graph LR
    A[Controller] --> B[Handler]
    B --> C[Repository 1]
```

**改進**：
- 添加註釋：「無論 Controller 是 API First 或 Code First 實現，Handler 層邏輯完全相同」
- 在「相關 Skills」中明確指向 `/api-development`

**預期新增內容**：
```markdown
## 注意事項

### Handler 與 API 開發流程無關
Handler 是純業務邏輯層，不受「API First vs Code First」選擇影響：
- **API First**：Controller 實作自動產生的介面 → 呼叫 Handler
- **Code First**：Controller 直接實作 → 呼叫 Handler
- **結果**：Handler 代碼完全相同

因此本 SKILL 未區分 API 開發流程，所有 Handler 實作指導均適用於兩種方式。

👉 **API 開發流程選擇** → 參考 `/api-development` SKILL
```

---

#### 4. `/error-handling` SKILL
**改進**：同上（加入「與 API 開發流程無關」的說明）

---

#### 5. `/bdd-testing` SKILL
**改進**：同上（加入說明 BDD 測試對兩種方式均適用）

---

#### 6. `/middleware` SKILL
**改進**：同上

---

#### 7. `/ef-core` SKILL
**改進**：同上

---

### ⚠️ 需要更新的核心文檔

#### 8. `CLAUDE.md`（AI 助理行為規則）
**現況**：第「強制互動確認」章節

**改進**：新增「API 開發流程決策」規則，**指向既有決策文檔**而非複製內容

**調整內容**（新增至「強制互動確認」小節）：
```markdown
### API 開發流程決策（強制確認）

開發任何新的 API 端點前，必須詢問使用者確認開發方式（API First 或 Code First）。

**決策依據和詳細對比**，參考 [decision-framework.md 的「## API 開發流程決策」章節](./decision-framework.md#api-開發流程決策)

**執行流程參考**：[/api-development SKILL](./skills/api-development/SKILL.md)

⚠️ **禁止混用**：同一專案內不得混用 API First 和 Code First。如需變更開發方式，須進行 Controller 層重構。
```

**原理**：避免在 CLAUDE.md 中複製 decision-framework.md 的決策樹內容，改為規則 + 連結，遵守專案自訂的三層架構分層原則。

---

#### 9. `decision-framework.md`
**現況**：已包含 API 決策邏輯

**改進**：確保「API 開發流程決策」清楚指向 `/api-development` SKILL

---

## 實作步驟

### Phase 1: 核心文檔更新
- [ ] **Step 1.1** 更新 README.md 第 184-191 行（引入雙 Controller 參考）
- [ ] **Step 1.2** ~~更新 command-processor.md~~ **已跳過**（檔案已於前置階段刪除）
- [ ] **Step 1.3** 更新 CLAUDE.md「強制互動確認」章節（新增 API 開發流程決策規則 + 連結）

### Phase 2: SKILL 強化說明
- [ ] **Step 2.1** 更新 `/handler/SKILL.md`（添加「與 API 開發流程無關」說明）
- [ ] **Step 2.2** 更新 `/error-handling/SKILL.md`（同上）
- [ ] **Step 2.3** 更新 `/bdd-testing/SKILL.md`（同上）
- [ ] **Step 2.4** 更新 `/middleware/SKILL.md`（同上）
- [ ] **Step 2.5** 更新 `/ef-core/SKILL.md`（同上）

### Phase 3: 驗證與同步
- [ ] **Step 3.1** 檢查所有 SKILL 的「相關 Skills」章節是否正確指向 `/api-development`
- [ ] **Step 3.2** 檢查 decision-framework.md 是否清楚指向 `/api-development`
- [ ] **Step 3.3** ~~編譯驗證（dotnet build）~~ **改為**：Markdown 連結驗證
  - 驗證 CLAUDE.md 中指向 decision-framework.md 的連結有效
  - 確認 FileResolver 參考路徑正確（含 `src/be/` 前綴）

### Phase 4: Git 提交
- [ ] **Step 4.1** Git add 所有調整檔案
- [ ] **Step 4.2** Git commit: `docs: 同步 API First 與 Code First 的文檔參考與說明`
- [ ] **Step 4.3** **Git push 需使用者授權**（按 health-check 慣例，push/MR 不自動執行）

---

## 受影響的檔案

| 檔案路徑 | 變更類型 | 優先級 |
|---------|--------|--------|
| `.claude/README.md` | 編輯 | P1 |
| `.claude/command-processor.md` | 編輯 | P1 |
| `CLAUDE.md` | 編輯 | P1 |
| `.claude/skills/handler/SKILL.md` | 編輯 | P2 |
| `.claude/skills/error-handling/SKILL.md` | 編輯 | P2 |
| `.claude/skills/bdd-testing/SKILL.md` | 編輯 | P2 |
| `.claude/skills/middleware/SKILL.md` | 編輯 | P2 |
| `.claude/skills/ef-core/SKILL.md` | 編輯 | P2 |
| `.claude/decision-framework.md` | 檢查 | P3 |

---

## 預期成果

✅ 所有文檔都清楚區分 API First 和 Code First  
✅ 新人被正確引導選擇開發方式  
✅ 所有相關 SKILL 都明確說明與開發方式的關係  
✅ CLAUDE.md 強化「強制詢問」原則  

---

## 相關決定

**關鍵決定**：在 CLAUDE.md 中新增「API 開發流程決策」為強制確認項

**原因**：
- 防止新人在同一專案混用 API First 和 Code First
- 確保團隊選擇清晰、一致
- 避免後期重構成本

---

## 預估工作量

- **核心文檔更新**（Phase 1）：~30 分鐘
- **SKILL 強化**（Phase 2）：~45 分鐘
- **驗證與同步**（Phase 3）：~15 分鐘
- **Git 提交**（Phase 4）：~5 分鐘

**總計**：~95 分鐘（1.5 小時）

---

**複雜度**：低-中  
**風險**：低  
**狀態**：待執行

