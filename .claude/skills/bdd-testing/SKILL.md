---
name: bdd-testing
description: BDD 測試實作技能，協助開發者使用 Reqnroll 撰寫行為驅動開發測試，包含 Gherkin 語法、測試步驟實作與 Docker 測試環境設定。
---

### ⚠️ 前置條件
本 SKILL 須搭配閱讀：
- [開發規則](../../development-rules.md)
- [決策框架 - 測試策略決策樹](../../decision-framework.md#測試策略決策樹)

# BDD Testing Skill

## 描述
BDD 測試實作技能，協助開發者使用 Reqnroll 撰寫行為驅動開發測試，包含 Gherkin 語法、測試步驟實作與 Docker 測試環境設定。

## 職責
- Gherkin .feature 檔案撰寫
- 測試步驟實作（Step Definitions）
- Docker 測試環境設定（Testcontainers）
- BDD 測試策略引導

## 注意事項

### BDD Testing 對兩種開發方式均適用

BDD 測試是獨立於 API 實作方式的測試策略，不受「API First vs Code First」選擇影響：
- **API First**：根據生成的 API 規格撰寫 BDD 測試
- **Code First**：根據實作的 Controller 邏輯撰寫 BDD 測試
- **結果**：BDD 測試方式與步驟實作邏輯完全相同

因此本 SKILL 未區分 API 開發流程，所有 BDD 測試實作指導均適用於兩種方式。

👉 **API 開發流程選擇** → 參考 [`/api-development` SKILL](../api-development/SKILL.md)

## 核心原則

### BDD 開發循環
1. **需求分析**：撰寫 Gherkin 情境
2. **測試實作**：實作測試步驟
3. **功能開發**：實作業務邏輯
4. **測試驗證**：執行測試確保符合需求

### Gherkin 語法
```gherkin
Feature: 會員註冊
  作為一個新使用者
  我想要註冊帳號
  以便使用系統功能

  Scenario: 成功註冊新會員
    Given 我是一個新使用者
    When 我使用有效的 Email "user@example.com" 和姓名 "張三" 註冊
    Then 註冊應該成功
    And 我應該收到會員資料
```

### Docker 優先測試策略
- 使用 Testcontainers 提供真實 SQL Server、Redis
- 避免使用 Mock（除非必要）
- 每個測試獨立資料
- 測試後自動清理

### API 測試必須使用 BDD
- 所有 Controller 功能必須使用 BDD 情境測試
- 禁止單獨測試 Controller
- 透過 WebApplicationFactory 執行完整管線

## 測試環境

### Docker 容器
- SQL Server 容器
- Redis 容器
- Seq 日誌容器

### WebApplicationFactory
```csharp
public class TestServer : WebApplicationFactory<Program>
{
    // 設定測試環境
}
```

## 參考文件
- [BDD Gherkin 情境範例](./references/bdd-gherkin-examples.md)
- [Docker Testcontainers 設定](./references/docker-testcontainers-setup.md)

## 實作範例

開發者應參考專案內的實際測試代碼：

**Gherkin 情境範例**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/Tests/Features/Member.feature
```

**測試步驟實作範例**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/Tests/Member/MemberSteps.cs
```
