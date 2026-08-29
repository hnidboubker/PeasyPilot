# PeasyPilot Samples

This directory contains sample projects demonstrating how to use PeasyPilot with different testing frameworks.

## Project Structure

### PeasyPilot.XUnit.Samples
Examples using the xUnit testing framework.

**Features demonstrated:**
- `PeasyPilotTestBase` inheritance
- `IAsyncLifetime` lifecycle management
- Theory tests with inline data
- Fluent test organization
- Integration with Bogus data factories

**Test files:**
- `UserServiceXUnitTests.cs` - Basic xUnit tests with a simple User service
- `UserBuilderXUnitTests.cs` - Builder pattern demonstration

### PeasyPilot.NUnit.Samples
Examples using the NUnit testing framework.

**Features demonstrated:**
- `PeasyPilotNUnitTestBase` inheritance
- `[SetUp]` and `[TearDown]` lifecycle
- `[TestCase]` parametrized tests
- NUnit-style assertions
- Fixture-based test organization

**Test files:**
- `InventoryServiceNUnitTests.cs` - Product inventory management tests
- Product and inventory service domain models

### PeasyPilot.TUnit.Samples
Examples using the TUnit testing framework.

**Features demonstrated:**
- `PeasyPilotTUnitTestBase` inheritance
- Async ValueTask lifecycle (`BeforeEachAsync`, `AfterEachAsync`)
- `[Arguments]` attribute for parametrized tests
- Parallel test execution with `[Parallelizable]`
- Modern async/await patterns

**Test files:**
- `OrderServiceTUnitTests.cs` - Order management workflow tests
- Order domain model with complete workflow demonstration

## Running the Samples

### Run all samples
```bash
cd samples
dotnet test
```

### Run specific sample project
```bash
# xUnit samples
dotnet test PeasyPilot.XUnit.Samples

# NUnit samples
dotnet test PeasyPilot.NUnit.Samples

# TUnit samples
dotnet test PeasyPilot.TUnit.Samples
```

### Run specific test class
```bash
dotnet test PeasyPilot.XUnit.Samples --filter "UserServiceXUnitTests"
```

### Run with verbosity
```bash
dotnet test PeasyPilot.XUnit.Samples --verbosity detailed
```

## Key Concepts Demonstrated

### 1. Base Test Classes
Each framework has a specialized base class:

**xUnit:**
```csharp
public class MyTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync() { ... }
    public override async Task DisposeAsync() { ... }
}
```

**NUnit:**
```csharp
public class MyTests : PeasyPilotNUnitTestBase
{
    [SetUp]
    public override void Setup() { ... }
}
```

**TUnit:**
```csharp
public class MyTests : PeasyPilotTUnitTestBase
{
    public override async ValueTask BeforeEachAsync() { ... }
}
```

### 2. Test Organization
Each sample demonstrates:
- Clear Arrange/Act/Assert pattern
- Descriptive test method names
- Domain models relevant to the scenario
- Service/business logic classes

### 3. Parametrized Tests

**xUnit:**
```csharp
[Theory]
[InlineData("test@example.com")]
public void Test_WithData(string email) { ... }
```

**NUnit:**
```csharp
[TestCase("value1")]
[TestCase("value2")]
public void Test_WithData(string value) { ... }
```

**TUnit:**
```csharp
[Test]
[Arguments("value1")]
[Arguments("value2")]
public async Task Test_WithData(string value) { ... }
```

### 4. Assertions

**xUnit (using standard assertions):**
```csharp
Assert.NotNull(result);
Assert.Equal(expected, result);
```

**NUnit (using Is syntax):**
```csharp
Assert.That(result, Is.Not.Null);
Assert.That(result, Is.EqualTo(expected));
```

**TUnit (similar to NUnit):**
```csharp
Assert.That(result, Is.Not.Null);
Assert.That(result, Is.EqualTo(expected));
```

## Sample Scenarios

### xUnit Samples
- **UserServiceXUnitTests**: CRUD operations (Create, Read, Update, Delete)
  - Creating users
  - Retrieving by ID
  - Listing all users
  - Deleting users
  - Theory tests with different data

- **UserBuilderXUnitTests**: Builder pattern for object construction
  - Fluent API usage
  - Chaining methods
  - Resetting builder state
  - Minimal vs. complete configuration

### NUnit Samples
- **InventoryServiceNUnitTests**: Inventory management system
  - Adding products
  - Updating quantities
  - Calculating total inventory value
  - Removing products
  - TestCase parametrization

### TUnit Samples
- **OrderServiceTUnitTests**: Order processing workflow
  - Creating orders
  - Adding items to orders
  - Calculating order totals
  - Status transitions (Pending → Confirmed → Shipped → Delivered)
  - Complete workflow test
  - Parallel test execution
  - Arguments-based parametrization

## Dependencies

All samples include:
- `PeasyPilot.Core` - Core framework
- Framework-specific adapter (XUnit, NUnit, or TUnit)
- `PeasyPilot.Bogus` - Fake data generation
- `PeasyPilot.Moq` - Mocking support
- `PeasyPilot.Unit` - Builder pattern utilities

## Extending the Samples

To add your own tests:

1. Create a new test class inheriting from the appropriate base class
2. Use domain models defined in the sample project
3. Follow the Arrange/Act/Assert pattern
4. Add documentation to test methods

Example:
```csharp
public class CustomTests : PeasyPilotTestBase
{
    [Fact]
    public void MyTest_WithExpectedBehavior_ProducesExpectedResult()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

## Framework Comparison

| Feature | xUnit | NUnit | TUnit |
|---------|-------|-------|-------|
| Parallel by Default | Yes | No | Yes |
| Attribute Style | `[Fact]`, `[Theory]` | `[Test]`, `[TestCase]` | `[Test]`, `[Arguments]` |
| Lifecycle | IAsyncLifetime | SetUp/TearDown | BeforeEachAsync/AfterEachAsync |
| Async Support | Full | Limited | Full (ValueTask) |
| Learning Curve | Medium | Low | Low |

## Best Practices

1. **Use descriptive names**: Test method names should describe the scenario
2. **Follow AAA pattern**: Arrange, Act, Assert
3. **One assertion focus**: Each test should verify one behavior
4. **Use builders**: For complex object creation
5. **Organize tests**: Group related tests in classes
6. **Use base classes**: Leverage PeasyPilot base classes for common setup

## Troubleshooting

### Tests not discovered
- Ensure project is marked as `<IsTestProject>true</IsTestProject>`
- Verify test class and method have correct attributes

### Build errors
- Check project references are correct
- Ensure `Directory.Packages.props` has all required packages

### Assertion failures
- Use descriptive assertion messages
- Check test data setup
- Verify service implementation

## References

- [xUnit Documentation](https://xunit.net/)
- [NUnit Documentation](https://docs.nunit.org/)
- [TUnit Documentation](https://thomhurst.github.io/TUnit/)
- [PeasyPilot Documentation](../README.md)
