# PeasyPilot.BDD API Reference

## Overview

`PeasyPilot.BDD` provides a comprehensive Behavior-Driven Development (BDD) framework for writing executable specifications in Gherkin syntax. It enables you to express test scenarios in human-readable Given-When-Then format, load feature files from disk, and execute scenarios with full step binding support and dependency injection integration.

**Key Responsibilities:**
- Gherkin feature and scenario modeling
- Feature file loading from disk (single files and directories)
- Gherkin text parsing into Feature object graphs
- Scenario execution with step binding resolution
- Step definition discovery and pattern matching via attributes
- Living documentation export (Markdown)
- Integration with PeasyPilot.Core test models

**Targets:** .NET 8.0, 9.0, 10.0

---

## Main Abstractions

### IFeatureFileLoader

Loads Gherkin feature files from disk and converts them to Feature objects.

```csharp
namespace PeasyPilot.BDD.FileLoading;

/// <summary>
/// Loads Gherkin feature files from disk and converts them to Feature objects.
/// </summary>
public interface IFeatureFileLoader
{
    /// <summary>
    /// Loads all .feature files from a directory recursively.
    /// </summary>
    /// <param name="directoryPath">Path to features directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of loaded features.</returns>
    Task<IReadOnlyList<Feature>> LoadFromDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single .feature file.
    /// </summary>
    /// <param name="filePath">Path to feature file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Loaded feature.</returns>
    Task<Feature> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
```

**Purpose:** Provides framework-agnostic feature file loading. The implementation (`GherkinFeatureFileLoader`) scans directories recursively and parses .feature files into Feature object graphs.

---

### IScenarioExecutor

Executes BDD scenarios step-by-step with optional step binding resolution.

```csharp
namespace PeasyPilot.BDD.Execution;

/// <summary>
/// Executes BDD scenarios with dependency injection support.
/// </summary>
public interface IScenarioExecutor
{
    /// <summary>
    /// Executes a scenario using the provided service provider for step resolution.
    /// </summary>
    /// <param name="scenario">Scenario to execute.</param>
    /// <param name="serviceProvider">Service provider for step definitions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution result with step details.</returns>
    Task<ScenarioExecutionResult> ExecuteAsync(
        Scenario scenario,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}
```

**Purpose:** Executes scenarios step-by-step, resolving step bindings via attributes, handling errors gracefully, and tracking execution results.

---

### IStepBindingResolver

Discovers and resolves step definition attributes to executable actions.

```csharp
namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Resolves step binding attributes to executable actions.
/// </summary>
public interface IStepBindingResolver
{
    /// <summary>
    /// Resolves a step text to an executable action.
    /// </summary>
    /// <param name="stepType">The type of step (Given, When, Then).</param>
    /// <param name="stepText">The step text to match.</param>
    /// <param name="serviceProvider">Service provider for dependency resolution.</param>
    /// <returns>An executable Func&lt;Task&gt; or null if no match found.</returns>
    Func<Task>? ResolveStep(StepType stepType, string stepText, IServiceProvider serviceProvider);
}
```

**Purpose:** Discovers step definition classes via reflection, matches step text patterns against [Given], [When], [Then] attributes, and returns executable step actions with parameter extraction.

---

### GherkinFeatureParser

Native Gherkin syntax parser without external dependencies.

```csharp
namespace PeasyPilot.BDD;

/// <summary>
/// Native Gherkin feature text parser converting Gherkin specifications into PeasyPilot Feature instances.
/// </summary>
public static class GherkinFeatureParser
{
    /// <summary>
    /// Parses a Gherkin specification string into a Feature object graph.
    /// </summary>
    /// <param name="gherkinContent">Gherkin text content.</param>
    /// <returns>Parsed Feature instance.</returns>
    public static Feature Parse(string gherkinContent);
}
```

**Purpose:** Parses Gherkin syntax into Feature → Scenario → Step object graphs without external dependencies. Handles Feature, Scenario, Given, When, Then, And, But keywords.

---

## Core Models

### Feature

Container for related BDD scenarios.

```csharp
public class Feature
{
    /// <summary>
    /// Gets the feature name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the read-only list of scenarios.
    /// </summary>
    public IReadOnlyList<Scenario> Scenarios { get; }

    /// <summary>
    /// Initializes a new Feature.
    /// </summary>
    public Feature(string name);

    /// <summary>
    /// Adds a scenario to this feature.
    /// </summary>
    public Scenario AddScenario(string name);

    /// <summary>
    /// Executes all scenarios in the feature asynchronously.
    /// </summary>
    public Task ExecuteAsync();

    /// <summary>
    /// Converts scenarios to TestRunResult.
    /// </summary>
    public Task<TestRunResult> ExecuteAndAsTestRunResultAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts scenarios to TestCase objects for orchestration.
    /// </summary>
    public IReadOnlyCollection<TestCase> ToTestCases();
}
```

---

### Scenario

Individual test scenario with Given-When-Then steps.

```csharp
public class Scenario
{
    /// <summary>
    /// Gets the scenario name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the steps in execution order.
    /// </summary>
    public IReadOnlyList<Step> Steps { get; }

    /// <summary>
    /// Initializes a new Scenario.
    /// </summary>
    public Scenario(string name);

    /// <summary>
    /// Adds a Given step (context setup).
    /// </summary>
    public Scenario Given(string stepText);

    /// <summary>
    /// Adds a When step (action).
    /// </summary>
    public Scenario When(string stepText);

    /// <summary>
    /// Adds a Then step (assertion).
    /// </summary>
    public Scenario Then(string stepText);

    /// <summary>
    /// Adds an And step (continuation).
    /// </summary>
    public Scenario And(string stepText);

    /// <summary>
    /// Adds a But step (negation).
    /// </summary>
    public Scenario But(string stepText);

    /// <summary>
    /// Executes all steps asynchronously.
    /// </summary>
    public Task ExecuteAsync();

    /// <summary>
    /// Executes and returns TestResult for integration.
    /// </summary>
    public Task<TestResult> ExecuteAndAsTestResultAsync(string featureName);
}
```

---

### Step

Individual step with execution logic and validation.

```csharp
public class Step
{
    /// <summary>
    /// Gets the step type (Given, When, Then, And, But).
    /// </summary>
    public StepType Type { get; }

    /// <summary>
    /// Gets the step text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets or sets the execution action.
    /// </summary>
    public Func<Task>? Execute { get; set; }

    /// <summary>
    /// Gets or sets the validation function for assertions.
    /// </summary>
    public Func<bool>? ExecuteValidation { get; set; }

    /// <summary>
    /// Executes the step action asynchronously.
    /// </summary>
    public Task ExecuteAsync();
}
```

---

### ScenarioExecutionResult

Result of executing a scenario including all step outcomes.

```csharp
public class ScenarioExecutionResult
{
    /// <summary>
    /// Gets or sets the scenario name.
    /// </summary>
    public string ScenarioName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall status.
    /// </summary>
    public ScenarioStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the individual step results.
    /// </summary>
    public List<StepExecutionResult> Steps { get; set; } = new();

    /// <summary>
    /// Gets or sets the execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the error message if scenario failed.
    /// </summary>
    public string? Error { get; set; }
}
```

---

### BddStepRegistry

Central registry for binding step text patterns to executable actions.

```csharp
public sealed class BddStepRegistry
{
    /// <summary>
    /// Registers a step pattern with an executable action.
    /// </summary>
    public void RegisterStep(string pattern, Func<Task> action);

    /// <summary>
    /// Finds a matching action for given step text.
    /// </summary>
    public Func<Task>? FindMatch(string stepText);
}
```

---

## Step Definition Attributes

Step definitions are declared using attributes on BddStepDefinition subclasses:

```csharp
namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Marks a method as a Given step (context setup).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class GivenAttribute : Attribute
{
    /// <summary>
    /// Gets the step pattern (supports {param} placeholders).
    /// </summary>
    public string Pattern { get; }

    public GivenAttribute(string pattern);
}

/// <summary>
/// Marks a method as a When step (action).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class WhenAttribute : Attribute
{
    /// <summary>
    /// Gets the step pattern (supports {param} placeholders).
    /// </summary>
    public string Pattern { get; }

    public WhenAttribute(string pattern);
}

/// <summary>
/// Marks a method as a Then step (assertion).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ThenAttribute : Attribute
{
    /// <summary>
    /// Gets the step pattern (supports {param} placeholders).
    /// </summary>
    public string Pattern { get; }

    public ThenAttribute(string pattern);
}
```

---

## Configuration & Patterns

### Feature File Format (Gherkin)

Feature files use standard Gherkin syntax:

```gherkin
Feature: User Account Management

Scenario: User can create new account
    Given a new user form is displayed
    When the user enters name "John Doe"
    And the user enters email "john@example.com"
    And the user submits the form
    Then the account is created successfully
    And the user receives a confirmation email
```

### Step Definition Patterns

Step patterns support parameter extraction via curly braces:

```csharp
[Given("a user with name {name}")]
public void CreateUser(string name) { }

[When("the user enters {count:int} items")]
public void AddItems(int count) { }

[Then("the total is {amount:decimal}")]
public void ValidateTotal(decimal amount) { }
```

Parameter types supported:
- `{name}` - string (default)
- `{count:int}` - integer
- `{amount:decimal}` - decimal
- `{flag:bool}` - boolean

---

## Working Examples

### Example 1: Basic Feature Definition and Execution

```csharp
using PeasyPilot.BDD;

public class BasicFeatureTest
{
    [Fact]
    public async Task TestBasicFeature()
    {
        var feature = new Feature("User Login");
        
        feature.AddScenario("Successful login")
            .Given("user has valid credentials")
            .When("user submits login form")
            .Then("user is redirected to dashboard");

        feature.AddScenario("Invalid password")
            .Given("user has invalid password")
            .When("user submits login form")
            .Then("error message is displayed");

        // Execute all scenarios
        await feature.ExecuteAsync();

        // Convert to test cases for orchestration
        var testCases = feature.ToTestCases();
        Assert.NotEmpty(testCases);
    }
}
```

### Example 2: Parsing Gherkin Text

```csharp
using PeasyPilot.BDD;

public class GherkinParsingTest
{
    [Fact]
    public void TestParseGherkinFeature()
    {
        var gherkinText = @"
Feature: Shopping Cart
    Scenario: Add item to cart
        Given the user is on the product page
        When the user clicks add to cart
        Then the item appears in the cart
        And the cart count increases
";

        var feature = GherkinFeatureParser.Parse(gherkinText);

        Assert.Equal("Shopping Cart", feature.Name);
        Assert.Single(feature.Scenarios);
        
        var scenario = feature.Scenarios.First();
        Assert.Equal("Add item to cart", scenario.Name);
        Assert.Equal(4, scenario.Steps.Count);
    }
}
```

### Example 3: Loading Feature Files

```csharp
using PeasyPilot.BDD.FileLoading;

public class FeatureFileLoaderTest
{
    private readonly IFeatureFileLoader _loader = new GherkinFeatureFileLoader();

    [Fact]
    public async Task TestLoadFeaturesFromDirectory()
    {
        var featuresPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Features");

        var features = await _loader.LoadFromDirectoryAsync(featuresPath);

        Assert.NotEmpty(features);
        
        foreach (var feature in features)
        {
            Assert.NotNull(feature.Name);
            Assert.NotEmpty(feature.Scenarios);
        }
    }

    [Fact]
    public async Task TestLoadSingleFeatureFile()
    {
        var featurePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Features",
            "UserManagement.feature");

        var feature = await _loader.LoadFromFileAsync(featurePath);

        Assert.NotNull(feature);
        Assert.NotEmpty(feature.Scenarios);
    }
}
```

### Example 4: Step Definitions with Attributes

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class UserStepDefinitions : BddStepDefinition
{
    private string? _currentUserName;
    private bool _creationSucceeded;

    [Given("a user database is empty")]
    public Task DatabaseIsEmpty()
    {
        // Setup code
        return Task.CompletedTask;
    }

    [Given("a user with name {name}")]
    public Task CreateUser(string name)
    {
        _currentUserName = name;
        return Task.CompletedTask;
    }

    [When("the system validates the name")]
    public Task ValidateName()
    {
        _creationSucceeded = !string.IsNullOrEmpty(_currentUserName) 
            && _currentUserName.Length > 2;
        return Task.CompletedTask;
    }

    [Then("the user is accepted")]
    public void UserIsAccepted()
    {
        Assert.True(_creationSucceeded);
    }
}
```

### Example 5: Executing Scenarios with Step Binding

```csharp
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.StepDefinitions;
using Microsoft.Extensions.DependencyInjection;

public class ScenarioExecutionTest
{
    [Fact]
    public async Task TestExecuteScenarioWithBindings()
    {
        // Setup DI container
        var services = new ServiceCollection();
        services.AddScoped<UserStepDefinitions>();
        var serviceProvider = services.BuildServiceProvider();

        // Create scenario
        var scenario = new Scenario("Create valid user")
            .Given("a user database is empty")
            .Given("a user with name John Doe")
            .When("the system validates the name")
            .Then("the user is accepted");

        // Setup step binding resolver
        var resolver = new StepBindingResolver();
        var executor = new ScenarioExecutor(resolver);

        // Execute with step binding resolution
        var result = await executor.ExecuteAsync(
            scenario,
            serviceProvider);

        Assert.Equal(ScenarioStatus.Passed, result.Status);
        Assert.All(result.Steps, step => Assert.True(step.Passed));
    }
}
```

### Example 6: Living Documentation Export

```csharp
using PeasyPilot.BDD;

public class LivingDocumentationTest
{
    [Fact]
    public void TestExportAsLivingDoc()
    {
        var feature = new Feature("Order Management");
        
        feature.AddScenario("Create new order")
            .Given("customer has items in cart")
            .When("customer proceeds to checkout")
            .Then("order is created with unique ID")
            .And("confirmation email is sent");

        feature.AddScenario("Cancel order")
            .Given("order exists with status pending")
            .When("customer cancels order")
            .Then("order status changes to cancelled");

        var exporter = new LivingDocExporter();
        var markdown = exporter.Export(feature);

        Assert.Contains("Order Management", markdown);
        Assert.Contains("Create new order", markdown);
        Assert.Contains("Given", markdown);
        Assert.Contains("When", markdown);
        Assert.Contains("Then", markdown);
    }
}
```

### Example 7: Integration with PeasyPilot.Core

```csharp
using PeasyPilot.BDD;
using PeasyPilot.Core.Models;

public class BddCoreIntegrationTest
{
    [Fact]
    public async Task TestConvertScenarioToTestCase()
    {
        var feature = new Feature("API Endpoint Testing");
        
        feature.AddScenario("GET request returns 200")
            .Given("API server is running")
            .When("GET request is sent")
            .Then("response status is 200");

        // Convert to unified test case model
        var testCases = feature.ToTestCases();

        Assert.Single(testCases);
        var testCase = testCases.First();
        Assert.Equal("GET request returns 200", testCase.Name);
    }

    [Fact]
    public async Task TestConvertFeatureToTestRunResult()
    {
        var feature = new Feature("Payment Processing");
        
        feature.AddScenario("Process valid payment")
            .Given("customer has valid payment method")
            .When("customer submits payment")
            .Then("payment is processed");

        feature.AddScenario("Decline invalid payment")
            .Given("customer has invalid payment method")
            .When("customer submits payment")
            .Then("payment is declined");

        // Execute and get unified result
        var result = await feature.ExecuteAndAsTestRunResultAsync();

        Assert.NotNull(result);
        Assert.True(result.Passed > 0 || result.Failed > 0);
    }
}
```

### Example 8: Custom Step Pattern Matching

```csharp
using PeasyPilot.BDD;

public class CustomPatternMatchingTest
{
    [Fact]
    public void TestBddStepRegistry()
    {
        var registry = new BddStepRegistry();

        // Register steps with regex patterns
        registry.RegisterStep(
            @"^user enters name (.+)$",
            async () => { await Task.CompletedTask; });

        registry.RegisterStep(
            @"^(\d+) items are added$",
            async () => { await Task.CompletedTask; });

        // Match steps
        var match1 = registry.FindMatch("user enters name John Doe");
        Assert.NotNull(match1);

        var match2 = registry.FindMatch("5 items are added");
        Assert.NotNull(match2);

        var noMatch = registry.FindMatch("unknown step");
        Assert.Null(noMatch);
    }
}
```

### Example 9: Multi-Scenario Feature Execution

```csharp
using PeasyPilot.BDD;

public class MultiScenarioTest
{
    [Fact]
    public async Task TestExecuteMultipleScenariosInFeature()
    {
        var feature = new Feature("User Registration");

        // Scenario 1: Happy path
        feature.AddScenario("Register with valid data")
            .Given("registration page is open")
            .When("user enters valid email and password")
            .Then("account is created successfully");

        // Scenario 2: Invalid email
        feature.AddScenario("Register with invalid email")
            .Given("registration page is open")
            .When("user enters invalid email")
            .Then("validation error is shown");

        // Scenario 3: Password mismatch
        feature.AddScenario("Register with mismatched passwords")
            .Given("registration page is open")
            .When("user enters mismatched passwords")
            .Then("validation error is shown");

        // Execute all scenarios
        await feature.ExecuteAsync();

        Assert.Equal(3, feature.Scenarios.Count);
    }
}
```

### Example 10: Scenario with Complex Setup

```csharp
using PeasyPilot.BDD;
using PeasyPilot.Integration.Abstractions;

public class ComplexScenarioTest
{
    [Fact]
    public async Task TestScenarioWithComplexSetup()
    {
        var feature = new Feature("Inventory Management");

        var scenario = feature.AddScenario("Update inventory after sale")
            .Given("product exists with quantity 100")
            .And("product has minimum stock 10")
            .And("recent sale of 25 units recorded")
            .When("inventory is updated from sale")
            .And("minimum stock check is performed")
            .Then("new quantity is 75")
            .And("stock is above minimum")
            .And("stock warning is not triggered");

        Assert.Equal(8, scenario.Steps.Count);

        var thenSteps = scenario.Steps
            .Where(s => s.Type == StepType.Then)
            .ToList();

        Assert.Equal(3, thenSteps.Count);
    }
}
```

---

## Common Patterns

### Scenario Outline (Data-Driven Testing)

While PeasyPilot.BDD doesn't provide built-in scenario outline support like Cucumber, you can achieve similar results using xUnit theories:

```csharp
public class ScenarioOutlineExample
{
    private readonly GherkinFeatureParser _parser = new();

    [Theory]
    [InlineData("John", true)]
    [InlineData("Jane", true)]
    [InlineData("", false)]
    public async Task TestUserCreationWithVariousNames(string name, bool shouldSucceed)
    {
        var scenario = new Scenario($"Create user {name}")
            .Given($"user with name {name}");
        // ... continue building scenario
        // This pattern allows data-driven BDD execution
    }
}
```

### Background (Shared Setup)

Create helper methods for common setup steps:

```csharp
public class SharedSetupExample
{
    private Scenario CreateScenarioWithCommonBackground(string scenarioName)
    {
        return new Scenario(scenarioName)
            .Given("database is initialized")
            .Given("test user exists")
            .Given("authentication is enabled");
    }

    [Fact]
    public async Task TestScenario1()
    {
        var scenario = CreateScenarioWithCommonBackground("Test scenario 1");
        // ... add specific steps
    }
}
```

---

## Performance Considerations

- **Feature File Loading:** Recursive directory scanning scales to thousands of features
- **Parsing:** Native parser handles large Gherkin files efficiently
- **Step Binding:** Reflection-based discovery runs once during setup, execution is fast
- **Async/Await:** All operations support async patterns for non-blocking execution

---

## See Also

- [BDD Testing Guide](../GUIDES/bdd-testing-guide.md)
- [PeasyPilot.Core API](api-core.md)
- [PeasyPilot.Integration API](api-integration.md)
- [Step Binding Resolver Guide](../GUIDES/bdd-testing-guide.md#step-binding)

