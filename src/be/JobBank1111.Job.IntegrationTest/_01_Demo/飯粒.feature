Feature: 飯粒

    Background:
        Given 調用端已準備 Header 參數
            | x-trace-id |
            | TW         |
        Given 調用端已準備 Query 參數
            | select-profile |
            | avatarUrl      |
        Given 初始化測試伺服器
            | Now                       | UserId |
            | 2000-01-01T00:00:00+00:00 | QOO    |

    Scenario: 新增一筆會員
        Given 調用端已準備 Body 參數(Json)
        """
        {
          "email": "yao@9527",
          "name": "yao",
          "age": 18
        }
        """
        When 調用端發送 "POST" 請求至 "member"
        Then 預期得到 HttpStatusCode 為 "204"
        Then 預期資料庫已存在 Member 資料為
            | Email    | Name | Age | CreatedAt                 | CreatedAt                 |
            | yao@9527 | yao  | 18  | 2000-01-01T00:00:00+00:00 | 2000-01-01T00:00:00+00:00 |

    Scenario: 查詢一筆會員
        Given 資料庫已存在 Member 資料
            | Id |
            | 1  |
        Given 建立模擬 API，HttpMethod = "POST"，URL = "/ec/V1/SalePage/UpdateStock"，StatusCode = "200"，ResponseContent =
        """
        {
            "ErrorId": "",
            "Status": "Success",
            "Data": "",
            "ErrorMessage": null,
            "TimeStamp": "2024-02-21T16:55:21.4988154+08:00"
        }
        """

    Scenario: 用 JsonDiff 驗證資料
        When 模擬呼叫 API，得到以下內容
        """
        {
            "Id": 1,
            "Age": 18,
            "Birthday": "2000-01-01T00:00:00+00:00",
             "FullName": {
                "FirstName": "John",
                "LastName": "Doe"
            }
        }
        """
        Then 預期回傳內容為
        """
        {
            "Id": 1,
            "Age": 18,
            "Birthday": "2000-01-01T00:00:00+00:00",
             "FullName": {
                "FirstName": "John",
                "LastName": "Doe"
            }
        }
        """

    Scenario: 用 JsonPath 驗證資料
        Given 已存在 Json 內容
        """
        {
            "Id": 1,
            "Age": 18,
            "Birthday": "2000-01-01T00:00:00+00:00",
             "FullName": {
                "FirstName": "John",
                "LastName": "Doe"
            }
        }
        """
        Then 預期回傳內容中路徑 "$.Age" 的"數值等於" "18"
        Then 預期回傳內容中路徑 "$.Birthday" 的"時間等於" "2000-01-01T00:00:00+00:00"
        Then 預期回傳內容中路徑 "$.FullName.FirstName" 的"字串等於" "John"
        Then 預期回傳內容中路徑 "$.FullName.LastName" 的"字串等於" "Doe"