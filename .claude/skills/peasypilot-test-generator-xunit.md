# PeasyPilot Test Generator - xUnit

Specialized test generation skill for **xUnit** projects using PeasyPilot. Analyzes code, generates comprehensive test suites with xUnit-specific patterns, and validates output.

**Trigger:** Use when the user asks to:
- Generate xUnit tests with PeasyPilot
- Create xUnit test suite
- Say "Génère des tests xUnit"
- Work specifically with xUnit framework

---

## Prerequisites

- Project uses **xUnit** test framework
- Project has or will add `PeasyPilot.XUnit` package
- Target code is in `src/` directory (typical layout)
- Test project follows pattern `tests/ProjectName.Tests`

---

## Workflow

```
User Selection (xUnit context)
    ↓
Analyze Target Code
    ↓
Build xUnit-Specific Test Matrix
    ↓
Generate Tests (xUnit patterns)
    ↓
Use PeasyPilot.XUnit Base Classes
    ↓
Build & Compile
    ↓
Execute Tests
    ↓
Report Results
```

---

## Step 1: Confirm xUnit Context

Before proceeding, verify:
- Project references `xunit` package
- Test project exists or will be created
- Project structure is standard (`src/`, `tests/`)
- User wants xUnit, not NUnit or TUnit

---

## Step 2: Analyze Target Code

Inspect the code being tested:
- Public methods and their signatures
- Constructor dependencies
- Return types and nullability
- Async/await patterns
- Exception handling
- Business logic branching

**Key for xUnit:** Look for state changes, dependencies, and test-relevant edge cases.

---

## Step 3: xUnit Test Class Template

All xUnit tests inherit from `PeasyPilotTestBase`:

```csharp
using Xunit;
using PeasyPilot.XUnit;
using Assert = PeasyPilot.Core.Assertions.Assert;

public class UserServiceTests : PeasyPilotTestBase
{
    private UserService _sut = default!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _sut = new UserService();
    }

    [Fact]
    public async Task MethodName_Condition_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert
        await Task.CompletedTask;
    }
}
```

**xUnit Patterns:**
- `[Fact]` for parameterless tests
- `[Theory]` + `[InlineData(...)]` for parameterized tests
- Inherits `PeasyPilotTestBase` for setup/teardown
- Uses fluent `Assert.That(...)` from PeasyPilot
- Always `async Task` for lifecycle compatibility

---

## Step 4: Build Test Matrix (xUnit-Specific)

For each public method, create:

### Happy Paths
```csharp
[Fact]
public async Task Create_WithValidData_ReturnsUser()
{
    var user = new TestDataFactory().Create<User>();
    var result = _sut.Create(user);
    Assert.That(result).IsNotNull();
}
```

### Boundary / Edge Cases
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(int.MaxValue)]
public async Task GetById_WithSpecialValues_Handles(int id)
{
    var result = _sut.GetById(id);
    Assert.That(result).IsNotNull();
}
```

### Exception Cases
```csharp
[Fact]
public async Task Delete_WithInvalidId_Throws()
{
    var ex = await Assert.ThrowsAsync<ArgumentException>(
        () => _sut.Delete(-1)
    );
    Assert.That(ex.Message).Contains("invalid");
}
```

### Dependency Failures
```csharp
[Fact]
public async Task Create_WhenRepositoryFails_PropagatesException()
{
    var mockRepo = new MockFactory().Create<IUserRepository>();
    mockRepo.Setup(r => r.Add(It.IsAny<User>()))
        .ThrowsAsync(new InvalidOperationException("DB error"));
    
    var sut = new UserService(mockRepo);
    var ex = await Assert.ThrowsAsync<InvalidOperationException>(
        () => sut.Create(new User())
    );
}
```

---

## Step 5: Generate Test Data (xUnit Context)

Use `PeasyPilot.Bogus`:

```csharp
var factory = new TestDataFactory();
var user = factory.Create<User>();
var users = factory.CreateMany<User>(5);
```

For xUnit, always use `async Task` even if test logic is synchronous.

---

## Step 6: Generate Mocks (xUnit Context)

Use `PeasyPilot.Moq`:

```csharp
var mockFactory = new MockFactory();
var repository = mockFactory.Create<IUserRepository>();
```

Set up behavior:
```csharp
repository.Setup(r => r.GetById(1))
    .ReturnsAsync(new User { Id = 1, Name = "Test" });
```

---

## Step 7: xUnit-Specific Assertions

Use PeasyPilot fluent API:

```csharp
Assert.That(result)
    .IsNotNull()
    .IsEqualTo(expected);

Assert.That(collection)
    .Contains(item)
    .HasCount(5);

Assert.That(value)
    .IsGreaterThan(0)
    .IsLessThanOrEqualTo(100);
```

---

## Step 8: File Generation

Create tests in:
```
tests/
└── ProjectName.Tests/
    └── Services/
        └── UserServiceTests.cs
```

Follow xUnit naming: `[ClassName]Tests.cs`

---

## Step 9: Build & Validate

```bash
dotnet build <solution>
```

Fix any compilation errors in generated tests (not in production code).

---

## Step 10: Execute Tests

```bash
dotnet test <test-project>
```

Or use PeasyPilot CLI:
```bash
peasypilot --filter UserServiceTests
```

---

## Step 11: Report

Provide summary:
- Tests generated: X
- Tests passing: X
- Tests failing: X
- Missing coverage: [list gaps]
- Next steps: [if any]

---

## xUnit-Specific Tips

1. **Always async Task** - even if logic is sync, use `await Task.CompletedTask`
2. **[Fact] vs [Theory]** - use Theory for parameterized, Fact for simple
3. **Inheritance** - all classes must inherit `PeasyPilotTestBase`
4. **Disposal** - let base class handle IAsyncLifetime
5. **No constructors** - use `InitializeAsync()` instead
6. **Parallel execution** - xUnit runs tests in parallel by default (safe if tests don't share state)

---

## When to Use This Skill

- Project uses **xUnit** as test framework
- Need test scaffolding for classes/services
- Want PeasyPilot-style fluent assertions
- Need mocked dependencies (Moq integration)
- Need test data generation (Bogus integration)

When framework is unknown or user wants auto-detection, use the **generic skill** instead.

---

## Implementation Notes

This skill is a specialized variant of `peasypilot-test-generator`. It:

1. Assumes xUnit environment (no framework detection)
2. Uses `PeasyPilot.XUnit` base classes exclusively
3. Follows xUnit async/await patterns strictly
4. Generates [Fact] and [Theory] attributes only
5. Stops at `READY_FOR_COMMIT` without pushing
6. Reports test coverage gaps clearly

The skill does NOT:
- Force xUnit if user wants another framework
- Modify production code
- Commit or push changes
- Create new projects without asking
