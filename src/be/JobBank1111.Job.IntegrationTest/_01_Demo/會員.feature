Feature: 會員管理

#    Background:
#        Given 設定測試參數
#          | Market | UserId   | Now               | JobName        |
#          | TW     | donnahsu | 1/1/2000 01:00:00 | AdjustStockJob |
#        Given 初始化
#        Given 資料庫已存在 ChannelShop 資料
#          | id   | firm_id | channel_id | name   | api_information_json_data                                                                      | setting_json_data | channel_type        | risk_setting                      |
#          | 5    | 2       | 3          | Yahoo  | {}                                                                                             | {}                | YahooShoppingCenter | {"IsStockQtyModulePaused": false} |
#          | 4311 | 2       | 2          | MOMO   | {}                                                                                             | {}                | MOMOShop            | {"IsStockQtyModulePaused": false} |
#          | 12   | 2       | 1          | NineYi | {"market": "TW", "shopId": 12453, "apiDomain": "{{nine1_domain}}", "x-api-key": "{{api_key}}"} | {}                | NineYi              | {"IsStockQtyModulePaused": false} |
#          | 4286 | 2       | 8          | Shopee | {}                                                                                             | {}                | Shopee              | {"IsStockQtyModulePaused": false} |
#          | 4315 | 2       | 6          | PChome | {}                                                                                             | {}                | PChome24H           | {"IsStockQtyModulePaused": false} |

    Scenario: 查詢一筆會員
        Given 資料庫已存在 Member 資料
            | Id |
            | 1  |
        Given 建立模擬 API - HttpMethod = "POST"，URL = "/ec/V1/SalePage/UpdateStock"，StatusCode = "200"，ResponseContent =
        """
        {
            "ErrorId": "",
            "Status": "Success",
            "Data": "",
            "ErrorMessage": null,
            "TimeStamp": "2024-02-21T16:55:21.4988154+08:00"
        }
        """