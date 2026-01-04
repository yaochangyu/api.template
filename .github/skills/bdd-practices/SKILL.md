---
name: bdd-practices
description: Cucumber/Gherkin BDD 最佳實踐指導技能，提供 Gherkin 撰寫規範、情境設計原則、Discovery Workshop 引導與常見反模式識別，協助團隊撰寫高品質的行為驅動開發規格。
---

# Cucumber BDD Best Practices Skill

## 描述
Cucumber/Gherkin BDD 最佳實踐指導技能，整合 Cucumber 官方文件與業界專家經驗，提供完整的 BDD 實踐指引，包含 Discovery、Formulation、Automation 三階段最佳實踐，協助團隊撰寫易讀、易維護的行為規格。

## 職責
- **Discovery Workshop 引導**：協助團隊進行探索式工作坊
- **Gherkin 撰寫審查**：檢視並改善 Gherkin 情境品質
- **BDD 反模式識別**：識別並修正常見的 BDD 錯誤
- **測試策略諮詢**：提供宣告式 vs 命令式測試建議

## BDD 三大核心實踐

### 1. Discovery（探索）- 它「可以」做什麼
> "軟體系統建構中最困難的部分是精確決定要建構什麼。" - Fred Brooks

**目的**：透過結構化對話建立共識，減少需求誤解

**工作坊方法**：
- **Example Mapping**：使用四色索引卡映射規則與範例
- **OOPSI Mapping**：映射成果、輸出、流程、情境、輸入
- **Feature Mapping**：識別角色、分解任務、映射範例

**工作坊原則**：
- 時機：開發前盡可能晚的時間點（避免細節遺失）
- 參與者：Three Amigos（Product Owner、Developer、Tester）最少 3-6 人
- 時長：每個 User Story 25-30 分鐘
- 超時處理：Story 太大需拆分，或缺少細節需研究

**產出**：
- 共識的使用者範例
- 識別的規則與約束
- 發現的知識缺口
- 延遲實作的低優先級功能

### 2. Formulation（表述）- 它「應該」做什麼

**目的**：將範例結構化為可執行文件，建立共同語言

**撰寫原則**：
```gherkin
# ✅ 良好範例 - 宣告式（Declarative）
Feature: 訂閱者根據訂閱等級看到不同文章

  Scenario: 免費訂閱者只能看到免費文章
    Given Free Frieda 擁有免費訂閱
    When Free Frieda 使用有效憑證登入
    Then 她看到一篇免費文章

# ❌ 不良範例 - 命令式（Imperative）
Feature: 訂閱者根據訂閱等級看到不同文章

  Scenario: 免費訂閱者只能看到免費文章
    Given 使用者在登入頁面
    When 我在 Email 欄位輸入 "free@example.com"
    And 我在密碼欄位輸入 "password123"
    And 我按下「送出」按鈕
    Then 我在首頁看到 "FreeArticle1"
```

**關鍵差異**：
- **宣告式**：描述「做什麼」（行為意圖）
- **命令式**：描述「怎麼做」（實作細節）

### 3. Automation（自動化）- 它「實際上」做什麼

**目的**：用自動化範例引導開發，建立安全網

**實踐方式**：
1. 一次取一個範例
2. 連接到系統作為測試（測試失敗）
3. 開發實作程式碼（使用低階範例引導）
4. 測試通過，重複下一個範例

## Gherkin 撰寫黃金法則

### 核心守則
> **把讀者當作你希望被對待的方式。撰寫 Gherkin 時，讓不了解功能的人能夠理解它。**

### 基數法則（Cardinal Rule）
> **一個情境，一個行為！**

### 步驟規則

#### 1. 步驟必須依序出現
```gherkin
# ❌ 錯誤：步驟順序混亂
Scenario: 錯誤的步驟順序
  Given 初始狀態
  When 執行動作
  Then 驗證結果
  When 再執行動作    # ❌ When 不能跟在 Then 後面
  Then 再驗證結果

# ✅ 正確：拆分成兩個情境
Scenario: 第一個行為
  Given 初始狀態
  When 執行第一個動作
  Then 驗證第一個結果

Scenario: 第二個行為
  Given 初始狀態
  When 執行第二個動作
  Then 驗證第二個結果
```

#### 2. 步驟類型的正確用途
- **Given**：建立初始狀態（描述場景）
- **When**：執行動作（觸發行為）
- **Then**：驗證結果（可觀察的輸出）
- **And/But**：連接相同類型的步驟

#### 3. 時態與語態
```gherkin
# ✅ 正確：一律使用現在式 + 第三人稱
Given Google 首頁已顯示
When 使用者在搜尋列輸入 "panda"
Then 顯示與 "panda" 相關的連結

# ❌ 錯誤：混合時態和人稱
Given 使用者導航到 Google 首頁        # ❌ 暗示動作，不是狀態
When 使用者輸入了 "panda"             # ❌ 過去式
Then 將會顯示 "panda" 相關連結        # ❌ 未來式
```

#### 4. 步驟結構：主詞 + 述詞
```gherkin
# ✅ 正確：完整的主詞述詞結構
Given 使用者導航到 Google 首頁
When 使用者在搜尋列輸入 "panda"
Then 結果頁面顯示與 "panda" 相關的連結
And 結果頁面顯示 "panda" 的圖片連結
And 結果頁面顯示 "panda" 的影片連結

# ❌ 錯誤：缺少主詞或述詞
Given 使用者導航到 Google 首頁
When 使用者在搜尋列輸入 "panda"
Then 結果頁面顯示與 "panda" 相關的連結
And "panda" 的圖片連結           # ❌ 缺少主詞和述詞
And "panda" 的影片連結           # ❌ 無法複用
```

## 常見反模式與修正

### 反模式 1：程序驅動測試
```gherkin
# ❌ 錯誤：將傳統測試步驟套用 BDD 關鍵字
Feature: Google 搜尋

  Scenario: Google 圖片搜尋顯示圖片
    Given 使用者開啟網頁瀏覽器
    And 使用者導航到 "https://www.google.com/"
    When 使用者在搜尋列輸入 "panda"
    Then 結果頁面顯示與 "panda" 相關的連結
    When 使用者點擊頂部的「圖片」連結        # ❌ 出現第二個 When-Then
    Then 結果頁面顯示與 "panda" 相關的圖片

# ✅ 正確：每個情境一個行為
Feature: Google 搜尋

  Scenario: 從搜尋列搜尋
    Given 網頁瀏覽器位於 Google 首頁
    When 使用者在搜尋列輸入 "panda"
    Then 顯示與 "panda" 相關的連結

  Scenario: 圖片搜尋
    Given 顯示 "panda" 的 Google 搜尋結果
    When 使用者點擊頂部的「圖片」連結
    Then 顯示與 "panda" 相關的圖片
```

### 反模式 2：過度命令式
```gherkin
# ❌ 錯誤：過度描述實作細節
Scenario: 使用者登入
  Given 我訪問 "/login"
  When 我在 "使用者名稱" 欄位輸入 "Bob"
  And 我在 "密碼" 欄位輸入 "tester"
  And 我按下 "登入" 按鈕
  Then 我應該看到 "歡迎" 頁面

# ✅ 正確：宣告式描述行為
Scenario: 使用者登入
  Given Bob 是註冊使用者
  When Bob 使用有效憑證登入
  Then Bob 看到歡迎頁面
```

### 反模式 3：誤用 Scenario Outline
```gherkin
# ❌ 錯誤：過多不必要的變化
Scenario Outline: 搜尋
  Given 使用者在搜尋頁面
  When 使用者搜尋 "<query>"
  Then 顯示與 "<query>" 相關的結果
  
  Examples:
    | query     |
    | panda     |
    | elephant  |  # ❌ 未增加測試價值
    | tiger     |  # ❌ 等價類重複
    | lion      |  # ❌ 浪費執行時間

# ✅ 正確：聚焦於有意義的變化
Scenario Outline: 不同訂閱等級的存取權限
  Given <user> 擁有 <subscription> 訂閱
  When <user> 登入
  Then <user> 可存取 <accessible> 文章
  
  Examples:
    | user  | subscription | accessible    |
    | Free  | 免費         | 免費文章      |
    | Basic | 基本付費     | 免費和付費文章 |
    | Pro   | 專業付費     | 所有文章      |
```

### 反模式 4：測試資料硬編碼
```gherkin
# ❌ 錯誤：硬編碼可能變更的資料
Scenario: Google 搜尋建議
  When 使用者搜尋 "panda"
  Then 顯示以下相關結果
    | 相關搜尋      |
    | Panda Express |  # ❌ 如果企業倒閉會失敗
    | 大貓熊        |
    | panda 影片    |

# ✅ 正確：防禦性驗證
Scenario: Google 搜尋建議
  When 使用者搜尋 "panda"
  Then 顯示與 "panda" 相關的連結
  And 每個結果包含 "panda" 關鍵字
```

## 描述行為，非實作

### 核心概念
**功能需求屬於特性，程序屬於實作細節**

```gherkin
# ✅ 功能需求（描述「做什麼」）
When Bob 登入

# ❌ 程序參考（描述「怎麼做」）
Given 我訪問 "/login"
When 我在 "user name" 欄位輸入 "Bob"
And 我在 "password" 欄位輸入 "tester"
And 我按下 "login" 按鈕
Then 我應該看到 "welcome" 頁面
```

### 驗證方式
問自己：「如果實作改變，這個措辭需要改變嗎？」
- 答案為「是」→ 重新撰寫，避免實作細節
- 答案為「否」→ 符合行為導向

## 步驟長度建議

### 理想長度
- 建議步驟數：**3-5 步**
- 最大步驟數：**單位數（<10 步）**

### 縮減技巧

#### 1. 宣告式取代命令式
```gherkin
# 命令式 - 8 步
When 使用者將滑鼠捲動到搜尋列
And 使用者點擊搜尋列
And 使用者輸入字母 "p"
And 使用者輸入字母 "a"
And 使用者輸入字母 "n"
And 使用者輸入字母 "d"
And 使用者輸入字母 "a"
And 使用者按下 ENTER 鍵

# 宣告式 - 1 步
When 使用者在搜尋列輸入 "panda"
```

#### 2. 隱藏實作細節
```gherkin
# ❌ 暴露所有細節
Given 使用者擁有 Email "user@example.com"
And 使用者擁有姓名 "張三"
And 使用者擁有電話 "0912345678"
When 使用者註冊

# ✅ 隱藏在步驟定義中
Given 張三是新使用者
When 張三使用有效資料註冊
```

## Scenario Outline 檢查清單

使用 Scenario Outline 時，問自己以下問題：

### 1. 等價類檢查
- ❓ 每一行代表不同的等價類別嗎？
- ❌ 搜尋 "elephant" 額外於 "panda" 未增加測試價值

### 2. 組合必要性
- ❓ 需要涵蓋所有輸入組合嗎？
- ⚠️ N 欄位各 M 個輸入 = M^N 種組合
- ✅ 考慮每個輸入只出現一次，不考慮組合

### 3. 行為分離度
- ❓ 是否有欄位代表不同行為？
- 🔍 如果欄位從未在同一步驟中一起引用
- ✅ 考慮按欄位拆分 Scenario Outline

### 4. 資料透明度
- ❓ 讀者需要明確知道所有資料嗎？
- ✅ 考慮在步驟定義中隱藏部分資料
- ✅ 部分資料可從其他資料衍生

## 情境標題撰寫準則

### 良好標題特性
- **簡潔**：一行描述行為
- **清晰**：即使不了解功能的人也能理解
- **面向使用者**：描述使用者價值
- **可記錄**：框架會記錄標題

### 範例
```gherkin
# ✅ 良好標題
Scenario: 免費會員只能看到免費內容
Scenario: 付費會員可存取進階功能
Scenario: 搜尋結果依相關性排序

# ❌ 不良標題
Scenario: 測試 1
Scenario: 檢查權限
Scenario: 驗證 API 端點回應
```

## 處理已知的未知

### 原則
- 防禦性撰寫情境，避免底層資料變更導致失敗
- 將資料視為行為範例，非測試資料

### 技巧

#### 1. 智慧驗證取代硬編碼
```gherkin
# ❌ 硬編碼驗證
Then 結果包含 "Panda Express"

# ✅ 模式驗證
Then 每個結果與搜尋詞 "panda" 相關
```

#### 2. 步驟定義隱藏資料
```gherkin
Scenario: 搜尋結果連結
  Given 顯示 "panda" 的 Google 搜尋結果
  When 使用者點擊第一個結果連結           # 未明確命名連結值
  Then 顯示所選結果連結的頁面             # 步驟定義傳遞連結資料
```

## 風格與結構

### 一致性原則
- **第三人稱**：一律使用第三人稱視角
- **現在式**：一律使用現在式
- **主詞述詞**：完整的句子結構

### 範例
```gherkin
# ✅ 一致的風格
Feature: 使用者認證

  Scenario: 成功登入
    Given 使用者擁有有效帳號
    When 使用者輸入正確憑證
    Then 使用者看到儀表板

# ❌ 不一致的風格
Feature: 使用者認證

  Scenario: 成功登入
    Given 我有一個帳號              # ❌ 第一人稱
    When 使用者輸入了憑證          # ❌ 過去式
    Then 將會顯示儀表板            # ❌ 未來式
```

## 互動式引導流程

當使用者尋求協助時，依循以下流程：

### 1. Discovery 階段協助
```
Q1: 這個 User Story 的主要使用者是誰？
Q2: 使用者想達成什麼目的？
Q3: 有哪些規則或約束？
Q4: 你能舉一個具體的例子嗎？
Q5: 有沒有邊界情況或例外？
```

### 2. Formulation 階段審查
```
檢查清單：
□ 情境是否描述行為而非實作？
□ 是否使用宣告式而非命令式？
□ 每個情境是否只涵蓋一個行為？
□ 步驟是否依照 Given-When-Then 順序？
□ 是否使用第三人稱現在式？
□ 標題是否清晰簡潔？
```

### 3. 反模式偵測
```
掃描以下反模式：
- [ ] 多個 When-Then 配對
- [ ] UI 實作細節（按鈕、欄位、URL）
- [ ] 硬編碼可能變更的資料
- [ ] 過長的情境（>10 步）
- [ ] 過度使用 Scenario Outline
```

## 提供建議時的格式

### 修正建議範本
```markdown
### 🔴 發現問題
[描述問題]

### ❌ 原始版本
```gherkin
[原始 Gherkin]
```

### 💡 問題分析
[解釋為什麼這是問題]

### ✅ 建議修正
```gherkin
[修正後 Gherkin]
```

### 📝 修正說明
[解釋為什麼這樣更好]
```

## 參考資源

### 官方文件
- [Cucumber BDD 文件](https://cucumber.io/docs/bdd/)
- [Gherkin 參考](https://cucumber.io/docs/gherkin/reference/)
- [Better Gherkin](https://cucumber.io/docs/bdd/better-gherkin/)
- [Discovery Workshop](https://cucumber.io/docs/bdd/discovery-workshop/)

### 最佳實踐文章
- [Automation Panda - BDD 101: Writing Good Gherkin](https://automationpanda.com/2017/01/30/bdd-101-writing-good-gherkin/)
- [Should Gherkin Steps Use First-Person or Third-Person?](https://automationpanda.com/2017/01/18/should-gherkin-steps-use-first-person-or-third-person/)
- [Good Gherkin Scenario Titles](https://automationpanda.com/2018/01/31/good-gherkin-scenario-titles/)

## 使用範例

### 請求範例 1：審查情境
```
使用者：請審查這個 Gherkin 情境：
Scenario: 使用者登入
  When 我訪問登入頁面
  And 我輸入帳號密碼
  Then 我看到首頁

助手：[使用修正建議範本提供回饋]
```

### 請求範例 2：引導 Discovery
```
使用者：我們要開發「購物車結帳」功能，但不確定如何開始

助手：[使用 Discovery 階段協助流程引導]
```

### 請求範例 3：轉換傳統測試
```
使用者：如何將這個傳統測試轉換為 BDD 情境？
[傳統測試步驟]

助手：[識別行為、拆分情境、轉換為宣告式 Gherkin]
```

## 注意事項

- 永遠優先考慮**可讀性**而非簡潔性
- 記住受眾包含**非技術人員**
- Gherkin 是**溝通工具**，不只是測試
- 保持**行為驅動思維**，避免程序驅動
- 持續**重構**情境，如同重構程式碼
