# EF Core Migration 戰略指南

本文詳細說明 Entity Framework Core Migration 的版本控制、線上遷移和多環境管理策略。

## 目錄
- [Code First vs Database First](#code-first-vs-database-first)
- [Migration 版本控制](#migration-版本控制)
- [線上 Migration 最佳實踐](#線上-migration-最佳實踐)
- [資料庫架構演化](#資料庫架構演化)
- [多環境 Migration](#多環境-migration)
- [常見 Migration 問題](#常見-migration-問題)

## Code First vs Database First

### Code First（程式碼優先）

**工作流程**
```
定義 EF Core 實體 → 建立 Migration → 更新資料庫
```

#### 優點
- ✅ 版本控制友善（Migration 檔案在 Git 中）
- ✅ 開發速度快（不需要手動寫 SQL）
- ✅ 跨資料庫支援（SQL Server、PostgreSQL 等）
- ✅ 無縫的 ORM 對映

#### 缺點
- ❌ 需要預先設計實體模型
- ❌ 可能產生次優的 SQL schema
- ❌ 某些複雜場景需要手動 SQL

#### 適用情況
- ✅ 新專案
- ✅ 前端/後端並行開發
- ✅ 快速原型與迭代
- ✅ 注重版本控制與協作

### Database First（資料庫優先）

**工作流程**
```
現有資料庫 → 反向工程產生實體 → 開發 EF Core 邏輯
```

#### 優點
- ✅ 與現有資料庫相容（遺留系統）
- ✅ DBA 可獨立管理 schema
- ✅ 可利用資料庫特定功能

#### 缺點
- ❌ Migration 需要手動撰寫
- ❌ 實體變更需要同步更新
- ❌ 不適合頻繁變更的開發週期

#### 適用情況
- ✅ 遺留系統整合
- ✅ DBA 集中管理資料庫
- ✅ 資料庫結構穩定

### 選擇指南

| 因素 | Code First | Database First |
|-----|-----------|-----------------|
| **開發速度** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **版本控制** | ⭐⭐⭐⭐⭐ | ⭐⭐ |
| **現有資料庫** | ⭐ | ⭐⭐⭐⭐⭐ |
| **跨資料庫** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **學習曲線** | ⭐⭐⭐⭐ | ⭐⭐ |

**本專案採用**：Code First

## Migration 版本控制

### 命名規範

#### ✅ 推薦格式
```
YYYYMMDD_HHmmss_DescriptiveAction.cs
20250710_143022_AddMemberTable.cs
20250710_150530_AddIndexToOrdersTable.cs
20250711_091045_RenameMemberNameColumn.cs
```

#### 好的 Migration 名稱
- `AddMemberTable` - 明確說明行動
- `AddUniqueIndexToEmail` - 具體而實用
- `CreateOrderAggregateSchema` - 清晰的結構
- `MigrateUserTypesToEnum` - 清楚的意圖

#### 不好的名稱
- ❌ `Update1` - 無法理解用途
- ❌ `FixBug` - 過於籠統
- ❌ `RemoveColumn` - 不說明是哪個資料表

### Migration 命令

#### 建立 Migration
```bash
# 基本用法
task ef-migration-add MIGRATION_NAME=AddMemberTable

# 這會產生兩個檔案：
# - Migrations/20250710_143022_AddMemberTable.cs (增加變更)
# - Migrations/20250710_143022_AddMemberTable.Designer.cs (快照)
```

#### 更新資料庫
```bash
# 套用所有待處理的 Migration
task ef-database-update

# 套用特定數量的 Migration
task ef-database-update --migration 20250710_143022_AddMemberTable

# 回滾至特定 Migration
task ef-database-update --migration 20250709_120000_PreviousMigration
```

### 暫存與版本控制

#### ✅ 應該提交的
```
Migrations/20250710_143022_AddMemberTable.cs (Up/Down 邏輯)
Migrations/20250710_143022_AddMemberTable.Designer.cs (快照)
Migrations/MigrationSnapshot.cs (目前最新狀態)
```

#### ❌ 不應該提交的
```
obj/ (編譯輸出)
bin/ (編譯輸出)
*.db (本機測試資料庫)
*.log (執行日誌)
```

### 回滾策略

#### 部分回滾
```bash
# 回滾至前一個 Migration
task ef-database-update --migration <previous-migration-name>
```

#### 完全回滾
```bash
# 移除所有 Migration，回到初始狀態
task ef-database-update --migration 0
```

## 線上 Migration 最佳實踐

### 大表遷移策略

#### 問題：大表鎖定造成停機
```csharp
// ❌ 危險：整個資料表被鎖定
public override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "NewColumn",
        table: "Orders",
        type: "nvarchar(255)",
        nullable: false,
        defaultValue: "");
    // 對 1,000 萬筆訂單的表，造成 30 秒鎖定！
}
```

#### ✅ 零停機遷移方案

```csharp
// 步驟 1：新增新欄位（允許 NULL）
public override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "NewColumn",
        table: "Orders",
        type: "nvarchar(255)",
        nullable: true); // 允許 NULL，避免鎖定
}

// 步驟 2：背景 Job 逐批填充資料
// 步驟 3：後續 Migration 中設為 NOT NULL
public override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<string>(
        name: "NewColumn",
        table: "Orders",
        type: "nvarchar(255)",
        nullable: false);
}
```

### 索引變更

#### ❌ 錯誤：同步修改造成停機
```csharp
migrationBuilder.DropIndex(name: "IX_OldIndex", table: "Orders");
migrationBuilder.CreateIndex(name: "IX_NewIndex", table: "Orders", columns: new[] { "NewColumn" });
// 大表會完全鎖定！
```

#### ✅ 最佳實踐：非同步建置索引
```csharp
public override void Up(MigrationBuilder migrationBuilder)
{
    // SQL Server 特定：ONLINE = ON
    migrationBuilder.Sql(
        @"CREATE NONCLUSTERED INDEX [IX_NewIndex] 
          ON [Orders]([NewColumn]) 
          WITH (ONLINE = ON)");
    
    // 之後再刪除舊索引
}
```

### 陷阱

| 陷阱 | 原因 | 解決方案 |
|-----|------|--------|
| **添加 NOT NULL 欄位無預設值** | 資料庫鎖定 | 先允許 NULL，後續再修改 |
| **重新命名欄位** | 應用程式中斷 | 使用別名，並行支援新舊名稱 |
| **刪除索引後新增** | 暫時性能下降 | 先新增，後續再刪除舊索引 |
| **變更欄位類型** | 資料遺失風險 | 建立新欄位，資料轉移，刪除舊欄位 |

## 資料庫架構演化

### 漸進修改策略

#### 階段 1：準備期（新舊並行）
```csharp
public class Order
{
    public Guid Id { get; set; }
    
    // 舊欄位（保留供相容性）
    public string? OldStatusCode { get; set; }
    
    // 新欄位（使用中）
    public OrderStatus NewStatus { get; set; }
}
```

#### 階段 2：過渡期（轉移資料）
```sql
-- 背景 Job 逐批填充資料
UPDATE Orders 
SET NewStatus = CASE 
    WHEN OldStatusCode = 'P' THEN 0 -- Pending
    WHEN OldStatusCode = 'C' THEN 1 -- Completed
    ELSE 2 -- Unknown
END
WHERE NewStatus IS NULL
  AND OldStatusCode IS NOT NULL;
```

#### 階段 3：清理期（移除舊欄位）
```csharp
public override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(name: "OldStatusCode", table: "Orders");
}
```

## 多環境 Migration

### 環境設定

#### 本機開發環境
```bash
# 使用本機 SQL Server 或 LocalDB
dotnet ef database update
```

#### 測試環境
```bash
# 使用 Docker 容器
docker-compose up -d sqlserver

# 套用 Migration
dotnet ef database update --environment Test
```

#### 生產環境
```bash
# 只讀驗證，不自動更新
dotnet ef migrations list --environment Production

# 需要 DBA 手動審批且執行
# 或使用 CI/CD Pipeline 管道
```

### CI/CD 管線整合

```yaml
# GitHub Actions 示例
name: Database Migration

on: [pull_request, push]

jobs:
  migrate:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: YourPassword123!
        options: >-
          --health-cmd="/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword123! -Q 'SELECT 1'"
          --health-interval=10s
          --health-timeout=5s
          --health-retries=5
    
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Validate migrations
        run: dotnet ef migrations list
      
      - name: Apply migrations
        run: dotnet ef database update
```

## 常見 Migration 問題

### 問題 1：Migration 重複執行

**症狀**：Migration 已套用但再次執行時又套用
**原因**：__EFMigrationsHistory 表損毀或不同步
**解決方案**：
```sql
-- 檢查已套用的 Migration
SELECT * FROM __EFMigrationsHistory ORDER BY Id DESC;

-- 手動新增缺失的 Migration 記錄（謹慎使用）
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20250710143022_AddMemberTable', '8.0.0');
```

### 問題 2：Migration 衝突

**症狀**：多個分支新增 Migration，合併時衝突
**解決方案**：
```bash
# 1. 從主分支更新
git checkout main
git pull

# 2. 回到分支並重新建置
git checkout feature-branch
git rebase main

# 3. 移除衝突的 Migration（保留最新的）
git rm Migrations/20250710_Conflicting_*

# 4. 重新產生 Migration
task ef-migration-add MIGRATION_NAME=ConsolidatedChanges
```

### 問題 3：資料庫與程式碼不同步

**症狀**：Model Snapshot 與實際資料庫結構不符
**解決方案**：
```bash
# 移除所有本機 Migration
rm -r Migrations

# 重新掃描現有資料庫
task ef-codegen

# 驗證
dotnet ef migrations list
```

### 問題 4：回滾後無法前進

**症狀**：回滾後重新套用 Migration 失敗
**解決方案**：
```bash
# 檢查 __EFMigrationsHistory
SELECT * FROM __EFMigrationsHistory;

# 如果需要手動清理
DELETE FROM __EFMigrationsHistory 
WHERE MigrationId LIKE '20250710%';
```

## 最佳實踐檢查清單

- [ ] 使用 Code First 和 Migration 進行版本控制
- [ ] 每個 Migration 名稱應清楚描述用途
- [ ] 大表變更使用漸進策略（NULL → 填充 → NOT NULL）
- [ ] 索引變更使用 ONLINE 模式
- [ ] 回滾邏輯始終包含在 Down() 方法中
- [ ] 線上環境 Migration 需要 DBA 審批
- [ ] 定期測試回滾流程
- [ ] 保持 Migration 檔案在版本控制中
