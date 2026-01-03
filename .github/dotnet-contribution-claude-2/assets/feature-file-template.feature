# Feature 檔案範本（BDD）
# 使用 Gherkin 語法定義功能情境

Feature: {Feature Name}
  As a {Role}
  I want to {Goal}
  So that {Value}

  # ========================================
  # Background: 所有 Scenario 共用的前置條件
  # ========================================
  Background:
    Given the system is running
    And the database is clean

  # ========================================
  # Scenario: 成功情境（Happy Path）
  # ========================================
  Scenario: Successfully {Action}
    Given I have the following data:
      | Field  | Value          |
      | Name   | Test Name      |
      | Email  | test@email.com |
    When I send a POST request to "/api/{endpoint}"
    Then the response status code should be 201
    And the response should contain an ID
    And the database should contain this record

  # ========================================
  # Scenario: 驗證錯誤情境
  # ========================================
  Scenario: Validation error when {Condition}
    Given I have the following data:
      | Field  | Value |
      | Name   |       |
      | Email  | test  |
    When I send a POST request to "/api/{endpoint}"
    Then the response status code should be 400
    And the response should contain error message "Validation error"

  # ========================================
  # Scenario: 資源不存在情境
  # ========================================
  Scenario: Not found when {Condition}
    Given no record exists with ID "550e8400-e29b-41d4-a716-446655440000"
    When I send a GET request to "/api/{endpoint}/550e8400-e29b-41d4-a716-446655440000"
    Then the response status code should be 404
    And the response should contain error message "Not found"

  # ========================================
  # Scenario: 重複資料情境
  # ========================================
  Scenario: Conflict when {Condition}
    Given the system already has the following data:
      | Field  | Value           |
      | Email  | existing@test.com |
    And I have the following data:
      | Field  | Value           |
      | Email  | existing@test.com |
    When I send a POST request to "/api/{endpoint}"
    Then the response status code should be 409
    And the response should contain error message "Duplicate"

  # ========================================
  # Scenario Outline: 參數化測試
  # ========================================
  Scenario Outline: Validation error for <Field>
    Given I have the following data:
      | Field  | Value   |
      | Name   | <Name>  |
      | Email  | <Email> |
    When I send a POST request to "/api/{endpoint}"
    Then the response status code should be 400
    And the response should contain error message "<ErrorMessage>"

    Examples:
      | Name      | Email              | ErrorMessage          |
      |           | test@example.com   | Name is required      |
      | Test Name |                    | Email is required     |
      | Test Name | invalid-email      | Invalid email format  |

  # ========================================
  # Scenario: 更新成功情境
  # ========================================
  Scenario: Successfully update {Resource}
    Given the system already has the following data:
      | Field  | Value          |
      | ID     | 550e8400-e29b  |
      | Name   | Old Name       |
      | Email  | old@email.com  |
    And I have the following update data:
      | Field  | Value          |
      | Name   | New Name       |
      | Email  | new@email.com  |
    When I send a PUT request to "/api/{endpoint}/550e8400-e29b"
    Then the response status code should be 200
    And the response should contain "New Name"
    And the database should be updated

  # ========================================
  # Scenario: 刪除成功情境
  # ========================================
  Scenario: Successfully delete {Resource}
    Given the system already has a record with ID "550e8400-e29b"
    When I send a DELETE request to "/api/{endpoint}/550e8400-e29b"
    Then the response status code should be 204
    And the database should not contain this record

  # ========================================
  # Scenario: 分頁查詢情境
  # ========================================
  Scenario: Successfully get paginated {Resources}
    Given the system has 25 records
    When I send a GET request to "/api/{endpoint}?pageIndex=0&pageSize=10"
    Then the response status code should be 200
    And the response should contain 10 items
    And the response should show total count of 25
    And the response should show 3 total pages

  # ========================================
  # Scenario: 條件查詢情境
  # ========================================
  Scenario: Successfully search {Resources} by keyword
    Given the system already has the following data:
      | Name        | Email              |
      | John Doe    | john@example.com   |
      | Jane Smith  | jane@example.com   |
      | Bob Johnson | bob@example.com    |
    When I send a GET request to "/api/{endpoint}?search=john"
    Then the response status code should be 200
    And the response should contain 2 items
    And the response should contain "John Doe"
    And the response should contain "Bob Johnson"

  # ========================================
  # Scenario: 併發衝突情境
  # ========================================
  Scenario: Concurrency conflict when updating
    Given the system already has a record with version 1
    And another user has updated the record to version 2
    When I try to update the record with version 1
    Then the response status code should be 409
    And the response should contain error message "Concurrency conflict"

  # ========================================
  # Scenario: 權限檢查情境
  # ========================================
  Scenario: Unauthorized when not logged in
    Given I am not authenticated
    When I send a POST request to "/api/{endpoint}"
    Then the response status code should be 401

  Scenario: Forbidden when insufficient permissions
    Given I am authenticated as "regular-user"
    When I send a DELETE request to "/api/{endpoint}/550e8400-e29b"
    Then the response status code should be 403
