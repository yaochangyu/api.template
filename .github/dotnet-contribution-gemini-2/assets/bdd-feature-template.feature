Feature: {{feature-name}} Management

Background:
    Given 初始化測試伺服器
        | Now                       | UserId |
        | 2000-01-01T00:00:00+00:00 | test-user |
    Given 調用端已準備 Header 參數
        | x-trace-id |
        | {{some-guid}} |

Scenario: Create a new {{feature-name-singular}} successfully
    Given 調用端已準備 Body 參數(Json)
    """
    {
      "name": "New {{feature-name-singular}}",
      "description": "A valid description for the new item."
    }
    """
    When 調用端發送 "POST" 請求至 "api/v1/{{feature-name-plural}}"
    Then 預期得到 HttpStatusCode 為 "201"
    And 預期回傳內容中路徑 "$.name" 的"字串等於" "New {{feature-name-singular}}"
    And 預期資料庫已存在 {{feature-name}} 資料為
        | Name                      | Description                        |
        | New {{feature-name-singular}} | A valid description for the new item. |

Scenario: Attempt to create a {{feature-name-singular}} with a duplicate name
    Given 資料庫已存在 {{feature-name}} 資料
        | Name                  |
        | Existing {{feature-name-singular}} |
    Given 調用端已準備 Body 參數(Json)
    """
    {
      "name": "Existing {{feature-name-singular}}",
      "description": "This should fail."
    }
    """
    When 調用端發送 "POST" 請求至 "api/v1/{{feature-name-plural}}"
    Then 預期得到 HttpStatusCode 為 "400"
    And 預期回傳內容中路徑 "$.code" 的"字串等於" "ValidationError"
