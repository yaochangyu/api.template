# 實作路線圖（Roadmap）

**最後更新**: 2026-07-10 18:30 GMT+8

---

## 📋 所有計畫書清單

### ✅ 已完成的計畫書

| 序號 | 計畫書 | 狀態 | Commit | 說明 |
|------|--------|------|--------|------|
| P0 | CLAUDE-refactoring-PLAN.completed.md | ✅ 完成 | 9d5ace9 | CLAUDE.md 精簡 + caching-strategy 新建 |
| P1 | P1-skill-enhancement.plan.md | ✅ 完成 | 3f28ac0 | error-handling + middleware 補強 |
| 舊 | enable-member-v1.plan.md | ✅ 完成 | - | 會員 V1 功能 |
| 舊 | fix-integration-failures.plan.md | ✅ 完成 | - | 整合測試失敗修復 |
| 舊 | fix-testcontainers-cleanup.plan.md | ✅ 完成 | - | Testcontainers 清理 |
| 舊 | fix-test-environment.plan.md | ✅ 完成 | - | 測試環境修復 |
| 舊 | rename-member-v2-controller.plan.md | ✅ 完成 | - | Controller 改名 |
| 舊 | health-check/plan.md | ✅ 完成 | 323bdbc | 健檢修復 |

### 🔄 進行中的計畫書
（無）

### ⏳ 待實作的計畫書（按優先級）

| 優先級 | 計畫書名稱 | 預估工時 | 目標 | 相關 SKILL |
|--------|----------|--------|------|-----------|
| **P2.1** | `SKILL-optimization-P2.1-bdd-testing.plan.md` | 1.5h | 補強 `/bdd-testing` | /bdd-testing |
| **P2.2** | `SKILL-optimization-P2.2-repository-design.plan.md` | 1.5h | 補強 `/repository-design` | /repository-design |
| **P3.1** | `SKILL-optimization-P3.1-ef-core.plan.md` | 2h | 補強 `/ef-core` | /ef-core |

---

## 🎯 P2.1：補強 `/bdd-testing` SKILL

**目標**: Docker 容器配置 + Testcontainers 使用 + .feature 範例

**完成條件**:
- [ ] 新增 `references/docker-testcontainers-setup.md`
- [ ] 新增 `references/bdd-gherkin-examples.md`
- [ ] 新增 `assets/test-setup-template.cs`
- [ ] 新增 `assets/sample.feature`
- [ ] Git commit + 計畫書封存

**預估工時**: 1.5 小時

---

## 🎯 P2.2：補強 `/repository-design` SKILL

**目標**: 設計決策指南 + 實作範本（兩種模式）

**完成條件**:
- [ ] 新增 `references/repository-design-checklist.md`
- [ ] 新增 `references/pattern-comparison.md`
- [ ] 新增 `assets/repository-template-tabledriven.cs`
- [ ] 新增 `assets/repository-template-domaindriven.cs`
- [ ] Git commit + 計畫書封存

**預估工時**: 1.5 小時

---

## 🎯 P3.1：補強 `/ef-core` SKILL

**目標**: 查詢最佳化 + Migration 策略 + DbContextFactory 範本

**完成條件**:
- [ ] 新增 `references/query-optimization.md`
- [ ] 新增 `references/migration-strategy.md`
- [ ] 新增 `assets/dbcontext-factory-template.cs`
- [ ] Git commit + 計畫書封存

**預估工時**: 2 小時

---

## 📊 優先級說明

### 🔴 P2（立即執行）— 測試和架構最關鍵
- **P2.1** `/bdd-testing` — 測試是核心，直接支持新功能開發
- **P2.2** `/repository-design` — 架構決策指南，避免設計錯誤

### 🟡 P3（後續執行）— 深度優化
- **P3.1** `/ef-core` — 效能優化指南

---

## 執行順序

**建議執行順序**：
```
1. P2.1: bdd-testing (1.5h)
   ↓
2. P2.2: repository-design (1.5h)
   ↓
3. P3.1: ef-core (2h) [可選]
```

**總工時**: P2 = 3h（必做）, P3 = 2h（可選）

---

## 📈 完成狀態追蹤

| 計畫 | 狀態 | 進度 |
|------|------|------|
| P0 (CLAUDE.md 精簡) | ✅ 完成 | 100% |
| P1 (error-handling + middleware) | ✅ 完成 | 100% |
| P2.1 (bdd-testing) | ✅ 完成 | 100% |
| P2.2 (repository-design) | ✅ 完成 | 100% |
| P3.1 (ef-core) | ✅ 完成 | 100% |

---

**下一步**: 開始 P2.1 計畫書實作
