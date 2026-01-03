## BDD 開發流程 (行為驅動開發)

專案採用 BDD 開發模式，使用 Docker 容器作為測試替身，確保需求、測試與實作的一致性。

### BDD 開發循環

#### 1. 需求分析階段
使用 Gherkin 語法定義功能情境，參考：[src/be/JobBank1111.Job.IntegrationTest/_01_Demo/](src/be/JobBank1111.Job.IntegrationTest/_01_Demo/) 目錄下的 `.feature` 檔案。

#### 2. 測試實作階段
使用 Reqnroll 與真實 Docker 服務實作測試步驟，參考測試步驟實作檔案。

#### 3. Docker 測試環境
完全基於 Docker 的測試環境，避免使用 Mock。包含：
- SQL Server 容器
- Redis 容器
- Seq 日誌容器

📝 **測試環境設定參考**: [src/be/JobBank1111.Job.IntegrationTest/TestServer.cs](src/be/JobBank1111.Job.IntegrationTest/TestServer.cs)

### Docker 優先測試策略

#### 核心原則
- **真實環境**: 使用 Docker 容器提供真實的資料庫、快取、訊息佇列等服務
- **避免 Mock**: 只有在無法使用 Docker 替身的外部服務才考慮 Mock
- **隔離測試**: 每個測試使用獨立的資料，測試完成後自動清理
- **並行執行**: 利用 Docker 容器的隔離特性支援測試並行執行

📝 **測試輔助工具參考**: [src/be/JobBank1111.Job.IntegrationTest/TestAssistant.cs](src/be/JobBank1111.Job.IntegrationTest/TestAssistant.cs)

### API 控制器測試指引

#### 核心原則
- **BDD 優先**: 所有控制器功能必須優先使用 BDD 情境測試
- **禁止單獨測試控制器**: 不應直接實例化控制器進行單元測試
- **強制使用 WebApplicationFactory**: 所有測試必須透過完整的 Web API 管線與 Docker 測試環境
- **情境驅動開發**: 從使用者行為情境出發
