# language: zh-TW
功能: {{FEATURE_NAME}}管理
  作為系統使用者
  我想要能夠管理{{FEATURE_DISPLAY_NAME}}
  以便有效地處理相關業務

  背景:
    假如 系統中存在以下測試資料
      | 資料類型 | 數量 |
      | 會員     | 3    |
      | 產品     | 5    |

  場景: 成功建立新的{{FEATURE_DISPLAY_NAME}}
    假如 我是已登入的使用者
    而且 我準備了有效的{{FEATURE_DISPLAY_NAME}}資料
      | 欄位名稱 | 值           |
      | Name     | 測試項目 A   |
      | Status   | Active       |
    當 我呼叫 POST /api/{{FEATURE_NAME_LOWER}} API
    那麼 回應狀態碼應該是 201 Created
    而且 回應本文應該包含 {{FEATURE_NAME}} ID
    而且 回應本文的 Name 應該是 "測試項目 A"

  場景: 建立{{FEATURE_DISPLAY_NAME}}時缺少必填欄位
    假如 我是已登入的使用者
    而且 我準備了不完整的{{FEATURE_DISPLAY_NAME}}資料
      | 欄位名稱 | 值    |
      | Status   | Active |
    當 我呼叫 POST /api/{{FEATURE_NAME_LOWER}} API
    那麼 回應狀態碼應該是 400 Bad Request
    而且 錯誤訊息應該包含 "Name is required"

  場景: 成功取得{{FEATURE_DISPLAY_NAME}}清單
    假如 系統中已存在 5 筆{{FEATURE_DISPLAY_NAME}}
    當 我呼叫 GET /api/{{FEATURE_NAME_LOWER}} API
    那麼 回應狀態碼應該是 200 OK
    而且 回應本文應該包含 5 筆資料

  場景: 根據 ID 成功取得{{FEATURE_DISPLAY_NAME}}
    假如 系統中存在 ID 為 1 的{{FEATURE_DISPLAY_NAME}}
    當 我呼叫 GET /api/{{FEATURE_NAME_LOWER}}/1 API
    那麼 回應狀態碼應該是 200 OK
    而且 回應本文的 Id 應該是 1

  場景: 取得不存在的{{FEATURE_DISPLAY_NAME}}
    當 我呼叫 GET /api/{{FEATURE_NAME_LOWER}}/999 API
    那麼 回應狀態碼應該是 404 Not Found
    而且 錯誤訊息應該包含 "not found"

  場景: 成功更新{{FEATURE_DISPLAY_NAME}}
    假如 系統中存在 ID 為 1 的{{FEATURE_DISPLAY_NAME}}
    而且 我準備了更新的{{FEATURE_DISPLAY_NAME}}資料
      | 欄位名稱 | 值           |
      | Name     | 已更新項目   |
      | Status   | Inactive     |
    當 我呼叫 PUT /api/{{FEATURE_NAME_LOWER}}/1 API
    那麼 回應狀態碼應該是 200 OK
    而且 回應本文的 Name 應該是 "已更新項目"
    而且 回應本文的 Status 應該是 "Inactive"

  場景: 更新不存在的{{FEATURE_DISPLAY_NAME}}
    當 我呼叫 PUT /api/{{FEATURE_NAME_LOWER}}/999 API 並提供有效資料
    那麼 回應狀態碼應該是 404 Not Found

  場景: 成功刪除{{FEATURE_DISPLAY_NAME}}
    假如 系統中存在 ID 為 1 的{{FEATURE_DISPLAY_NAME}}
    當 我呼叫 DELETE /api/{{FEATURE_NAME_LOWER}}/1 API
    那麼 回應狀態碼應該是 204 No Content
    而且 當我再次呼叫 GET /api/{{FEATURE_NAME_LOWER}}/1 API
    那麼 回應狀態碼應該是 404 Not Found

  場景: 刪除不存在的{{FEATURE_DISPLAY_NAME}}
    當 我呼叫 DELETE /api/{{FEATURE_NAME_LOWER}}/999 API
    那麼 回應狀態碼應該是 404 Not Found

  場景大綱: 資料驗證測試
    假如 我準備了{{FEATURE_DISPLAY_NAME}}資料，其中 <欄位> 為 <值>
    當 我呼叫 POST /api/{{FEATURE_NAME_LOWER}} API
    那麼 回應狀態碼應該是 <狀態碼>

    範例:
      | 欄位   | 值              | 狀態碼 |
      | Name   | ""              | 400    |
      | Name   | "有效名稱"      | 201    |
      | Status | "InvalidStatus" | 400    |
      | Status | "Active"        | 201    |

# ==================== 使用說明 ====================
# 
# 此範本提供完整的 CRUD 操作測試情境
# 
# 需要替換的變數:
# - {{FEATURE_NAME}}: 功能名稱 (PascalCase)，例如: Member, Order, Product
# - {{FEATURE_NAME_LOWER}}: 功能名稱 (lowercase)，例如: member, order, product
# - {{FEATURE_DISPLAY_NAME}}: 功能顯示名稱 (中文)，例如: 會員, 訂單, 產品
#
# 測試情境包含:
# 1. ✅ 建立 (Create)
#    - 成功建立
#    - 缺少必填欄位
#    - 資料驗證錯誤
#
# 2. ✅ 讀取 (Read)
#    - 取得清單
#    - 根據 ID 取得單筆
#    - 取得不存在的資料
#
# 3. ✅ 更新 (Update)
#    - 成功更新
#    - 更新不存在的資料
#
# 4. ✅ 刪除 (Delete)
#    - 成功刪除
#    - 刪除不存在的資料
#
# 5. ✅ 驗證 (Validation)
#    - 使用場景大綱測試多種驗證情境
#
# 自訂建議:
# - 根據實際業務需求新增特定情境
# - 調整測試資料以符合真實情境
# - 加入更多邊界條件與異常測試
# - 考慮加入效能測試情境（大量資料）
# - 加入安全性測試情境（權限驗證）
