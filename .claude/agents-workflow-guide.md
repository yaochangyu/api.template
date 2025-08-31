# AI Agents 開發工作流程指引

基於專案中 15 個專業 AI agents 的完整開發工作流程和呼叫方式。

## 📋 完整開發流程

### 1. 專案初始化與分析
```
@steering-architect 分析現有程式碼庫並建立專案指導文件
```
- **輸出**: `.ai-rules/` 資料夾與核心專案指導文件
- **用途**: 建立專案架構理解，為後續開發奠定基礎
- **時機**: 專案開始、重大架構變更後

### 2. 商業需求規劃 (可選)
```
@brd-writer 撰寫新電商平台的商業需求規劃書
```
- **輸出**: BRD 文件，包含商業目標、資源需求、效益分析
- **適用**: 新產品開發、向高層提案時使用
- **觸發**: 需要董事會審核或投資決策時

### 3. 市場需求分析 (可選)
```  
@mrd-writer 分析目標客群和競爭對手，制定市場需求規劃
```
- **輸出**: MRD 文件，包含市場分析、競爭分析、產品定位
- **適用**: BRD 通過後，需要詳細市場分析時使用
- **時機**: 產品市場化前的策略規劃

### 4. 產品需求文件
```
@prd-writer 為用戶認證系統建立完整的產品需求文件
```
- **輸出**: PRD 文件，包含功能規格、使用者故事、驗收標準
- **用途**: 將商業需求轉換為具體的產品功能需求
- **關鍵**: 連接商業需求與技術實作的橋樑

### 5. 技術規格規劃
```
@strategic-planner 規劃用戶認證功能的技術設計與任務分解
```
- **輸出**: `specs/user-authentication/requirements.md`, `design.md`, `tasks.md`
- **用途**: 將產品需求轉換為技術實作規格
- **特色**: 創建結構化規格文件，不撰寫程式碼

### 6. 規格驗證 (自動觸發)
系統會自動呼叫以下驗證 agents：
- `@spec-requirements-validator` - 驗證需求完整性
- `@spec-design-validator` - 驗證設計技術可行性  
- `@spec-task-validator` - 驗證任務可實作性

### 7. 逐步功能實作
```
@task-executor 執行 specs/user-authentication/tasks.md 中的任務
```
- **行為**: 執行特定任務，自動標記完成狀態
- **重複**: 直到所有任務完成
- **精準度**: 以外科手術般的精確度執行預先核准的任務

或者自主執行：
```
@task-executor 自己繼續實作剩餘的任務，我要下班了
```

### 8. ASP.NET Core 專項開發
```
@clean-architecture-engineer 實作 JobPosting 處理器，遵循乾淨架構原則
```
- **專長**: Clean Architecture、Result Pattern、錯誤處理
- **輸出**: Handler、Repository、完整的業務邏輯實作
- **原則**: 依據 Clean Architecture 原則與專案規範產生程式碼

### 9. 中介軟體開發
```
@middleware-architect 設計身分驗證中介軟體
```
- **專長**: ASP.NET Core 中介軟體設計與實作
- **輸出**: 遵循專案分層架構的中介軟體
- **整合**: 自動遵循專案的分層架構和職責分離原則

### 10. 效能最佳化
```
@performance-optimizer 最佳化 API 回應時間和記憶體使用
```
- **專長**: 快取策略、資料庫最佳化、記憶體管理、並發處理
- **輸出**: 效能最佳化建議與實作
- **範圍**: 涵蓋快取、資料庫、記憶體管理與並發處理

### 11. 安全性強化
```
@api-security-specialist 加強 API 的輸入驗證和授權機制
```
- **專長**: 輸入驗證、授權機制、安全標頭、威脅防護
- **輸出**: 安全防護實作與配置
- **防護**: 包含輸入驗證、授權機制、安全標頭與威脅防護

### 12. 測試策略與實作
```
@testing-engineer 為用戶認證功能設計測試策略
```
- **專長**: 單元測試、整合測試、BDD 情境測試、效能測試
- **輸出**: 完整的測試套件與測試策略
- **覆蓋**: 單元、整合、效能、BDD 情境測試

### 13. 程式碼審查
```
@aspnetcore-code-reviewer 深度分析專案架構和效能最佳化建議
```
- **專長**: 架構設計、效能分析、安全檢查、最佳實務審查
- **輸出**: 詳細的程式碼審查報告與改進建議
- **深度**: 架構、效能、安全性、最佳實務全方位分析

### 14. 資料分析 (需要時)
```
@data-scientist 分析用戶登入行為數據並產生報告
```
- **專長**: SQL 查詢、BigQuery 操作、資料視覺化、統計分析
- **輸出**: 資料分析報告與洞察
- **應用**: 資料分析、SQL 查詢、BigQuery 操作、資料視覺化

### 15. 錯誤排除
```
@error-debugger 調試資料庫連線錯誤問題
```
- **觸發**: 遇到技術錯誤、測試失敗、系統異常時使用
- **專長**: 錯誤診斷、根因分析、解決方案提供
- **範圍**: 技術錯誤、程式碼失敗、測試失敗、例外、系統故障

## 🔄 迭代開發循環

### 新功能開發循環
```
# 步驟 1: 規劃新功能
@strategic-planner 規劃支付系統功能

# 步驟 2: 實作功能  
@clean-architecture-engineer 實作支付處理器
@middleware-architect 設計支付安全中介軟體

# 步驟 3: 測試與最佳化
@testing-engineer 為支付系統建立測試
@performance-optimizer 最佳化支付流程效能
@api-security-specialist 加強支付 API 安全性

# 步驟 4: 審查與部署
@aspnetcore-code-reviewer 審查支付系統實作
@task-executor 執行剩餘的整合任務
```

### 程式碼品質提升循環
```
# 步驟 1: 深度分析
@aspnetcore-code-reviewer 分析整個專案的程式碼品質

# 步驟 2: 專項最佳化
@performance-optimizer 針對效能瓶頸進行最佳化
@api-security-specialist 強化安全防護措施

# 步驟 3: 測試驗證
@testing-engineer 補強測試覆蓋率和測試品質

# 步驟 4: 錯誤修復
@error-debugger 處理發現的問題和錯誤
```

## 🎯 特殊情境使用指南

### 專案啟動情境
```
# 全新專案啟動
@steering-architect 分析現有程式碼庫並建立專案指導文件
@strategic-planner 規劃整體專案架構和核心功能
@clean-architecture-engineer 建立基礎架構模板
@testing-engineer 設定測試框架和 CI/CD 流程
```

### 既有專案重構情境
```
# legacy 系統現代化
@steering-architect 更新專案指導文件以反映新架構
@aspnetcore-code-reviewer 全面分析現有程式碼
@performance-optimizer 識別效能瓶頸
@api-security-specialist 進行安全性稽核
@clean-architecture-engineer 逐步重構為 Clean Architecture
```

### 生產問題排除情境
```
# 緊急問題處理
@error-debugger 分析生產環境錯誤日誌
@aspnetcore-code-reviewer 審查相關程式碼區塊
@performance-optimizer 檢查效能指標
@api-security-specialist 檢查是否為安全性問題
@testing-engineer 建立回歸測試避免問題重現
```

## 🛠️ 與專案工具整合

### 結合 /jb 指令使用
```
# 建立新實體的完整流程
@strategic-planner 規劃 Product 實體的需求和設計
/jb:handler Product        # 產生 Handler
/jb:controller Product     # 產生 Controller
/jb:repository Product     # 產生 Repository
@testing-engineer 為 Product 實體建立完整測試套件
@aspnetcore-code-reviewer 審查產生的 Product 相關程式碼
```

### 結合 /webapi 指令使用
```
# 快速建立功能模組
@strategic-planner 規劃 Order 管理功能
/webapi full Order         # 一次產生完整功能模組
@api-security-specialist 為 Order API 加強安全防護
@performance-optimizer 最佳化 Order 相關的查詢效能
```

## 💡 最佳實務原則

### 1. 循序漸進原則
**基礎流程**: 1 → 5 → 7 → 8 → 11 → 12 → 13
- 先分析專案 → 規劃功能 → 執行任務 → 實作程式碼 → 加強安全 → 建立測試 → 審查程式碼

### 2. 適時驗證原則
- 讓系統自動觸發驗證 agents (步驟 6)
- 每個重要階段後都進行程式碼審查

### 3. 專業分工原則
- 針對特定技術領域使用專門的 agents
- 避免單一 agent 處理過多不同類型的任務

### 4. 持續改善原則
- 定期使用 @aspnetcore-code-reviewer 確保程式品質
- 遇到問題時立即使用 @error-debugger
- 重大變更後使用 @steering-architect 更新指導文件

### 5. 安全優先原則
- 所有 API 開發完成後必須經過 @api-security-specialist 審查
- 敏感功能（如支付、認證）優先考慮安全防護

### 6. 效能意識原則
- 核心業務功能完成後使用 @performance-optimizer 進行最佳化
- 上線前進行完整的效能測試

### 7. 測試驅動原則
- 重要功能必須有完整的測試覆蓋
- 使用 @testing-engineer 建立多層次測試策略

## 🚀 快速開始範例

### 建立新的會員管理功能
```
# 完整流程範例
@strategic-planner 規劃會員管理功能
# 輸出: specs/member-management/requirements.md, design.md, tasks.md

@task-executor 執行 specs/member-management/tasks.md 中的任務
# 或使用工具: /webapi full Member

@api-security-specialist 為會員 API 加強安全防護
@performance-optimizer 最佳化會員查詢效能
@testing-engineer 建立會員功能的完整測試
@aspnetcore-code-reviewer 審查會員管理模組
```

### 既有功能重構
```
# 重構產品管理功能
@aspnetcore-code-reviewer 分析現有 Product 相關程式碼
@clean-architecture-engineer 重構 Product 模組符合 Clean Architecture
@performance-optimizer 最佳化產品查詢和快取策略
@testing-engineer 補強產品功能的測試覆蓋
```

這個工作流程確保從商業需求到技術實作的完整開發週期，每個 agent 都有明確的職責分工和輸出期待，同時與專案的程式碼產生工具完美整合。