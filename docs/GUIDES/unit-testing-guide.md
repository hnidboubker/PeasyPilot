# Unit Testing Guide

## Overview

In this guide you'll learn how to:
- Write clean, focused unit tests using PeasyPilot
- Use fluent assertions effectively
- Apply builder patterns for test data
- Integrate mocking with Moq
- Structure tests following best practices
- Test edge cases and error conditions

**Prerequisites:** Complete [Getting Started](../GETTING-STARTED.md)  
**Time estimate:** 30 minutes  
**Frameworks:** xUnit, NUnit, TUnit  
**Code examples:** 8 working examples

---

## Why Unit Testing Matters

Unit tests are the first line of defense in your testing strategy. They verify that individual methods and classes work correctly in isolation, catching bugs early and providing confidence when refactoring.

A well-written unit test is:
- **Fast** – Runs in milliseconds, no database calls
- **Isolated** – Tests one behavior at a time
- **Repeatable** – Same result every time
- **Self-checking** – No manual verification needed
- **Maintainable** – Clear intent, easy to understand

---

## Core Concepts

### 1. Arrange-Act-Assert (AAA) Pattern

Every unit test follows three distinct phases:

```csharp
[Fact]
public void Add_WithPositiveNumbers_ReturnsSum()
{
    // ARRANGE: Set up test data and preconditions
    var calculator = new Calculator();
    int a = 5;
    int b = 3;
    int expected = 8;
    
    // ACT: Execute the code under test
    int result = calculator.Add(a, b);
    
    // ASSERT: Verify the result
    Assert.Equal(expected, result);
}
```

**Arrange** sets the stage. **Act** performs the operation. **Assert** validates the outcome.

### 2. Test Naming Convention

Test names should clearly describe what is being tested and what is expected:

```
[MethodName]_[Condition]_[ExpectedBehavior]
```

**Good examples:**
- `Add_WithPositiveNumbers_ReturnsSum`
- `Divide_ByZero_ThrowsArgumentException`
- `GetUser_WithValidId_ReturnsUser`

**Bad examples:**
- `TestAdd` (vague)
- `Add_Works` (unclear what works)
- `Test1` (no meaning)

### 3. Single Responsibility

Each test should verify **one behavior**. If a test has multiple assertions, ask: "Are these testing the same behavior or different behaviors?"

```csharp
// ❌ BAD: Tests multiple unrelated behaviors
[Fact]
public void Calculator_DoesEverything()
{
    var calc = new Calculator();
    Assert.Equal(8, calc.Add(5, 3));           // Addition
    Assert.Equal(20, calc.Multiply(4, 5));    // Multiplication
    Assert.Throws<DivideByZeroException>(() => calc.Divide(1, 0)); // Division errors
}

// ✅ GOOD: One test per behavior
[Fact]
public void Add_WithPositiveNumbers_ReturnsSum()
{
    var calc = new Calculator();
    Assert.Equal(8, calc.Add(5, 3));
}

[Fact]
public void Multiply_WithTwoNumbers_ReturnsProduct()
{
    var calc = new Calculator();
    Assert.Equal(20, calc.Multiply(4, 5));
}

[Fact]
public void Divide_ByZero_ThrowsDivideByZeroException()
{
    var calc = new Calculator();
    Assert.Throws<DivideByZeroException>(() => calc.Divide(1, 0));
}
```

---

## Getting Started: Basic Unit Tests

### Example 1: Simple Assertion

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    [Fact]
    public void Add_WithTwoNumbers_ReturnSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        int result = calculator.Add(5, 3);
        
        // Assert
        Assert.Equal(8, result);
    }
}
```

Run with:
```bash
dotnet test
```

### Example 2: Multiple Related Assertions

When a method produces multiple outputs, group related assertions:

```csharp
[Fact]
public void CreateUser_WithValidEmail_ReturnsUserWithEmailSet()
{
    // Arrange
    var userService = new UserService();
    string email = "john@example.com";
    
    // Act
    var user = userService.CreateUser(email);
    
    // Assert
    Assert.NotNull(user);
    Assert.Equal(email, user.Email);
    Assert.True(user.IsActive);
}
```

### Example 3: Testing Exceptions

```csharp
[Fact]
public void WithdrawMoney_InsufficientFunds_ThrowsInsufficientFundsException()
{
    // Arrange
    var account = new BankAccount(balance: 100);
    
    // Act & Assert
    var exception = Assert.Throws<InsufficientFundsException>(
        () => account.Withdraw(500)
    );
    
    Assert.Equal("Account balance is insufficient.", exception.Message);
}
```

---

## Advanced Patterns

### Example 4: Builder Pattern for Complex Objects

When creating complex test data, use builders to keep tests readable:

```csharp
public class UserBuilder
{
    private string _email = "test@example.com";
    private string _name = "Test User";
    private bool _isActive = true;
    
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public UserBuilder Inactive()
    {
        _isActive = false;
        return this;
    }
    
    public User Build() => new User(_email, _name) { IsActive = _isActive };
}

// Usage in test:
[Fact]
public void SendWelcomeEmail_WithInactiveUser_DoesNotSend()
{
    // Arrange
    var user = new UserBuilder()
        .WithEmail("john@example.com")
        .WithName("John Doe")
        .Inactive()
        .Build();
    var emailService = new EmailService();
    
    // Act
    emailService.SendWelcomeEmail(user);
    
    // Assert - verify no email was sent
    Assert.False(emailService.WasEmailSent(user.Email));
}
```

### Example 5: Mocking Dependencies with Moq

Unit tests must isolate the code being tested. Mock external dependencies:

```csharp
using PeasyPilot.Moq;

[Fact]
public void CreateOrder_CallsInventoryService()
{
    // Arrange
    var mockInventory = new MockFactory().Create<IInventoryService>();
    var orderService = new OrderService(mockInventory);
    var product = new Product { Id = 1, Name = "Widget" };
    
    // Act
    var order = orderService.CreateOrder(product, quantity: 5);
    
    // Assert
    // Verify the mock was called
    mockInventory.Verify(
        x => x.ReserveStock(product.Id, 5),
        Times.Once
    );
}
```

### Example 6: Parameterized Tests

Test multiple scenarios with data-driven tests:

```csharp
[Theory]
[InlineData(5, 3, 8)]
[InlineData(0, 0, 0)]
[InlineData(-5, 3, -2)]
[InlineData(100, -50, 50)]
public void Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    int result = calculator.Add(a, b);
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Example 7: Async Tests

Modern .NET code is async. PeasyPilot handles this natively:

```csharp
[Fact]
public async Task FetchUserAsync_WithValidId_ReturnsUser()
{
    // Arrange
    var userRepository = new UserRepository();
    
    // Act
    var user = await userRepository.GetUserAsync(userId: 1);
    
    // Assert
    Assert.NotNull(user);
    Assert.Equal("John Doe", user.Name);
}
```

### Example 8: Using Test Fixtures

Share setup code across multiple tests:

```csharp
public class OrderServiceTests : PeasyPilotTestBase
{
    private OrderService _orderService = null!;
    private IInventoryService _inventoryService = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Setup shared dependencies
        _inventoryService = new MockInventoryService();
        _orderService = new OrderService(_inventoryService);
    }
    
    [Fact]
    public void CreateOrder_WithValidProduct_Succeeds()
    {
        var order = _orderService.CreateOrder(productId: 1, quantity: 5);
        Assert.NotNull(order);
    }
    
    [Fact]
    public void CreateOrder_WithInvalidProduct_ThrowsException()
    {
        Assert.Throws<InvalidProductException>(
            () => _orderService.CreateOrder(productId: 999, quantity: 5)
        );
    }
}
```

---

## Best Practices

### ✅ DO

- **Write one assertion per behavior** – Each test verifies one thing
- **Use descriptive names** – Test names document expected behavior
- **Keep tests fast** – Avoid I/O, databases, network calls
- **Make tests independent** – No test should depend on another
- **Use meaningful variable names** – `user` not `u`, `emailService` not `es`
- **Test edge cases** – Boundary values, null inputs, empty collections
- **Keep tests maintainable** – Refactor test code as you would production code
- **Mock external dependencies** – Isolate the code under test

### ❌ DON'T

- **Don't test multiple behaviors in one test** – Split into separate tests
- **Don't use magic numbers** – Use named constants: `const int ADMIN_ID = 1;`
- **Don't create complex test setup** – Simplify or move to builders/fixtures
- **Don't test implementation details** – Test public behavior
- **Don't skip tests** – Fix or remove them
- **Don't use `Thread.Sleep`** – Use proper async/await
- **Don't repeat test code** – Use shared fixtures or builders
- **Don't ignore exceptions** – Always assert on error cases

---

## Troubleshooting

### "Test is too slow"
**Cause:** Accessing databases, files, or network  
**Solution:** Mock external dependencies with Moq or test doubles

### "Test passes sometimes, fails other times"
**Cause:** Tests depend on each other or external state  
**Solution:** Ensure each test is independent; use fixtures to reset state

### "Test name is too long"
**Cause:** Trying to describe too much in the name  
**Solution:** The test should only verify one behavior. Split into multiple tests.

### "Mock verification fails randomly"
**Cause:** Using `Times.AtLeastOnce()` or similar non-deterministic expectations  
**Solution:** Be specific: `Times.Once`, `Times.Exactly(2)`, etc.

---

## Next Steps

You've mastered unit testing! Here's what to explore next:

📖 **[Integration Testing Guide](./integration-testing-guide.md)** – When to test with real databases  
📖 **[BDD Testing Guide](./bdd-testing-guide.md)** – Write tests in business language  
📖 **[Test Generation Guide](./test-generation-guide.md)** – Automate test creation  
📖 **[Framework Adapters](./framework-adapters-guide.md)** – Choose between xUnit/NUnit/TUnit  

**Key Takeaway:** Good unit tests are fast, focused, and document the expected behavior of your code. Write them first, refactor confidently later. 🚀

---

**[← Back to Learning Guides](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./unit-testing-guide-FR.md)**
