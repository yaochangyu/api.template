---
name: repository-design
description: Repository 設計指導技能，協助開發者根據業務需求選擇最合適的 Repository 設計策略（資料表導向 vs 需求導向 vs 混合模式）。
---

# Repository Design Skill

## 描述
Repository 設計指導技能，協助開發者根據業務需求選擇最合適的 Repository 設計策略（資料表導向 vs 需求導向）。

## 職責
- 分析業務需求複雜度
- 建議設計策略（資料表導向/需求導向/混合）
- 提供決策檢查清單
- 產生 Repository 程式碼範本

## 能力

### 1. 設計策略分析
- 評估業務邏輯複雜度
- 判斷是否需要跨表操作
- 識別交易需求

### 2. 互動式決策引導
透過結構化問答幫助選擇設計策略

### 3. 程式碼範本產生
- 資料表導向 Repository 範本
- 需求導向 Repository 範本

## 設計策略

### 策略 A：資料表導向（適合小型專案）
- 一個 Repository 對應一個資料表
- 簡單 CRUD 操作
- 適合：< 10 個資料表、1-3 人團隊

### 策略 B：需求導向（推薦中大型專案）
- 一個 Repository 封裝完整業務操作
- 跨表操作、交易管理
- 適合：> 10 個資料表、複雜業務邏輯

### 策略 C：混合模式（本專案採用）
- 簡單主檔用資料表導向
- 複雜業務用需求導向
- 靈活調整

## 決策檢查清單

**使用需求導向的判斷標準**：
- [ ] 涉及 3 個以上資料表？
- [ ] 需要交易一致性保證？
- [ ] 業務邏輯複雜，需要多步驟協調？
- [ ] 多個 API 端點共用此業務邏輯？
- [ ] 未來可能擴展更多相關功能？

**如果 2 個以上為「是」，建議使用需求導向**

## 參考文件
- [Repository 設計哲學](./references/repository-design-philosophy.md)
- [Repository Pattern 最佳實踐](./references/repository-pattern-best-practices.md) - 完整的 Repository Pattern 實作指南，包含 DDD、CQRS、常見反模式與實作範例

## 範本檔案
- [資料表導向範本](./assets/repository-table-oriented-template.cs)
- [需求導向範本](./assets/repository-domain-oriented-template.cs)
