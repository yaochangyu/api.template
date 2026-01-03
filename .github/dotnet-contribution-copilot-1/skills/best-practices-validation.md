# Skill: Best Practices Validation

> 最佳實踐驗證 Skill - 驗證程式碼是否符合專案規範

## Skill 職責

- 驗證 Repository Pattern 設計
- 檢查命名規範
- 驗證非同步程式設計
- 檢查快取策略使用
- 驗證錯誤處理模式

## 引用文件

- [Repository Pattern 指南](../references/repository-pattern.md)
- [命名規範](../references/naming-conventions.md)
- [快取策略](../references/caching-strategy.md)
- [錯誤處理](../references/error-handling.md)

## 驗證檢查清單

### 1. Repository Pattern 驗證

```markdown
✅ 檢查項目：

Repository 設計：
- [ ] 命名符合規範（{Feature}Repository 或 {BusinessDomain}Repository）
- [ ] 使用 IDbContextFactory 而非直接注入 DbContext
- [ ] 使用 await using 確保資源釋放
- [ ] 唯讀查詢使用 AsNoTracking()
- [ ] 回傳 Result<T, Failure>

設計策略選擇：
- [ ] 簡單 CRUD 使用資料表導向 ✅
- [ ] 複雜業務使用需求導向 ✅
- [ ] 適當使用混合模式
```

### 2. 命名規範檢查

```markdown
檔案命名：
- [ ] Controller: {Feature}Controller.cs
- [ ] Handler: {Feature}Handler.cs
- [ ] Repository: {Feature}Repository.cs

C# 命名：
- [ ] 類別/方法：PascalCase
- [ ] 參數/變數：camelCase
- [ ] 非同步方法：Async 後綴
- [ ] 私有欄位：_camelCase（如果使用）
```

### 3. 非同步程式設計檢查

```markdown
- [ ] 所有 I/O 操作使用 async/await
- [ ] 傳遞 CancellationToken
- [ ] 避免使用 .Result 或 .Wait()
- [ ] 使用 ConfigureAwait(false)（Library 專案）
```

### 4. 錯誤處理驗證

```markdown
- [ ] Handler/Repository 回傳 Result<T, Failure>
- [ ] 不拋出業務邏輯例外
- [ ] 保存原始例外到 Failure.Exception
- [ ] 使用 nameof(FailureCode.*)
- [ ] Controller 使用 FailureCodeMapper 轉換狀態碼
- [ ] 業務邏輯層不記錄錯誤日誌
```

### 5. 快取策略檢查

```markdown
- [ ] 快取鍵使用命名規範：{feature}:{operation}:{parameters}
- [ ] 定義 CacheKeys 靜態類別
- [ ] 設定合理的 TTL
- [ ] 資料異動時清除快取
```

## 驗證流程

### 自動檢查

使用 grep 搜尋常見問題：

```powershell
# 檢查是否直接注入 DbContext（應使用 IDbContextFactory）
grep -r "DbContext dbContext" --include="*Repository.cs"

# 檢查是否使用 .Result（應避免）
grep -r "\.Result" --include="*.cs"

# 檢查是否缺少 CancellationToken
grep -r "async Task" --include="*.cs" | grep -v "CancellationToken"
```

### 手動審查

引導使用者：
1. 閱讀相關最佳實踐文件
2. 對照檢查清單逐項驗證
3. 修正不符合規範的程式碼

## 輸出成果

- ✅ 所有檢查項目通過
- ✅ 程式碼符合專案規範
- ✅ 提供改進建議（如有需要）

---

**Skill 類型**：程式碼審查  
**執行時機**：實作完成後  
**適用 Agents**：dotnet-api-developer
