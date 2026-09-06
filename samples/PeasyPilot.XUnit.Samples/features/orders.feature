Feature: Order Processing
  As an e-commerce system
  I want to process orders
  So that customers can purchase products

  Scenario: Create an order
    Given the order database is empty
    When I create an order for customer "customer1"
    Then the order should be created
    And the order status should be "Pending"

  Scenario: Add items to order
    Given an order for customer "customer1"
    When I add 2 units of product "Widget" at $10.00 each
    Then the order should have 1 item
    And the order total should be $20.00

  Scenario: Multiple items in order
    Given an order for customer "customer1"
    When I add 1 unit of product "Widget" at $10.00
    And I add 2 units of product "Gadget" at $5.00 each
    Then the order should have 2 items
    And the order total should be $20.00

  Scenario: Update order status
    Given an order for customer "customer1" with status "Pending"
    When I update the order status to "Confirmed"
    Then the order status should be "Confirmed"

  Scenario: Cancel order
    Given an order for customer "customer1" with items
    When I cancel the order
    Then the order status should be "Cancelled"
    And the order should not be active
