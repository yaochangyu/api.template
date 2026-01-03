# 架構決策 Skill (`architecture`)

## 職責
此 Skill 專門用於協助使用者在開發初期、新功能規劃或現有專案重構時，進行重要的架構決策。它涵蓋專案結構組織方式、核心開發原則及 Repository Pattern 的選擇與應用。

## 核心流程
本 Skill 參考 `CLAUDE.md` 中的「架構概述」與「專案最佳實踐」章節。

### 1. 專案結構組織方式選擇 (強制詢問)
本 Skill 會引導使用者選擇適合的專案組織方式：
- **方案 A：單一專案結構**: 適合小型團隊、快速開發。
- **方案 B：多專案結構**: 適合大型團隊、明確分工、長期維護。
- 參考 `references/architecture-guide.md` 中的「組織方式」詳細說明。

### 2. Repository Pattern 設計決策 (必要時觸發)
當涉及到資料存取層的設計時，本 Skill 會根據需求提供 Repository Pattern 的決策協助：
- 引導使用者評估業務需求，判斷應採用「簡單資料表導向」或「業務需求導向」的 Repository 設計。
- 強調參考 `references/repository-pattern-guide.md` 中的「設計決策檢查清單」來做出明智選擇。

### 3. 其他架構考量
- 根據使用者需求，提供核心開發原則的指引，例如不可變物件設計、依賴注入最佳實踐等。
- 參考 `references/architecture-guide.md` 中的「技術堆疊」與「程式碼產生工作流程」。

## 引用文件
- `references/interaction-rules.md`: 用於所有使用者互動的規範。
- `references/architecture-guide.md`: 提供分層架構、技術堆疊和專案組織方式的詳細說明。
- `references/repository-pattern-guide.md`: 詳細說明 Repository Pattern 的設計哲學、實務範例與決策清單。
- `references/error-handling-guide.md`: 提供錯誤處理原則，與架構設計相關。
- `references/trace-context-guide.md`: 提供追蹤內容管理原則，與架構設計相關。

## 輸出
- 協助使用者做出明確的架構決策。
- 提供相關的最佳實踐與指引，影響程式碼生成和實作的方向。

## 範例互動流程 (Agent 調用此 Skill 時)
1. **Agent**: "您好！在進行功能實作前，我們需要確認專案的架構選擇。請問您希望採用哪種專案組織方式？\n1) 單一專案結構 (適合小型團隊、快速開發)\n2) 多專案結構 (適合大型團隊、明確分工、長期維護)"
2. **使用者**: 選擇 "1)。"
3. **Agent**: "好的，選擇單一專案結構。當前功能實作會涉及資料庫操作，請問您對 Repository 的設計有何考量？\n1) 簡單資料表導向 (僅單一資料表的簡單 CRUD)\n2) 業務需求導向 (涉及多資料表、複雜業務邏輯)\n3) 混合模式 (視情況彈性調整)"
4. **使用者**: 選擇 "3)。"
5. **Agent**: "本專案預設採用混合模式，將核心業務使用需求導向，簡單主檔使用資料表導向。您是否同意此策略？"
6. ... (後續架構相關的討論與決策)
