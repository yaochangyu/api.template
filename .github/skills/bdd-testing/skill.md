---
name: bdd-testing
description: BDD 測試實作技能，協助開發者使用 Reqnroll 撰寫行為驅動開發測試，包含 Gherkin 語法、測試步驟實作與 Docker 測試環境設定。
---

# BDD Testing Skill

## 描述
BDD 測試實作技能，協助開發者使用 Reqnroll 撰寫行為驅動開發測試，包含 Gherkin 語法、測試步驟實作與 Docker 測試環境設定。

## 職責
- Gherkin .feature 檔案撰寫
- 測試步驟實作（Step Definitions）
- Docker 測試環境設定（Testcontainers）
- BDD 測試策略引導

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
- [BDD 測試指南](./references/bdd-testing-guide.md)

## 範本檔案
- [Feature 範本](./assets/feature-template.feature)
- [測試步驟範本](./assets/test-steps-template.cs)
