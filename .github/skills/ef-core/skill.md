---
name: ef-core
description: EF Core 操作與最佳化技能，協助開發者正確使用 Entity Framework Core，包含 DbContextFactory 模式、查詢最佳化、Migration 管理與反向工程。
---

# EF Core Skill

## 描述
EF Core 操作與最佳化技能，協助開發者正確使用 Entity Framework Core，包含 DbContextFactory 模式、查詢最佳化、Migration 管理等。

## 職責
- DbContextFactory 使用指導
- 查詢最佳化（AsNoTracking、Include）
- Migration 管理
- 反向工程（Database First）

## 核心原則

### 1. DbContextFactory 模式
```csharp
// ✅ 正確：使用 Factory 模式
public class Repository(IDbContextFactory<JobBankDbContext> dbContextFactory)
{
    public async Task<Result> OperationAsync(CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        // 使用 dbContext
    }
}

// ❌ 錯誤：直接注入 DbContext
public class Repository(JobBankDbContext dbContext) // 生命週期問題
```

### 2. 查詢最佳化
- **AsNoTracking**：唯讀查詢使用
- **Include/ThenInclude**：避免 N+1 查詢
- **Select 投影**：只取需要的欄位
- **分頁查詢**：大量資料使用 Skip/Take

### 3. Migration 管理
```bash
# 建立 Migration
task ef-migration-add NAME=AddMemberTable

# 更新資料庫
task ef-database-update

# 反向工程
task ef-codegen
```

### 4. 交易管理
```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
try
{
    // 操作
    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

## 參考文件
- [EF Core 最佳實踐](./references/ef-core-best-practices.md)

## 範本檔案
- [DbContext 使用範本](./assets/dbcontext-usage-template.cs)
