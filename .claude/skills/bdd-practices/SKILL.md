---
name: bdd-practices
description: Cucumber/Gherkin BDD 最佳實踐與測試策略完整指南。涵蓋 BDD 核心原則、Docker 優先測試策略、API 控制器測試規範，提供快速上手的 Gherkin 範本與步驟定義範例。
---

# BDD 最佳實踐技能

## 概述

本技能提供完整的行為驅動開發（BDD）指導，包括 Gherkin 撰寫原則、測試策略與實踐方法。內容涵蓋從需求探索（Discovery）到自動化測試（Automation）的三階段流程。

## 職責

- ✅ **Gherkin 情境審查與改進** — 檢視並提升情境品質
- ✅ **BDD 反模式識別** — 發現常見的 BDD 錯誤並提供修正建議
- ✅ **測試策略諮詢** — 協助制訂 API 端點與整合測試策略
- ✅ **Discovery Workshop 引導** — 協助團隊進行需求探索工作坊
- ✅ **範本與工具提供** — 提供即用的 .feature 檔案、步驟定義範本

## 核心概念速覽

### BDD 三階段

1. **Discovery（探索）** — 透過工作坊建立共識，發現規則與範例
2. **Formulation（表述）** — 將範例轉化為結構化的 Gherkin 情境
3. **Automation（自動化）** — 以情境引導開發，建立自動化測試

### 核心原則

- **宣告式優先** — 描述「做什麼」而非「怎麼做」
- **行為驅動** — 焦點在可觀察的結果，而非實作細節
- **一情境一行為** — 每個 Scenario 專注單一行為
- **可讀性至上** — 非技術人員應能理解情境

---

## 深入閱讀

### 📚 完整文檔

| 文檔 | 內容 | 適用場景 |
|------|------|--------|
| [**BDD 基礎原則**](./references/bdd-principles.md) | Gherkin 撰寫規範、常見反模式、Scenario Outline 最佳實踐 | Gherkin 情境編寫、情境審查 |
| [**Docker 優先測試策略**](./references/docker-testing-strategy.md) | 容器隔離、測試替身優先級、效能優化、並行執行 | 整合測試、E2E 測試設計 |
| [**API 控制器測試指引**](./references/api-controller-testing.md) | BDD 測試必須性、WebApplicationFactory、常見情境模式 | API 端點測試、測試隔離 |

### 🔧 實作範例

開發者應參考專案內的實際代碼：

**BDD 情境範例**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/Tests/Features/Member.feature
```

**測試步驟實作範例**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/Tests/Member/MemberSteps.cs
```

**WebApplicationFactory 設置範例**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/Tests/Infrastructure/TestServerFactory.cs
```

---

## 快速開始

### 場景 1：編寫新的 BDD 情境

**步驟**：
1. 參考專案內已有的 `.feature` 檔案（透過 FileResolver 取得）
2. 填入功能名稱與背景條件
3. 參考 [BDD 基礎原則](./references/bdd-principles.md) 中的規範撰寫情境
4. 檢查：宣告式？一情境一行為？給非技術人員清晰嗎？

**常見問題**：
- ❓ 不確定情境是否宣告式？→ 參考「[描述行為，非實作](./references/bdd-principles.md#描述行為非實作)」
- ❓ 步驟過多？→ 參考「[步驟長度建議](./references/bdd-principles.md#步驟長度建議)」

### 場景 2：設置 API 測試

**步驟**：
1. 參考專案內的 `TestServerFactory` 實作（透過 FileResolver 取得）
2. 調整資料庫連線、DI 配置
3. 實作 `ApplyMigrationsAsync()` 與 `SeedTestDataAsync()`
4. 參考 [API 控制器測試指引](./references/api-controller-testing.md) 建立測試類別

**核心規則**：
- ✅ **必須使用 BDD** — 所有 API 端點測試使用 .feature + Reqnroll
- ✅ **完整 Web 管線** — 使用 WebApplicationFactory，不直接測試 Controller
- ✅ **優先 Docker** — 使用 Testcontainers，避免 Mock

### 場景 3：審查現有情境

**使用此技能進行審查**：
1. 貼上您的 Gherkin 情境
2. 技能會掃描常見反模式（程序驅動、多個 When-Then、硬編碼資料等）
3. 提供改進建議，包括「為什麼」和「正確做法」

**審查清單**：
- [ ] 情境描述「做什麼」而非「怎麼做」？
- [ ] 每個情境只涵蓋一個行為？
- [ ] 使用第三人稱現在式？
- [ ] 標題清晰簡潔？
- [ ] 無實作細節（URL、按鈕名稱、欄位名稱）？

---

## 互動式引導

### 需要 Discovery Workshop 幫助？

詢問如下：
```
我們要開發「[功能名稱]」功能，請引導我進行需求探索。
```

技能會：
1. 逐一提問，理解主要使用者、目標、規則
2. 幫助識別邊界情況與異常流程
3. 協助提煉具體範例

### 需要情境修正建議？

貼上您的情境並詢問：
```
請審查這個情境並提供改進建議：
[您的 Gherkin 情境]
```

技能會：
1. 識別反模式與問題
2. 提供改進版本
3. 解釋每項修改的原因

---

## 參考資源

- [Cucumber 官方文件](https://cucumber.io/docs/bdd/)
- [Gherkin 參考](https://cucumber.io/docs/gherkin/reference/)
- [Automation Panda - BDD 最佳實踐](https://automationpanda.com/)

---

**版本**: 2.0 | **更新於**: 2026-07-10
