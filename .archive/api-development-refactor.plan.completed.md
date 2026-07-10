⏹️ **Status**: COMPLETED (2026-07-11 01:18 GMT+8)

此計畫已全部執行完成，已封存至 .archive/  
詳見 Git commit: 4037a59（重命名 MemberV2ControllerImpl.cs → MemberController.cs）

---

# /api-development SKILL 調整計畫

**計畫時間**: 2026-07-11 01:20 GMT+8  
**進度**: ✅ [已完成]  
**優先級**: P1（影響 Controller 層設計指導）

---

## 問題陳述

現有 `/api-development` SKILL 已涵蓋 API First 和 Code First 的決策邏輯，但在以下方面需要強化：

1. **缺少 Code First 的完整開發流程範例** — 只有對比表和簡短描述
2. **Controller 實作指導混淆** — 未明確區分兩種方式的實作差異
3. **參考代碼單一** — 只引用 API First 的 MemberV1ControllerImpl
4. **核心原則不夠明確** — 未強調不得混用兩種方式

---

## 調整清單

### Step 1: 拆分 Controller 實作指導（第 273-285 行）

**目的**：明確區分 API First 和 Code First 的實作方式

**現況**：
```markdown
## Controller 實作指導

產生的 Controller 骨架需要實作自動產生的介面，整合以下元件：
1. Handler 整合
2. Result Pattern 處理
3. HTTP 狀態碼映射
```

**調整內容**：
- 新增子章節「API First: 實作自動產生的介面」
- 新增子章節「Code First: 直接實作無預先介面」
- 添加命名約定對比表
- 更新參考代碼指向兩個版本

**預期行數**：273-285 → 273-320

---

### Step 2: 新增 Code First 開發流程範例（第 347 後新增）

**目的**：提供 Code First 的完整實作指導

**新增內容**：
- 完整的 Controller 範例代碼（MemberV2ControllerImpl）
- 步驟化開發流程（4 個步驟）
- 開發時間線對比（API First vs Code First）
- 適用場景說明

**預期新增行數**：~100 行

**參考代碼**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/Member/MemberController.cs
```

---

### Step 3: 強化核心原則（第 372-384 行）

**目的**：明確禁止混用兩種方式，強化命名約定

**調整內容**：
- 將 3 項原則擴展為 5 項
- 新增「禁止混用」原則
- 新增「命名約定」原則（API First vs Code First）
- 區分「API First 專用」和「Code First 專用」的細節

**關鍵新增**：
1. 強制詢問：明確詢問開發方式
2. 文件優先（API First）/ 文件同步（Code First）區分
3. 禁止混用：同一專案內只能選一種
4. 命名約定：XxxControllerImpl vs XxxController

---

### Step 4: 新增「開發方式確認」問卷問題（第 132 後新增）

**目的**：在選擇流程前明確確認開發方式

**新增問題 2.5**：
```
請確認此專案的 API 開發方式：
1️⃣ API First（推薦用於團隊協作、Client SDK 需求）
2️⃣ Code First（推薦用於快速原型、內部小型專案）
```

**預期行數**：~30 行

---

### Step 5: 更新相關 Skills 列表（第 419-425 行）

**目的**：澄清各 SKILL 與開發方式的關係

**調整內容**：
- 標註「與開發方式無關」的 Skills（handler, error-handling, bdd-testing）
- 新增相關文檔連結
- 新增相關 Agents（architecture-review-agent）

---

## 實作步驟

### Phase 1: 編輯 SKILL.md 檔案
- [ ] **Step 1.1**：編輯第 273-285 行，拆分為 API First / Code First 兩小節（含對比表）
- [ ] **Step 1.2**：在第 347 行後新增 Code First 完整開發流程範例章節
- [ ] **Step 1.3**：編輯第 372-384 行，強化核心原則為 5 項
- [ ] **Step 1.4**：在第 132 行後新增問卷問題 2.5
- [ ] **Step 1.5**：編輯第 419-425 行，更新相關 Skills 列表

### Phase 2: 驗證與測試
- [ ] **Step 2.1**：驗證 FileResolver 參考代碼正確（MemberV1 + MemberV2）
- [ ] **Step 2.2**：檢查 YAML 格式正確性
- [ ] **Step 2.3**：檢查新增程式碼範例無語法錯誤

### Phase 3: 同步相關文件
- [ ] **Step 3.1**：檢查 `decision-framework.md` 是否需要更新
- [ ] **Step 3.2**：檢查 `development-rules.md` 是否需要新增「Controller 命名約定」
- [ ] **Step 3.3**：檢查 `CLAUDE.md` 的 SKILL 索引表是否需要更新

### Phase 4: Git 提交
- [ ] **Step 4.1**：Git add 所有調整檔案
- [ ] **Step 4.2**：Git commit: `refactor: 區分 API First 和 Code First 的 api-development SKILL 指導`
- [ ] **Step 4.3**：Git push

---

## 檔案變更清單

| 檔案 | 變更類型 | 說明 |
|------|--------|------|
| `.claude/skills/api-development/SKILL.md` | 編輯 | 添加 Code First 章節、強化原則 |
| `decision-framework.md` | 檢查 | 檢查 API 決策框架是否需要補充 |
| `development-rules.md` | 檢查 | 檢查是否需要添加「Controller 命名約定」 |
| `CLAUDE.md` | 檢查 | 檢查 SKILL 索引表 |

---

## 預期成果

✅ `/api-development` SKILL 明確指導兩種開發方式  
✅ 新增 Code First 的完整範例流程  
✅ Controller 命名約定清晰可追蹤（Impl vs 無後綴）  
✅ 核心原則強化「禁止混用」  
✅ 參考代碼同時涵蓋 API First（V1）和 Code First（V2）  

---

## 依賴與前置條件

✅ MemberV2ControllerImpl.cs 已調整為 Code First 直接實作（已完成）

---

**預估工作量**：~2-3 小時  
**複雜度**：中等（編輯量大，但邏輯清晰）  
**風險**：低（僅文檔調整，無程式碼變更）
