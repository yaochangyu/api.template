# Repository Pattern 設計指引

本文件提供專案中 Repository Pattern 的設計哲學、策略選擇與實踐範例。

## 核心原則：以需求為導向，而非資料表

**❌ 錯誤的思維：資料表導向**
```
資料表: Members, Orders, OrderItems
Repository: MemberRepository, OrderRepository, OrderItemRepository
問題: 業務邏輯分散、跨表操作複雜、難以維護
```

**✅ 正確的思維：需求導向**
```
業務需求: 會員管理、訂單處理、庫存管理
Repository: MemberRepository, OrderManagementRepository, InventoryRepository
優點: 封裝完整業務邏輯、減少跨層呼叫、更易維護
```

## 設計策略選擇

**策略 A：簡單資料表導向（適合小型專案）**
- 專案規模小（< 10 個資料表）
- 業務邏輯簡單
- 團隊人數少（1-3 人）
- 快速開發優先
- **範例**: `MemberRepository` 對應 `Members` 資料表

**策略 B：業務需求導向（推薦用於中大型專案）**
- 專案規模中等以上（> 10 個資料表）
- 複雜業務邏輯
- 需要跨表操作
- 長期維護考量
- **範例**: `OrderManagementRepository` 處理訂單、訂單明細、付款等相關操作

**策略 C：混合模式（實務常見）**
- 核心業務使用需求導向（如訂單處理）
- 簡單主檔使用資料表導向（如會員、產品）
- 根據複雜度靈活調整
- **本專案採用此策略**

## 命名規範建議

**資料表導向命名**
- `{TableName}Repository` - 例如：`MemberRepository`, `ProductRepository`
- 適用於簡單 CRUD 操作

**需求導向命名**
- `{BusinessDomain}Repository` - 例如：`OrderManagementRepository`, `InventoryRepository`
- `{AggregateRoot}Repository` - 例如：`ShoppingCartRepository`, `UserAccountRepository`
- 適用於複雜業務邏輯

## 設計決策檢查清單

在設計 Repository 時，應詢問自己：

**✅ 需求導向的判斷標準**
- [ ] 此業務操作涉及 3 個以上資料表？
- [ ] 操作需要交易一致性保證？
- [ ] 業務邏輯複雜，需要多步驟協調？
- [ ] 多個 API 端點共用此業務邏輯？
- [ ] 未來可能擴展更多相關功能？

**如果以上有 2 個以上為「是」，建議使用需求導向 Repository**

**❌ 資料表導向的適用場景**
- [ ] 僅單一資料表的簡單 CRUD
- [ ] 無複雜業務邏輯
- [ ] 不需要跨表操作
- [ ] 查詢條件簡單明確

## 本專案的實作策略

本專案採用**混合模式**：
- **簡單主檔**：使用資料表導向（如 `MemberRepository`）
- **複雜業務**：視需求採用業務導向（如未來的訂單管理）
- **靈活調整**：根據實際需求演進

**重要原則**: 
- 設計初期可以從簡單的資料表導向開始
- 當發現業務邏輯分散、難以維護時，重構為需求導向
- 不要過度設計，根據實際複雜度調整
