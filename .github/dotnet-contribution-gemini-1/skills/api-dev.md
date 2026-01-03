# API 開發 Skill (`api-dev`)

## 職責
此 Skill 負責引導使用者完成 API 功能的實作，從設計選擇到程式碼生成和實作骨架，涵蓋 Controller、Handler 和 Repository 各層。

## 核心流程
本 Skill 的核心流程基於 `CLAUDE.md` 中「開發工作流程」的「標準開發流程」與「API First 開發流程詳解」。

### 1. API 開發流程選擇 (強制互動)
本 Skill 會首先強制詢問使用者選擇「API First」或「Code First」流程，並根據選擇進入不同的子流程。
- **API First（推薦）**: 建議先定義 OpenAPI 規格 (`doc/openapi.yml`)，再透過工具產生 Controller 骨架。
- **Code First**: 允許直接實作程式碼，後續再手動維護 OpenAPI 規格。

### 2. 實作分層 (強制互動)
本 Skill 會詢問使用者需要實作哪些分層：
- Controller (HTTP 請求處理與路由)
- Handler (核心業務邏輯處理)
- Repository (資料存取與資料庫操作)

### 3. 程式碼生成與實作引導
- **Controller**:
    - 若選擇 API First，會指引使用者更新 OpenAPI 規格，然後執行 `task codegen-api-server` 產生介面。
    - 提供 `assets/controller-template.cs` 作為實作範本。
- **Handler**:
    - 提供 `assets/handler-template.cs` 作為實作範本，並引導使用者撰寫業務邏輯、錯誤處理（參考 `references/error-handling-guide.md`）。
- **Repository**:
    - 引導使用者選擇 Repository 設計策略（資料表導向或需求導向），並提供 `assets/repository-demand-oriented-template.cs` 作為實作範本。
    - 強調參考 `references/repository-pattern-guide.md` 中的設計原則。

## 引用文件
- `references/interaction-rules.md`: 用於所有使用者互動的規範。
- `references/architecture-guide.md`: 提供分層架構和技術堆疊的背景知識。
- `references/repository-pattern-guide.md`: 詳細說明 Repository Pattern 的設計哲學與決策清單。
- `references/error-handling-guide.md`: 指導如何使用 Result Pattern 處理錯誤。
- `assets/controller-template.cs`: Controller 程式碼骨架。
- `assets/handler-template.cs`: Handler 程式碼骨架。
- `assets/repository-demand-oriented-template.cs`: 需求導向 Repository 程式碼骨架。

## 輸出
- 根據使用者選擇生成的 Controller、Handler、Repository 程式碼骨架。
- 指導使用者完成各層次的業務邏輯實作。

## 範例互動流程 (Agent 調用此 Skill 時)
1. **Agent**: "您好！我們將開始實作新的 API 功能。首先，請選擇 API 開發流程：\n1) API First (推薦：先定義 OpenAPI 規格，再產生程式碼骨架)\n2) Code First (直接實作程式碼，後續手動維護 OpenAPI)"
2. **使用者**: 選擇 "1"。
3. **Agent**: "好的，API First 流程。請問此 API 的 OpenAPI 規格狀態為何？\n1) 已定義 (`doc/openapi.yml` 已包含此 API 規格定義)\n2) 需要更新 (`doc/openapi.yml` 需要修改加入新的 endpoint)\n3) 尚未定義 (需要從頭建立 OpenAPI 規格)"
4. **使用者**: 選擇 "3"。
5. **Agent**: "請提供新 API 的 OpenAPI 規格內容，我將協助您更新 `doc/openapi.yml`。"
6. ... (使用者提供規格後，Agent 執行 `task codegen-api-server`，然後提供 Controller 骨架)
7. **Agent**: "接下來，您需要實作哪些分層？ (可多選，以逗號分隔)\n1) Controller\n2) Handler\n3) Repository"
8. **使用者**: 選擇 "1,2,3"。
9. **Agent**: 提供 Controller、Handler、Repository 的程式碼骨架，並指導實作。
