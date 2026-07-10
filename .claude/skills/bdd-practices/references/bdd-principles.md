# BDD 基礎原則與開發循環

## BDD 定義與價值主張

行為驅動開發（BDD, Behavior-Driven Development）是透過定義系統應該「做什麼」而非「怎麼做」來引導軟體開發。BDD 將需求轉化為可執行的規格，建立開發者、測試者與產品擁有者之間的共識。

**核心價值**：
- ✅ 減少需求誤解，建立共同語言
- ✅ 測試與文件同步，消除文件與實作脫節
- ✅ 提升測試品質，轉向行為導向而非流程導向
- ✅ 持續交付信心，自動化驗證業務行為

## BDD 三大核心實踐

### 1. Discovery（探索）- 它「可以」做什麼

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

**宣告式 vs 命令式範例**：
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

### 基數法則

> **一個情境，一個行為！**

每個 Scenario 應專注於單一行為。如需測試多個行為，應分拆成多個情境，避免多重 When-Then 配對。

## 步驟撰寫規則

### 1. 步驟必須依序出現

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

### 2. 步驟類型的正確用途

- **Given**：建立初始狀態（描述場景）
- **When**：執行動作（觸發行為）
- **Then**：驗證結果（可觀察的輸出）
- **And/But**：連接相同類型的步驟

### 3. 時態與語態

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

### 4. 步驟結構：主詞 + 述詞

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

## Scenario Outline 最佳實踐

### 何時使用 Scenario Outline

使用 Scenario Outline 時，問自己以下問題：

#### 1. 等價類檢查
- ❓ 每一行代表不同的等價類別嗎？
- ❌ 搜尋 "elephant" 額外於 "panda" 未增加測試價值

#### 2. 組合必要性
- ❓ 需要涵蓋所有輸入組合嗎？
- ⚠️ N 欄位各 M 個輸入 = M^N 種組合
- ✅ 考慮每個輸入只出現一次，不考慮組合

#### 3. 行為分離度
- ❓ 是否有欄位代表不同行為？
- 🔍 如果欄位從未在同一步驟中一起引用
- ✅ 考慮按欄位拆分 Scenario Outline

#### 4. 資料透明度
- ❓ 讀者需要明確知道所有資料嗎？
- ✅ 考慮在步驟定義中隱藏部分資料
- ✅ 部分資料可從其他資料衍生

**有意義的使用範例**：
```gherkin
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

## 情境標題撰寫準則

### 良好標題特性

- **簡潔**：一行描述行為
- **清晰**：即使不了解功能的人也能理解
- **面向使用者**：描述使用者價值
- **可記錄**：框架會記錄標題

### 範例對比

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

## BDD vs 傳統測試對比

| 面向 | BDD | 傳統測試 |
|------|-----|--------|
| **焦點** | 行為意圖 | 實作細節 |
| **文件** | Gherkin 即文件 | 分離的測試與文件 |
| **維護** | 易改易讀 | 易碎、易失效 |
| **利害關係人** | 包含非技術人員 | 技術人員為主 |
| **溝通** | 共同語言 | 單向文件 |

## 常見反模式與修正

### 反模式 1：程序驅動測試

```gherkin
# ❌ 錯誤：將傳統測試步驟套用 BDD 關鍵字
Scenario: Google 圖片搜尋顯示圖片
  Given 使用者開啟網頁瀏覽器
  And 使用者導航到 "https://www.google.com/"
  When 使用者在搜尋列輸入 "panda"
  Then 結果頁面顯示與 "panda" 相關的連結
  When 使用者點擊頂部的「圖片」連結        # ❌ 出現第二個 When-Then
  Then 結果頁面顯示與 "panda" 相關的圖片

# ✅ 正確：每個情境一個行為
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

### 反模式 3：硬編碼可能變更的資料

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

## 步驟長度建議

### 理想長度

- **建議步驟數**：3-5 步
- **最大步驟數**：單位數（< 10 步）

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

## BDD 最佳實踐

### 互動式引導流程

當進行 Discovery 階段時，依循以下流程：

```
Q1: 這個 User Story 的主要使用者是誰？
Q2: 使用者想達成什麼目的？
Q3: 有哪些規則或約束？
Q4: 你能舉一個具體的例子嗎？
Q5: 有沒有邊界情況或例外？
```

### Formulation 階段審查清單

```
□ 情境是否描述行為而非實作？
□ 是否使用宣告式而非命令式？
□ 每個情境是否只涵蓋一個行為？
□ 步驟是否依照 Given-When-Then 順序？
□ 是否使用第三人稱現在式？
□ 標題是否清晰簡潔？
```

### 反模式偵測

掃描以下反模式：
- [ ] 多個 When-Then 配對
- [ ] UI 實作細節（按鈕、欄位、URL）
- [ ] 硬編碼可能變更的資料
- [ ] 過長的情境（> 10 步）
- [ ] 過度使用 Scenario Outline

## 重要原則

- 永遠優先考慮**可讀性**而非簡潔性
- 記住受眾包含**非技術人員**
- Gherkin 是**溝通工具**，不只是測試
- 保持**行為驅動思維**，避免程序驅動
- 持續**重構**情境，如同重構程式碼
