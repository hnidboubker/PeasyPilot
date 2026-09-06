Feature: User Management
  As a system
  I want to manage users
  So that users can be created, retrieved, and deleted

  Scenario: Create a new user
    Given the user database is empty
    When I create a user with email "alice@example.com" and name "Alice"
    Then the user should exist in the database
    And the user count should be 1

  Scenario: Retrieve user by ID
    Given a user with email "bob@example.com" and name "Bob"
    When I retrieve the user
    Then the user name should be "Bob"
    And the user email should be "bob@example.com"

  Scenario: Multiple users isolation
    Given the user database is empty
    When I create a user with email "charlie@example.com" and name "Charlie"
    And I create a user with email "diana@example.com" and name "Diana"
    Then the user count should be 2
    And the database should contain a user named "Charlie"
    And the database should contain a user named "Diana"

  Scenario: Delete user
    Given a user with email "eve@example.com" and name "Eve"
    When I delete the user
    Then the user should not exist in the database
