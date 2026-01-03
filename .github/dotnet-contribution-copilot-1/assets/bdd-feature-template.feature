# language: zh-TW
# encoding: utf-8

功能: {Feature} 管理
  作為 {角色}
  我想要 {功能描述}
  以便 {業務價值}

  背景:
    假如 系統已初始化
    而且 資料庫已清空

  @positive @smoke
  場景: 成功建立 {Feature}
    假如 系統中不存在 Email "test@example.com" 的 {Feature}
    當 我使用以下資訊建立 {Feature}：
      | 欄位  | 值               |
      | Email | test@example.com |
      | Name  | 測試使用者        |
    那麼 建立應該成功
    而且 系統應該回傳 {Feature} ID
    而且 資料庫中應該存在該 {Feature} 資料

  @negative
  場景: 重複 Email 建立失敗
    假如 系統中已存在 Email "test@example.com" 的 {Feature}
    當 我使用 Email "test@example.com" 建立 {Feature}
    那麼 建立應該失敗
    而且 HTTP 狀態碼應該是 409
    而且 錯誤訊息應該包含 "already exists"

  @negative @validation
  場景: 無效 Email 格式驗證
    當 我使用以下無效 Email 建立 {Feature}：
      | Email          |
      | invalid-email  |
      | @example.com   |
      | test@          |
    那麼 建立應該失敗
    而且 HTTP 狀態碼應該是 400
    而且 錯誤訊息應該包含 "Invalid email format"

  @negative @validation
  場景大綱: 必填欄位驗證
    當 我使用以下資訊建立 {Feature}：
      | Email   | Name   |
      | <Email> | <Name> |
    那麼 建立應該失敗
    而且 HTTP 狀態碼應該是 400
    而且 錯誤訊息應該包含 "<ErrorMessage>"

    例子:
      | Email            | Name | ErrorMessage          |
      |                  | Test | Email is required     |
      | test@example.com |      | Name is required      |
      |                  |      | Email and Name are required |

  @positive
  場景: 查詢 {Feature} 列表
    假如 資料庫中有以下 {Feature}：
      | Email             | Name      |
      | user1@test.com    | 使用者1   |
      | user2@test.com    | 使用者2   |
      | user3@test.com    | 使用者3   |
    當 我呼叫 GET "/api/{feature}?pageNumber=0&pageSize=10"
    那麼 HTTP 狀態碼應該是 200
    而且 回應應該包含 3 筆 {Feature} 資料

  @positive @pagination
  場景: 分頁查詢 {Feature} 列表
    假如 資料庫中有 25 筆 {Feature} 資料
    當 我呼叫 GET "/api/{feature}?pageNumber=0&pageSize=10"
    那麼 HTTP 狀態碼應該是 200
    而且 回應應該包含 10 筆 {Feature} 資料
    當 我呼叫 GET "/api/{feature}?pageNumber=1&pageSize=10"
    那麼 HTTP 狀態碼應該是 200
    而且 回應應該包含 10 筆 {Feature} 資料
    當 我呼叫 GET "/api/{feature}?pageNumber=2&pageSize=10"
    那麼 HTTP 狀態碼應該是 200
    而且 回應應該包含 5 筆 {Feature} 資料

  @positive
  場景: 根據 ID 查詢單一 {Feature}
    假如 資料庫中有以下 {Feature}：
      | Email          | Name   |
      | test@test.com  | 測試者 |
    而且 我儲存該 {Feature} 的 ID
    當 我呼叫 GET "/api/{feature}/{ID}"
    那麼 HTTP 狀態碼應該是 200
    而且 回應的 Email 應該是 "test@test.com"
    而且 回應的 Name 應該是 "測試者"

  @negative
  場景: 查詢不存在的 {Feature}
    假如 資料庫中不存在 ID 為 "00000000-0000-0000-0000-000000000001" 的 {Feature}
    當 我呼叫 GET "/api/{feature}/00000000-0000-0000-0000-000000000001"
    那麼 HTTP 狀態碼應該是 404
    而且 錯誤訊息應該包含 "not found"

  @positive
  場景: 更新 {Feature} 資料
    假如 資料庫中有以下 {Feature}：
      | Email          | Name     |
      | test@test.com  | 原始名稱 |
    而且 我儲存該 {Feature} 的 ID
    當 我使用以下資訊更新該 {Feature}：
      | Email             | Name     |
      | updated@test.com  | 新名稱   |
    那麼 更新應該成功
    而且 HTTP 狀態碼應該是 200
    而且 資料庫中該 {Feature} 的 Email 應該是 "updated@test.com"
    而且 資料庫中該 {Feature} 的 Name 應該是 "新名稱"

  @negative
  場景: 更新不存在的 {Feature}
    當 我嘗試更新 ID 為 "00000000-0000-0000-0000-000000000001" 的 {Feature}
    那麼 更新應該失敗
    而且 HTTP 狀態碼應該是 404

  @positive
  場景: 刪除 {Feature}
    假如 資料庫中有以下 {Feature}：
      | Email          | Name   |
      | test@test.com  | 測試者 |
    而且 我儲存該 {Feature} 的 ID
    當 我呼叫 DELETE "/api/{feature}/{ID}"
    那麼 HTTP 狀態碼應該是 204
    而且 資料庫中不應該存在該 {Feature}

  @negative
  場景: 刪除不存在的 {Feature}
    當 我呼叫 DELETE "/api/{feature}/00000000-0000-0000-0000-000000000001"
    那麼 HTTP 狀態碼應該是 404

  @performance
  場景: 大量資料查詢效能測試
    假如 資料庫中有 1000 筆 {Feature} 資料
    當 我呼叫 GET "/api/{feature}?pageNumber=0&pageSize=100"
    那麼 HTTP 狀態碼應該是 200
    而且 回應時間應該少於 1000 毫秒
    而且 回應應該包含 100 筆 {Feature} 資料

  @security @authorization
  場景: 未授權的使用者無法存取
    假如 我未登入
    當 我呼叫 GET "/api/{feature}"
    那麼 HTTP 狀態碼應該是 401
    而且 錯誤訊息應該包含 "Unauthorized"

#
# 使用說明：
#
# 1. 將 {Feature} 替換為實際的功能名稱（例如：Member、Product、Order）
# 2. 將 {feature} 替換為小寫的 API 路徑（例如：member、product、order）
# 3. 將 {角色} 替換為實際角色（例如：系統管理員、一般使用者）
# 4. 完善功能描述與業務價值
# 5. 根據實際需求調整場景與資料表
#
# Gherkin 關鍵字：
# - 功能 (Feature): 描述功能
# - 背景 (Background): 每個場景執行前的共同前置條件
# - 場景 (Scenario): 單一測試情境
# - 場景大綱 (Scenario Outline): 參數化測試
# - 假如 (Given): 前置條件
# - 當 (When): 執行動作
# - 那麼 (Then): 預期結果
# - 而且 (And): 額外條件或結果
#
# 標籤 (Tags)：
# - @positive: 正向測試（Happy Path）
# - @negative: 負向測試（錯誤情境）
# - @smoke: 冒煙測試（核心功能）
# - @validation: 驗證測試
# - @pagination: 分頁測試
# - @performance: 效能測試
# - @security: 安全性測試
# - @authorization: 授權測試
#
# 最佳實踐：
# - 每個場景專注於單一行為
# - 使用有意義的場景名稱
# - 使用資料表 (Table) 提供測試資料
# - 使用場景大綱 (Scenario Outline) 減少重複
# - 使用標籤組織測試（可選擇執行特定標籤的測試）
# - 保持 Given-When-Then 結構清晰
#
