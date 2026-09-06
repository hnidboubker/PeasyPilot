# BDD Guide — PeasyPilot

Complete guide to using Behavior-Driven Development (BDD) with PeasyPilot's Gherkin support and automatic step binding.

---

## Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Feature Files](#feature-files)
4. [Step Definitions](#step-definitions)
5. [Step Binding Resolver](#step-binding-resolver)
6. [Integration Testing](#integration-testing)
7. [Complete Workflow](#complete-workflow)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## Overview

PeasyPilot provides **full Gherkin support** with:

- ✅ Feature file loading (`.feature` files)
- ✅ Automatic step discovery via reflection
- ✅ Pattern matching for step text
- ✅ Parameter extraction (string, int, decimal, etc.)
- ✅ Integration with test fixtures
- ✅ Database lifecycle management

### Core Components

```
Feature File (.feature)
        ↓
GherkinFeatureFileLoader
        ↓
Feature + Scenarios + Steps
        ↓
StepBindingResolver (reflection + pattern matching)
        ↓
ScenarioExecutor (step-by-step execution)
        ↓
ScenarioExecutionResult (Passed/Failed/Skipped)
```

---

## Quick Start

### 1. Create a Feature File

**File:** `features/calculator.feature`

```gherkin
Feature: Calculator
  Scenario: Add two numbers
    Given I have entered 50 into the calculator
    And I have entered 70 into the calculator
    When I press add
    Then the result should be 120 on the screen
```

### 2. Define Step Bindings

**File:** `StepDefinitions/CalculatorSteps.cs`

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class CalculatorSteps : BddStepDefinition
{
    private int _input1;
    private int _input2;
    private int _result;

    [Given("I have entered {number} into the calculator")]
    public async Task EnterNumber(string number)
    {
        if (int.TryParse(number, out var num))
        {
            if (_input1 == 0)
                _input1 = num;
            else
                _input2 = num;
        }
        await Task.CompletedTask;
    }

    [When("I press add")]
    public async Task PressAdd()
    {
        _result = _input1 + _input2;
        await Task.CompletedTask;
    }

    [Then("the result should be {expected} on the screen")]
    public async Task VerifyResult(string expected)
    {
        if (int.TryParse(expected, out var exp))
        {
            assert _result == exp, $"Expected {exp}, got {_result}";
        }
        await Task.CompletedTask;
    }
}
```

### 3. Write Test

**File:** `Tests/CalculatorBddTests.cs`

```csharp
using PeasyPilot.BDD.FileLoading;
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.StepDefinitions;
using Xunit;

public class CalculatorBddTests
{
    [Fact]
    public async Task CalculatorScenarios_ExecuteSuccessfully()
    {
        // Load feature file
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/calculator.feature");

        // Setup step binding resolver
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(CalculatorSteps));

        // Execute scenarios
        var executor = new ScenarioExecutor(resolver);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, serviceProvider);
            Assert.True(result.Status == ScenarioStatus.Passed, 
                $"Scenario failed: {scenario.Name}");
        }
    }
}
```

### 4. Run Tests

```bash
dotnet test
# or
dotnet test --filter "CalculatorBddTests"
```

---

## Feature Files

### File Structure

```
features/
├── calculator.feature
├── user-management.feature
└── order-processing.feature
```

### Gherkin Syntax

```gherkin
Feature: Brief description
  This is a longer description that explains the feature

  Background:
    Given some setup that applies to all scenarios

  Scenario: First scenario
    Given initial state
    When I do something
    Then I expect a result

  Scenario: Another scenario
    Given different initial state
    And some additional setup
    When I do another thing
    And do something else
    Then I expect different results
    But not this other thing
```

### Step Keywords

| Keyword | Use Case |
|---------|----------|
| `Given` | Setup/precondition |
| `When` | Action/trigger |
| `Then` | Assertion/verification |
| `And` | Continue previous step |
| `But` | Negate/alternative |

---

## Step Definitions

### Basic Structure

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class MySteps : BddStepDefinition
{
    // Step state
    private string _testValue;

    // Step methods with attributes
    [Given("some initial condition")]
    public async Task SetupCondition()
    {
        _testValue = "initialized";
        await Task.CompletedTask;
    }

    [When("I perform an action")]
    public async Task PerformAction()
    {
        _testValue = "modified";
        await Task.CompletedTask;
    }

    [Then("the result should be correct")]
    public async Task VerifyResult()
    {
        Assert.Equal("modified", _testValue);
        await Task.CompletedTask;
    }
}
```

### Attributes

```csharp
public class StepAttributes
{
    [Given("pattern")]        // Setup
    [When("pattern")]         // Action
    [Then("pattern")]         // Assertion
    [And("pattern")]          // Continuation
    [But("pattern")]          // Negation
}
```

### Parameter Patterns

```csharp
// Simple parameter
[Given("I have {name}")]
public async Task HaveName(string name) { }

// Multiple parameters
[When("I create a user with email {email} and age {age}")]
public async Task CreateUser(string email, string age) { }

// Type conversion (auto)
[Given("I have {count} items")]
public async Task HaveItems(int count) { }  // int parsed from text

[Given("I have {amount} dollars")]
public async Task HaveMoney(decimal amount) { }  // decimal parsed
```

### State Management

```csharp
public class UserSteps : BddStepDefinition
{
    // Shared state across steps in same scenario
    private User _currentUser;
    private List<User> _users = new();
    private bool _creationSucceeded;

    [Given("a user with email {email}")]
    public async Task CreateUser(string email)
    {
        _currentUser = new User { Email = email };
        await Task.CompletedTask;
    }

    [When("I save the user")]
    public async Task SaveUser()
    {
        _users.Add(_currentUser);
        _creationSucceeded = true;
        await Task.CompletedTask;
    }

    [Then("the user should be saved")]
    public async Task VerifySaved()
    {
        Assert.Contains(_currentUser, _users);
        await Task.CompletedTask;
    }
}
```

---

## Step Binding Resolver

### How It Works

```
Step Binding Resolver
    ↓
Scan BddStepDefinition classes for [Given]/[When]/[Then] attributes
    ↓
Extract pattern from attribute (e.g., "I create a user {name}")
    ↓
Convert pattern to regex (e.g., "I create a user (?<name>[^"]+)")
    ↓
When executing step, match text against all patterns
    ↓
If match found, extract parameters and invoke method
    ↓
Return Func<Task> for scenario executor
```

### Registration

```csharp
var resolver = new StepBindingResolver();

// Register step definitions
resolver.RegisterStepDefinition(typeof(UserSteps));
resolver.RegisterStepDefinition(typeof(OrderSteps));
resolver.RegisterStepDefinition(typeof(CalculatorSteps));

// Use in executor
var executor = new ScenarioExecutor(resolver);
```

### Pattern Matching Examples

```
Pattern: "I create a user {name}"
Text: "I create a user Alice"
Result: name = "Alice"

Pattern: "I have {count} items"
Text: "I have 5 items"
Result: count = "5" (converted to int 5)

Pattern: "I have {count} dollars and {cents} cents"
Text: "I have 10 dollars and 50 cents"
Result: count = "10", cents = "50"
```

---

## Integration Testing

### With Database Fixture

```csharp
using PeasyPilot.Integration.Fixtures;
using PeasyPilot.BDD.StepDefinitions;

public class UserBddIntegrationTests : XUnitIntegrationTestFixture
{
    // Configure services once
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register your repository
        var userRepository = new InMemoryUserRepository();
        services.AddSingleton<IUserRepository>(userRepository);

        // Register for automatic reset between scenarios
        RegisterResettableService(userRepository);
    }

    [Fact]
    public async Task UserManagementScenarios()
    {
        // Load features
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/users.feature");

        // Setup resolver
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(UserSteps));

        // Execute scenarios
        var executor = new ScenarioExecutor(resolver);

        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, Services);
            Assert.True(result.Status == ScenarioStatus.Passed);

            // Reset database between scenarios
            await ResetDatabaseAsync();
        }
    }
}
```

### Step Definition with DI

```csharp
public class UserSteps : BddStepDefinition
{
    private readonly IUserRepository _repository;

    // Optional: constructor for DI
    public UserSteps(IUserRepository repository)
    {
        _repository = repository;
    }

    [Given("a user with email {email}")]
    public async Task CreateUser(string email)
    {
        var user = new User { Email = email };
        await _repository.AddAsync(user);
    }

    [Then("the user should be in database")]
    public async Task VerifyInDatabase()
    {
        var users = await _repository.GetAllAsync();
        Assert.NotEmpty(users);
    }
}
```

---

## Complete Workflow

### Step 1: Plan Your Feature

```gherkin
Feature: User Registration
  Users should be able to register with email and password

  Scenario: Successful registration
    Given the registration form is displayed
    When I enter email "john@example.com"
    And I enter password "SecurePass123"
    And I submit the form
    Then I should see "Registration successful"
    And the user should be in the database
```

### Step 2: Implement Step Definitions

```csharp
public class RegistrationSteps : BddStepDefinition
{
    private readonly IUserRepository _repository;
    private User _newUser = new();
    private string _message = "";

    public RegistrationSteps(IUserRepository repository)
    {
        _repository = repository;
    }

    [Given("the registration form is displayed")]
    public async Task FormDisplayed() => await Task.CompletedTask;

    [When("I enter email {email}")]
    public async Task EnterEmail(string email)
    {
        _newUser.Email = email;
        await Task.CompletedTask;
    }

    [And("I enter password {password}")]
    public async Task EnterPassword(string password)
    {
        _newUser.Password = password;
        await Task.CompletedTask;
    }

    [And("I submit the form")]
    public async Task SubmitForm()
    {
        try
        {
            await _repository.AddAsync(_newUser);
            _message = "Registration successful";
        }
        catch
        {
            _message = "Registration failed";
        }
    }

    [Then("I should see {message}")]
    public async Task VerifyMessage(string message)
    {
        Assert.Equal(message, _message);
        await Task.CompletedTask;
    }

    [And("the user should be in the database")]
    public async Task VerifyUserInDatabase()
    {
        var users = await _repository.GetAllAsync();
        Assert.Contains(_newUser, users);
    }
}
```

### Step 3: Create Test Fixture

```csharp
public class UserRegistrationBddTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        var repository = new InMemoryUserRepository();
        services.AddSingleton<IUserRepository>(repository);
        RegisterResettableService(repository);
    }

    [Fact]
    public async Task RegistrationFeature()
    {
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync(
            "features/user-registration.feature");

        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(RegistrationSteps));

        var executor = new ScenarioExecutor(resolver);

        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, Services);
            Assert.True(result.Status == ScenarioStatus.Passed);
            await ResetDatabaseAsync();
        }
    }
}
```

### Step 4: Run and Verify

```bash
dotnet test --filter "UserRegistrationBddTests"
```

---

## Best Practices

### ✅ DO

- ✅ Write scenarios in business language (not technical)
- ✅ One scenario per user story
- ✅ Keep steps simple and focused
- ✅ Use descriptive step text
- ✅ Organize steps by domain (UserSteps, OrderSteps, etc.)
- ✅ Reset state between scenarios
- ✅ Use parameters for variation
- ✅ Document complex pattern matching

### ❌ DON'T

- ❌ Write scripts instead of scenarios
- ❌ Mix technical details in steps
- ❌ Create dependencies between scenarios
- ❌ Ignore setup/teardown
- ❌ Use ambiguous step patterns
- ❌ Hardcode test data
- ❌ Skip error scenarios

### Pattern Examples

```csharp
// ✅ GOOD: Clear, focused steps
[Given("a registered user with email {email}")]
public async Task RegisterUser(string email) { }

[When("the user logs in")]
public async Task UserLogsIn() { }

[Then("the dashboard should be displayed")]
public async Task DashboardDisplayed() { }

// ❌ BAD: Technical details, too much logic
[Given("UserService.Register called with specific email")]
public async Task TooTechnical() { }

[When("clicking button ID button_submit")]
public async Task TooTechnical2() { }
```

---

## Troubleshooting

### Step Not Found

**Error:** `No step binding found for: I do something`

**Cause:** Step definition not registered or pattern doesn't match

**Solution:**
```csharp
// Ensure step is registered
resolver.RegisterStepDefinition(typeof(MySteps));

// Check pattern matches exactly
[When("I do something")]  // Pattern
// Text: "I do something"  // Must match exactly
```

### Parameter Extraction Failed

**Error:** `Cannot convert value to type`

**Cause:** Text doesn't convert to parameter type

**Solution:**
```csharp
// Text: "I have 5 items"
[Given("I have {count} items")]
public async Task HaveItems(int count)  // ✅ Converts "5" to int
{
    Assert.IsType<int>(count);
}

// Wrong:
// [Given("I have {count} items")]
// public async Task HaveItems(string count)  // ❌ Type mismatch
```

### Scenario Not Executed

**Error:** Feature file not found or not parsed correctly

**Solution:**
```csharp
// Ensure path is correct
var feature = await loader.LoadFromFileAsync(
    "features/my-feature.feature");  // ✅ Correct path

// Check file format
// Feature: My Feature        ✅ Correct
// Scenario: My Scenario      ✅ Correct
//   Given something

// Feature My Feature         ❌ Missing colon
// Scenario My Scenario       ❌ Missing colon
```

### Database State Not Reset

**Error:** Data persists between scenarios

**Solution:**
```csharp
// Implement IResettable
public class MyRepository : IResettable
{
    private List<Item> _items = new();

    public async Task ResetAsync()
    {
        _items.Clear();
        await Task.CompletedTask;
    }
}

// Register for reset
protected override void ConfigureServices(IServiceCollection services)
{
    var repo = new MyRepository();
    services.AddSingleton<IRepository>(repo);
    RegisterResettableService(repo);  // ✅ Enable reset
}

// Call reset in test
await ResetDatabaseAsync();  // ✅ Clears repo
```

---

## Examples in Repository

See complete working examples:

- `samples/PeasyPilot.XUnit.Samples/features/users.feature`
- `samples/PeasyPilot.XUnit.Samples/features/orders.feature`
- `samples/PeasyPilot.XUnit.Samples/StepDefinitions/UserSteps.cs`
- `samples/PeasyPilot.XUnit.Samples/StepDefinitions/OrderSteps.cs`
- `tests/PeasyPilot.Core.Tests/BDD/StepBindingResolverTests.cs`

---

## Running BDD Tests

```bash
# All BDD tests
dotnet test

# Specific feature tests
dotnet test --filter "UserBddTests"

# Step binding tests
dotnet test --filter "StepBindingResolver"

# With detailed output
dotnet test --verbosity detailed

# Single framework
dotnet test --filter "net8.0"
```

---

## Summary

1. **Create feature file** with scenarios
2. **Write step definitions** extending `BddStepDefinition`
3. **Register steps** in resolver
4. **Execute scenarios** via `ScenarioExecutor`
5. **Assert results** and reset state between scenarios

PeasyPilot handles:
- ✅ Feature file parsing
- ✅ Step discovery (reflection)
- ✅ Pattern matching (regex)
- ✅ Parameter extraction
- ✅ Scenario execution

You provide:
- ✅ Feature files (Gherkin)
- ✅ Step implementations
- ✅ Test fixtures/assertions
