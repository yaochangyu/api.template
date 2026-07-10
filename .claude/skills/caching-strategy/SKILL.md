---
name: caching-strategy
description: 快取策略與多層快取設計技能，協助開發者實作 L1 Memory + L2 Redis 分層快取，包含 TTL 管理、版本控制與失效策略。
---

# Caching Strategy Skill

## 描述
多層快取設計與實作技能，協助開發者設計高效快取架構，支援記憶體快取（L1）與 Redis（L2）分層備援。

## 職責
- 多層快取架構設計與選擇
- Redis 配置與連線管理
- 快取 TTL 與版本控制
- 快取失效策略決策
- 效能度量與監控

## 核心概念

### 多層快取架構

#### L1 快取 (記憶體內快取 - IMemoryCache)
- **特點**: 超快速存取（毫秒級）、進程內、數據不共享
- **適用場景**: 頻繁存取的小型數據（< 1MB）、應用初始化數據
- **缺點**: 單機限制、不支援分散式共用

#### L2 快取 (分散式快取 - Redis)
- **特點**: 速度快（毫秒級）、可跨實例共用、支援複雜結構
- **適用場景**: 中等大小數據、需要多實例共用的數據
- **缺點**: 網路延遲、依賴外部服務

#### 快取備援策略
```
讀取流程:
1. 檢查 L1 快取
   ├─ 命中 → 直接返回
   └─ 未命中 ↓
2. 檢查 L2 快取 (Redis)
   ├─ 命中 → 寫入 L1、返回
   └─ 未命中 ↓
3. 從資料庫讀取
   └─ 寫入 L1 + L2、返回
```

## 快取鍵命名規範

**格式**: `{feature}:{operation}:{parameters}` (冒號分隔)

**範例**:
```
members:page:0:10              # 會員列表分頁 0-10
member:email:test@example.com  # 特定郵件會員
order:detail:12345             # 訂單詳情 ID=12345
product:list:category:electronics  # 分類商品列表
```

## TTL (Time To Live) 管理

### 建議的 TTL 策略

| 資料類型 | TTL | 原因 |
|---------|-----|------|
| 會員資訊 | 30-60 分鐘 | 中等變更頻率 |
| 產品目錄 | 1-4 小時 | 低變更頻率 |
| 設定數據 | 24 小時 | 很少變更 |
| 會話數據 | 15-30 分鐘 | 高安全要求 |
| 使用者權限 | 5-10 分鐘 | 中等安全要求 |
| 訂單狀態 | 5 分鐘 | 實時性要求高 |

## 快取失效策略

### 1. 時間型失效 (TTL)
- **自動過期**: 依據 TTL 自動清除
- **適用**: 大多數場景
- **優點**: 實作簡單、無額外依賴
- **缺點**: 過期延遲、無法及時同步

### 2. 事件型失效 (主動清除)
**資料修改時立即清除相關快取**:
```csharp
// 例: 修改會員資訊時清除快取
public async Task UpdateMemberAsync(Member member)
{
    await memberRepo.UpdateAsync(member);
    await cache.RemoveAsync($"member:email:{member.Email}");
    await cache.RemoveAsync("members:list:*");  // 批次清除
}
```

### 3. 標籤快取 (Tag-Based Invalidation)
```csharp
// 使用 Redis tag 支援批次清除
await cache.SetAsync(
    key: "member:123",
    value: memberData,
    tags: new[] { "members", "user-123" });

// 清除所有會員相關快取
await cache.RemoveByTagAsync("members");
```

### 4. 版本控制
```csharp
// 版本前綴策略：快取鍵包含版本號
const string VERSION = "v2";
var cacheKey = $"{VERSION}:member:email:test@example.com";
// 版本更新時無需主動清除，新版本鍵與舊版本共存
// 可配置最舊版本自動過期
```

## 決策檢查清單

實作快取時，應檢查：

- [ ] **是否應該快取此數據？**
  - 讀取頻率是否足夠高？
  - 更新頻率是否足夠低？
  - 數據量是否不太大？
  
- [ ] **選擇快取層級**
  - [ ] L1 (IMemoryCache) — 單機高速存取
  - [ ] L1 + L2 (Redis) — 分散式共用
  
- [ ] **TTL 設定是否合理**
  - [ ] 是否設定過短（頻繁重新計算）
  - [ ] 是否設定過長（過期延遲）
  - [ ] 是否有特殊時間考量（營業時間、時區）
  
- [ ] **失效策略選擇**
  - [ ] TTL 自動過期（無額外代碼）
  - [ ] 事件型主動清除（實時性強）
  - [ ] 標籤快取（支援批次清除）
  - [ ] 版本控制（零停機更新）
  
- [ ] **快取雪崩防護**
  - [ ] 快取失效時是否會同時大量查詢資料庫？
  - [ ] 是否實現了快取預熱機制？
  - [ ] 是否有降級方案（快取不可用時）？

## 常見反模式（避免）

### ❌ 反模式 1: 快取所有數據
```csharp
// 不要這樣做 — 快取太多數據導致記憶體溢出
var users = await db.Users.ToListAsync();
await cache.SetAsync("all-users", users);  // ❌ 超大資料集
```

### ❌ 反模式 2: 無版本的長期 TTL
```csharp
// 不要這樣做 — 資料遺失風險高
await cache.SetAsync("config", data, TimeSpan.FromDays(30));  // ❌ 30 天无法更新
```

### ❌ 反模式 3: 快取機密信息
```csharp
// 不要這樣做 — 安全風險
await cache.SetAsync("user-password", pwd);  // ❌ 不應快取
await cache.SetAsync("api-token", token);    // ❌ 不應快取
```

## 參考文件
- [快取架構設計](./references/caching-architecture.md)
- [Redis 配置指南](./references/redis-configuration.md)
- [失效策略深解](./references/cache-invalidation-strategies.md)

## 實作範例

開發者應參考專案內的實際實作代碼：

**多層快取實作**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/JobBank1111.Infrastructure/Caching/MultiLayerCacheService.cs
```

**Redis 配置**：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  src/appsettings.json
```
