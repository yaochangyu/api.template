# Claude Code WebAPI Framework

專為 JobBank1111 API 專案設計的程式碼產生框架，實作方案一：Command 模式 - Slash Commands。

---

## 📚 三層文檔系統

本專案採用分層文檔架構，適應不同使用者需求：

| 層級 | 檔案名 | 使用對象 | 內容 |
|------|--------|---------|------|
| **第 1 層** | [CLAUDE.md](../CLAUDE.md) | Claude Code AI 助理 | AI 助理行為規則、互動原則、計畫管理 |
| **第 2 層** | [development-rules.md](./development-rules.md) | 所有開發者 | 開發指令、架構概述、強制詢問情境、禁止行為、最佳實踐 |
| **第 3 層** | [decision-framework.md](./decision-framework.md) | 特定工作流 | API 流程決策、測試策略、資料庫選擇、快取設計、效能優化 |

**使用建議**：
- 實作功能前，先讀 development-rules.md（了解規則與詢問情境）
- 面臨決策點時，查閱 decision-framework.md（取得決策邏輯）
- SKILL 使用時，檢查 SKILL.md 頂部的前置條件提示

---

## 📁 目錄結構

```
.claude/
├── README.md                   # 此說明文件
├── agents/                     # Agent 定義（4 個）
│   ├── architecture-review.md  # 架構檢視與反模式檢查
│   ├── feature-development.md  # 完整功能開發流程協調
│   ├── project-setup.md        # 專案初始化引導
│   └── testing-strategy.md     # 測試策略規劃
├── skills/                     # Skill 定義（16 個，主檔一律為 SKILL.md）
│   ├── api-development/ bdd-testing/ ef-core/
│   ├── error-handling/ handler/ middleware/ project-init/
│   ├── repository-design/ skill-agent-design/ skill-creator/
│   ├── security-check-config/ security-check-dependencies/
│   ├── security-check-secrets/ security-deep-review/
│   ├── security-fast-scan/     # （assets/ 內含共用安全報告範本）
│   └── webapi-real-testing/
└── agents-workflow-guide.md    # Agent 工作流程指南
```

> **模板管理**：所有程式碼實作參考現已改為使用 FileResolver 動態取得真實項目代碼，
> 確保參考範例始終與生產代碼同步。詳見各 SKILL 的「實作參考」章節。

## 🚀 快速開始

所有開發工作流程現已透過 SKILL 系統實作，請使用相應的 Skill 指令：

- **API 端點設計**: `/api-development`
- **Controller 實作**: `/handler`
- **資料存取層**: `/repository-design`
- **中介軟體**: `/middleware`
- **錯誤處理**: `/error-handling`
- **EF Core 最佳化**: `/ef-core`
- **測試策略**: `/bdd-testing`

## 🎯 設計原則

所有產生的程式碼都遵循：

### Clean Architecture 原則
- **分層架構**: Handler (業務邏輯) → Repository (資料存取) → Controller (API 層)
- **依賴反向**: 高層模組不依賴低層模組，都依賴於抽象
- **關注點分離**: 每層專注於自己的職責

### 專案規範 (CLAUDE.md)
- **Result Pattern**: 統一的錯誤處理模式
- **TraceContext**: 完整的請求追蹤機制
- **日誌整合**: 結構化日誌和錯誤記錄
- **快取策略**: 多層快取和失效機制
- **驗證邏輯**: 連續驗證和錯誤回應

### 程式碼品質
- **不可變物件**: 使用 record 類型和 init 屬性
- **異步操作**: 所有 I/O 操作使用 async/await
- **錯誤處理**: 統一的 Failure 物件和狀態碼對應
- **測試友善**: 相依性注入和可測試設計

## 🔧 架構與程式碼參考

所有程式碼範本現已改為透過 FileResolver 動態取得真實項目代碼，確保參考始終與生產代碼同步。詳見各 SKILL 文件的「實作參考」章節。

### Controller 實作參考（API First vs Code First）

根據所選開發方式，可參考對應的生產代碼範例：

**API First 方式**（規格優先）
```bash
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/Member/MemberV1ControllerImpl.cs
```
適用於：API 規格已確定，自動產生 Controller 實作

**Code First 方式**（實作優先）
```bash
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/Member/MemberController.cs
```
適用於：直接實作 Controller，無規格自動生成

👉 **如何選擇?** 參考 [/api-development SKILL](./skills/api-development/SKILL.md) 的開發方式決策說明

## 📝 最佳實務

### 建議的建立順序

1. **先建立實體層**:
   ```bash
   # 確保 EF 實體已存在於 JobBank1111.Job.DB
   ```

2. **建立資料存取層**:
   ```bash
   /webapi:repository Product
   ```

3. **建立業務邏輯層**:
   ```bash
   /webapi:handler Product
   ```

4. **建立 API 層**:
   ```bash
   /webapi:controller Product
   ```

5. **註冊依賴注入**:
   ```csharp
   // 在 Program.cs 中註冊
   builder.Services.AddScoped<ProductRepository>();
   builder.Services.AddScoped<ProductHandler>();
   ```

### 後續工作檢查清單

產生程式碼後，通常需要：

- [ ] 建立或更新 Request/Response 模型類別
- [ ] 更新 OpenAPI 規格 (`dotnet-project-template/doc/openapi.yml`)
- [ ] 執行 `task codegen-api` 重新產生 Contract
- [ ] 更新依賴注入註冊
- [ ] 撰寫單元測試和整合測試
- [ ] 執行 `task api-dev` 測試功能

