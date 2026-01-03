# Skill: Testing Strategy

> 測試策略 Skill - 管理測試需求與實作

## Skill 職責

- 詢問測試需求（BDD/單元測試/兩者/不需要）
- 協助設計 BDD 情境（Gherkin 語法）
- 指導 Docker 測試環境設定
- 提供測試資料準備策略

## 引用文件

- [BDD 測試指南](../references/bdd-testing.md)
- [互動規則](../references/interaction-rules.md)
- [BDD Feature 範本](../assets/bdd-feature-template.feature)
- [BDD Steps 範本](../assets/bdd-steps-template.cs)

## 執行流程

### 1. 測試需求詢問（強制）

```markdown
是否需要實作測試？

1️⃣ 完整測試（BDD 整合測試 + 單元測試）
2️⃣ 僅 BDD 整合測試
3️⃣ 僅單元測試
4️⃣ 暫不實作測試（快速原型、POC 驗證）

請輸入選項：
```

### 2. 測試範圍詢問（如果需要測試）

```markdown
測試範圍為何？

a) 新增功能的完整測試
b) 僅測試核心業務邏輯
c) 僅測試關鍵路徑（Happy Path）
d) 包含異常情境與邊界條件

請輸入選項：
```

### 3. BDD 測試情境設計（如果選擇 BDD）

```markdown
BDD 測試設定：

1. 是否已有 .feature 檔案？
2. 需要新增哪些情境（Given-When-Then）？
3. 是否需要 AI 協助撰寫 Gherkin 語法？

請回答：
```

協助建立：
- Feature 檔案（使用 bdd-feature-template.feature）
- Steps 實作（使用 bdd-steps-template.cs）
- 測試資料準備

### 4. 測試資料準備策略

```markdown
測試資料準備策略：

a) 使用 Docker 容器（資料庫、Redis）✅ 推薦
b) 使用固定測試資料（Seed Data）
c) 每次測試動態產生資料
d) 測試後清理資料

請選擇：
```

### 5. 執行測試

```powershell
# BDD 整合測試
task test-integration

# 單元測試
task test-unit
```

## 核心規則

- ✅ **API 端點測試必須使用 BDD 測試**
- ✅ **優先使用 Testcontainers（Docker 容器）**
- ✅ **禁止對 Controller 進行單元測試**
- ❌ 不可假設測試需求（必須明確詢問）

## 輸出成果

- ✅ .feature 檔案已建立（BDD）
- ✅ Steps 實作已完成（BDD）
- ✅ 測試環境已設定（Docker）
- ✅ 測試執行通過

---

**Skill 類型**：互動式測試設計  
**前置 Skills**：api-development  
**適用 Agents**：dotnet-api-developer
