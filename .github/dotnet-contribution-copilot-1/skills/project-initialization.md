# Skill: Project Initialization

> 專案初始化 Skill - 處理專案狀態檢測與初始化流程

## Skill 職責

本 Skill 負責：
- 檢測專案狀態（空白 vs 現有專案）
- 詢問是否使用 GitHub 範本
- 收集專案配置資訊（資料庫、快取、專案結構）
- 產生/更新 `env/.template-config.json`
- 引導使用者完成專案初始化

## 引用文件

- [互動規則](../references/interaction-rules.md) - 強制互動規範
- [架構概覽](../references/architecture-overview.md) - 專案架構說明

## 執行流程

### 1. 專案狀態檢測

檢查以下條件（任一符合即視為空白專案）：
```powershell
# 檢查 1: 配置檔案
Test-Path "env\.template-config.json"

# 檢查 2: 解決方案檔案
Get-ChildItem -Filter "*.sln"

# 檢查 3: src 目錄
Test-Path "src\" -And (Get-ChildItem "src\").Count -gt 0

# 檢查 4: 核心配置檔案
Test-Path "appsettings.json" -Or Test-Path "docker-compose.yml"
```

**決策邏輯**：
- 所有檢查都失敗 → 空白專案 → 執行初始化流程
- 任一檢查成功 → 現有專案 → 跳過初始化，進入正常工作模式

### 2. GitHub 範本詢問（僅空白專案）

**強制詢問**（不可擅自假設）：
```markdown
🤖 檢測到這是一個空白專案。

是否要使用 GitHub 範本快速建立專案？

1️⃣ 是 - 從 https://github.com/yaochangyu/api.template clone 範本
2️⃣ 否 - 從頭建立專案結構

請輸入 1 或 2：
```

#### 選項 1：使用 GitHub 範本

**安全檢查**：
```powershell
# 檢查工作目錄是否為空
$isEmpty = (Get-ChildItem -Force).Count -eq 0

if (-not $isEmpty) {
    # 目錄非空，詢問使用者
    Write-Host "⚠️ 工作目錄非空，發現以下檔案："
    Get-ChildItem | Select-Object Name
    
    Write-Host "`n請選擇："
    Write-Host "1️⃣ 改用子資料夾"
    Write-Host "2️⃣ 清空目錄後繼續"
    Write-Host "3️⃣ 取消"
}
```

**執行 Clone**：
```powershell
# 在空目錄中
git clone https://github.com/yaochangyu/api.template .

# 刪除 Git 歷史
Remove-Item -Recurse -Force .git
```

#### 選項 2：從頭建立

建立基本專案結構：
```powershell
# 建立目錄結構
New-Item -ItemType Directory -Path "src\be", "src\fe", "env", "doc", "k8s" -Force

# 建立基本檔案
New-Item -ItemType File -Path ".gitignore", "README.md", "Taskfile.yml" -Force
```

### 3. 配置資訊收集（分階段詢問）

**重要原則**：
- 每次最多詢問 3-4 個問題
- 根據前一階段回答決定後續問題
- 所有選項都需要詢問，不可擅自假設

#### 階段 1：資料庫配置

```markdown
🤖 階段 1：資料庫配置

1. 請選擇資料庫類型：
   1️⃣ SQL Server（推薦）
   2️⃣ PostgreSQL
   3️⃣ MySQL
   4️⃣ SQLite（僅開發環境）

2. 資料庫版本：
   （根據資料庫類型顯示對應版本選項）
   
3. 是否使用 EF Core？
   1️⃣ 是（推薦）
   2️⃣ 否（使用 Dapper 或其他）

請輸入選擇（例如：1, 2022, 1）：
```

#### 階段 2：快取配置

```markdown
🤖 階段 2：快取配置

1. 是否使用 Redis 快取？
   1️⃣ 是（推薦用於生產環境）
   2️⃣ 否（僅使用記憶體快取）

2. Redis 版本：（如果選擇使用 Redis）
   1️⃣ 7-alpine（推薦）
   2️⃣ 6-alpine
   3️⃣ latest

請輸入選擇：
```

#### 階段 3：專案結構

```markdown
🤖 階段 3：專案結構組織

請選擇專案組織方式：

1️⃣ 單一專案結構（推薦）
   - 所有層都在同一專案內
   - 適合小型團隊（3 人以下）
   - 編譯快速、部署簡單
   
2️⃣ 多專案結構
   - Controller、Handler、Repository 各自獨立專案
   - 適合大型團隊、明確分工
   - 職責清晰分離

請輸入 1 或 2：
```

### 4. 確認與儲存

```markdown
🤖 配置確認

請確認以下設定：

資料庫：
  - 類型：SQL Server
  - 版本：2022
  - ORM：EF Core

快取：
  - 使用 Redis：是
  - 版本：7-alpine

專案結構：
  - 組織方式：單一專案結構

是否正確？（y/n）
```

**儲存配置**：
```powershell
$config = @{
    database = @{
        type = "SQL Server"
        version = "2022"
        useEfCore = $true
    }
    cache = @{
        useRedis = $true
        version = "7-alpine"
    }
    projectOrganization = "single-project"
    createdAt = (Get-Date).ToUniversalTime().ToString("o")
    createdBy = "GitHub Copilot"
}

$config | ConvertTo-Json | Set-Content "env\.template-config.json"
```

### 5. 專案結構產生/修改

根據配置資訊：
- 修改 `docker-compose.yml`（資料庫、Redis 設定）
- 建立 `appsettings.json`（連線字串範本）
- 建立/更新 `Taskfile.yml`（開發指令）
- 建立 `README.md`（專案說明）

## 輸出成果

執行完成後：
1. ✅ `env/.template-config.json` 已建立
2. ✅ 專案結構已建立或更新
3. ✅ Docker 與配置檔案已就緒
4. ✅ 使用者了解下一步操作

## 錯誤處理

### 目錄非空且使用者取消

```markdown
🤖 已取消專案初始化。

建議：
1. 手動清空目錄
2. 或使用子資料夾：mkdir my-project && cd my-project
3. 重新執行初始化
```

### Git Clone 失敗

```markdown
❌ 無法從 GitHub clone 範本

可能原因：
1. 網路連線問題
2. Git 未安裝或未在 PATH 中

建議：
1. 檢查網路連線
2. 手動 clone：git clone https://github.com/yaochangyu/api.template
3. 或選擇「從頭建立」選項
```

## 使用範例

### 情境 1：使用 GitHub 範本初始化

```markdown
👤 使用者：初始化一個新的 .NET API 專案

🤖 Copilot：
檢測到這是一個空白專案。

是否要使用 GitHub 範本快速建立專案？
1️⃣ 是
2️⃣ 否

👤 使用者：1

🤖 Copilot：
正在從 GitHub clone 範本...
✅ 範本下載完成
✅ 已移除 Git 歷史

階段 1：資料庫配置
（詢問資料庫類型、版本、ORM）

👤 使用者：1, 2022, 1

🤖 Copilot：
階段 2：快取配置
（詢問 Redis 使用與版本）

👤 使用者：1, 1

🤖 Copilot：
階段 3：專案結構
（詢問專案組織方式）

👤 使用者：1

🤖 Copilot：
（顯示配置確認）

👤 使用者：y

🤖 Copilot：
✅ 專案初始化完成！

已建立：
- env/.template-config.json
- docker-compose.yml (SQL Server 2022 + Redis 7)
- appsettings.json
- Taskfile.yml

下一步：
1. 啟動開發環境：task dev-up
2. 建立第一個 API：使用 @dotnet-api-developer
```

### 情境 2：現有專案跳過初始化

```markdown
👤 使用者：初始化專案

🤖 Copilot：
檢測到 env/.template-config.json 已存在。

專案配置：
- 資料庫：SQL Server 2022
- 快取：Redis 7-alpine
- 專案結構：單一專案

專案已初始化，可以直接開始開發。

需要協助嗎？
1️⃣ 實作新的 API
2️⃣ 修改資料庫配置
3️⃣ 其他
```

## 注意事項

### 強制執行的規則

- ✅ 必須詢問是否使用 GitHub 範本（不可擅自假設）
- ✅ 所有配置選項都必須詢問（即使有預設值）
- ✅ 分階段詢問（每次最多 3-4 個問題）
- ✅ 目錄非空時必須詢問使用者處理方式
- ✅ 儲存配置前必須讓使用者確認

### 禁止的行為

- ❌ 不可擅自使用預設值（必須詢問）
- ❌ 不可在目錄非空時直接覆蓋
- ❌ 不可一次詢問超過 4 個問題
- ❌ 不可跳過任何配置步驟

---

**Skill 類型**：互動式初始化  
**預期執行時間**：3-5 分鐘（含使用者互動）  
**相依 Skills**：無  
**適用 Agents**：project-initializer
