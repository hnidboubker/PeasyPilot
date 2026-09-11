# BDD Testing Guide

## Overview

Behavior-Driven Development (BDD) writes tests as business scenarios. Instead of "test user creation," you write "Given I'm a user, When I register, Then I should receive a confirmation email."

**Prerequisites:** [Getting Started](../GETTING-STARTED.md)  
**Time:** 25 minutes  

---

## Why BDD Matters

BDD bridges the gap between developers and business stakeholders. Tests read like requirements, making them self-documenting.

---

## Gherkin Syntax

Feature files use Gherkin language:

```gherkin
Feature: User Registration
  Scenario: Register with valid email
    Given I'm a new user
    When I submit the registration form with email "john@example.com"
    Then I should receive a confirmation email
    And my account should be active
```

---

## Step Definitions

Bind Gherkin steps to code using attributes:

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class UserRegistrationSteps : BddStepDefinition
{
    private string _email = null!;
    private bool _emailSent;
    private User _registeredUser = null!;
    
    [Given("I'm a new user")]
    public async Task NewUser()
    {
        await Task.CompletedTask;
    }
    
    [When("I submit the registration form with email {email}")]
    public async Task SubmitRegistrationWithEmail(string email)
    {
        _email = email;
        var service = new UserService();
        _registeredUser = await service.RegisterAsync(email);
        _emailSent = true;
    }
    
    [Then("I should receive a confirmation email")]
    public async Task ConfirmationEmailSent()
    {
        Assert.True(_emailSent);
        await Task.CompletedTask;
    }
    
    [And("my account should be active")]
    public async Task AccountIsActive()
    {
        Assert.True(_registeredUser.IsActive);
        await Task.CompletedTask;
    }
}
```

---

## Pattern Matching

Extract parameters from step text:

```csharp
[Given("I have {count} items in the cart")]
public async Task ItemsInCart(int count)
{
    _cart.AddItems(count);
    await Task.CompletedTask;
}

[When("I apply the coupon {code}")]
public async Task ApplyCoupon(string code)
{
    _discount = await _couponService.GetDiscountAsync(code);
    await Task.CompletedTask;
}

[Then("the total should be ${amount}")]
public async Task TotalIs(decimal amount)
{
    Assert.Equal(amount, _cart.Total);
    await Task.CompletedTask;
}
```

---

## Examples

### Example 1: Simple Feature Test

**Feature file** (features/login.feature):
```gherkin
Feature: User Login
  Scenario: Login with valid credentials
    Given a user with email "john@example.com" and password "password123"
    When I log in
    Then I should see the dashboard
```

**Step definitions**:
```csharp
public class LoginSteps : BddStepDefinition
{
    private User _user = null!;
    private bool _loginSuccess;
    
    [Given("a user with email {email} and password {password}")]
    public async Task UserExists(string email, string password)
    {
        _user = new User { Email = email, Password = password };
        await Task.CompletedTask;
    }
    
    [When("I log in")]
    public async Task LogIn()
    {
        var service = new AuthService();
        _loginSuccess = await service.LoginAsync(_user.Email, _user.Password);
    }
    
    [Then("I should see the dashboard")]
    public async Task SeeDashboard()
    {
        Assert.True(_loginSuccess);
        await Task.CompletedTask;
    }
}
```

### Example 2: Order Processing

```gherkin
Feature: Order Processing
  Scenario: Calculate order total with tax
    Given a cart with items
    And the customer is from California
    When I calculate the total
    Then tax should be 8.25%
    And I should see the order summary
```

### Example 3: Multiple Scenarios

```gherkin
Feature: Password Validation
  Scenario: Accept strong password
    When I enter password "SecurePass123!"
    Then the password should be valid
  
  Scenario: Reject weak password
    When I enter password "123"
    Then the password should be invalid
    And I should see "Password too short"
```

### Example 4: Executing BDD Tests

```csharp
public class UserRegistrationBddTests
{
    [Fact]
    public async Task UserRegistration_ExecuteFeature()
    {
        // Load feature file
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/user-registration.feature");
        
        // Setup step binding
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(UserRegistrationSteps));
        
        // Execute scenarios
        var executor = new ScenarioExecutor(resolver);
        var scenario = feature.Scenarios.First();
        var result = await executor.ExecuteAsync(scenario, null);
        
        Assert.True(result.Status == ScenarioStatus.Passed);
    }
}
```

### Example 5: Integration with Database

```csharp
public class OrderBddTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOrderService, OrderService>();
    }
    
    [Fact]
    public async Task OrderProcessing_CompleteFlow()
    {
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/order-processing.feature");
        
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(OrderSteps));
        
        var executor = new ScenarioExecutor(resolver);
        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, null);
            Assert.True(result.Status == ScenarioStatus.Passed);
        }
    }
}
```

---

## Best Practices

✅ **DO**
- Write scenarios in business language
- One scenario = one user journey
- Use concrete examples
- Keep steps independent
- Share context between steps properly

❌ **DON'T**
- Write technical scenarios (use unit tests instead)
- Hardcode test data in step definitions
- Create dependencies between scenarios
- Use vague step names
- Mix multiple workflows in one scenario

---

## Next Steps

📖 **[Integration Testing Guide](./integration-testing-guide.md)** – Add database tests  
📖 **[Test Generation Guide](./test-generation-guide.md)** – Auto-generate tests  

BDD makes tests readable and maintainable! 🎭
