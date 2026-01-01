# BDD Feature Template for API Template
# 使用 Gherkin 語法定義業務情境

Feature: {功能名稱}
  作為一個 {角色}
  我想要 {執行某操作}
  以便 {達成某目標}

  Background:
    Given 系統已啟動
    And 資料庫已初始化

  # 成功路徑（Happy Path）
  Scenario: {成功情境描述}
    Given {前置條件}
    And {額外前置條件}
    When {執行動作}
    Then {預期結果}
    And {額外驗證}

  # 使用 Scenario Outline 處理多組測試資料
  Scenario Outline: {參數化測試情境}
    Given 我準備資料
      | 欄位1   | 欄位2   |
      | <值1>  | <值2>  |
    When 我發送請求
    Then 回應狀態碼應為 <狀態碼>
    
    Examples:
      | 值1        | 值2      | 狀態碼 |
      | valid_val  | test     | 200    |
      | invalid    | test     | 400    |

  # 錯誤處理情境
  Scenario: {錯誤情境描述 - 輸入驗證}
    Given {前置條件}
    When {執行帶有無效資料的動作}
    Then 回應狀態碼應為 400
    And 錯誤訊息應為 "{預期錯誤訊息}"

  Scenario: {錯誤情境描述 - 資源不存在}
    Given 資料庫中不存在特定資源
    When 我嘗試存取該資源
    Then 回應狀態碼應為 404

  Scenario: {錯誤情境描述 - 衝突}
    Given 資料庫中已存在相同資源
    When 我嘗試建立重複資源
    Then 回應狀態碼應為 409

  # 實際範例：會員註冊
  @example @member
  Scenario: 成功註冊新會員
    Given 我準備註冊會員資料
      | Email              | Name   | Password  |
      | test@example.com   | 測試員 | Pass@123  |
    When 我發送註冊請求至 "/api/members"
    Then 回應狀態碼應為 201
    And 回應標頭 "Location" 應包含 "/api/members/"
    And 回應內容應包含
      | 欄位   | 值                |
      | Email  | test@example.com  |
      | Name   | 測試員            |
    And 資料庫中應存在 Email 為 "test@example.com" 的會員
    And Redis 快取中不應存在此會員資料

  @example @member @validation
  Scenario: 註冊失敗 - Email 格式錯誤
    Given 我準備註冊會員資料
      | Email        | Name   | Password  |
      | invalid-mail | 測試員 | Pass@123  |
    When 我發送註冊請求至 "/api/members"
    Then 回應狀態碼應為 400
    And 錯誤訊息應包含 "Email 格式不正確"

  @example @member @duplicate
  Scenario: 註冊失敗 - Email 已存在
    Given 資料庫中已存在會員
      | Email              | Name     |
      | test@example.com   | 既有會員 |
    And 我準備註冊會員資料
      | Email              | Name   | Password  |
      | test@example.com   | 重複員 | Pass@456  |
    When 我發送註冊請求至 "/api/members"
    Then 回應狀態碼應為 409
    And 錯誤訊息應為 "Email 已被使用"

  # 快取行為驗證
  @example @caching
  Scenario: 查詢會員 - 快取命中流程
    Given 資料庫中已存在會員
      | Id    | Email              | Name   |
      | m001  | test@example.com   | 測試員 |
    When 我發送 GET 請求至 "/api/members/m001"
    Then 回應狀態碼應為 200
    And Redis 快取應包含 key "member:m001"
    When 我再次發送 GET 請求至 "/api/members/m001"
    Then 資料庫查詢次數應為 1
    And Redis 查詢次數應為 2
