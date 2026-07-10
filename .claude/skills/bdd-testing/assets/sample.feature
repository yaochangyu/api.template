# language: zh-TW
功能: 會員 API
  作為應用使用者
  我想要管理會員帳戶
  以便能完整使用服務功能

  背景:
    假設資料庫已初始化
    並且未有任何會員記錄

  場景: 成功建立新會員 (Happy Path)
    當我使用以下資料 POST /api/v1/members
      | Email          | Name      |
      | user@example.com | John Doe  |
    那麼應該返回 201 Created
    並且回應包含 id 欄位
    並且會員已保存到資料庫

  場景: Email 重複時拒絕 (Error Case)
    假設已存在會員
      | Email           | Name    |
      | existing@test.com | Alice  |
    當我使用以下資料 POST /api/v1/members
      | Email           | Name    |
      | existing@test.com | Bob    |
    那麼應該返回 409 Conflict
    並且錯誤代碼為 "DuplicateEmail"
    並且錯誤訊息包含 "Email already exists"

  場景: 無效 Email 格式驗證 (Edge Case)
    當我使用以下資料 POST /api/v1/members
      | Email         | Name      |
      | not-an-email  | Invalid   |
    那麼應該返回 400 Bad Request
    並且錯誤代碼為 "ValidationError"
    並且錯誤詳情包含欄位 "email"

  場景: 必填欄位驗證
    當我使用以下資料 POST /api/v1/members
      | Email | Name |
      |       |      |
    那麼應該返回 400 Bad Request
    並且錯誤詳情包含欄位 "email"
    並且錯誤詳情包含欄位 "name"

  場景: 未授權存取拒絕
    當我未經認證 GET /api/v1/members/profile
    那麼應該返回 401 Unauthorized
    並且錯誤代碼為 "Unauthorized"
