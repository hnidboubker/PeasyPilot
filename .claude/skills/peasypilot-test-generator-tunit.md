# PeasyPilot Test Generator - TUnit

Specialized test generation skill for **TUnit** projects using PeasyPilot. Analyzes code, generates comprehensive test suites with TUnit-specific patterns, and validates output.

**Trigger:** Use when the user asks to:
- Generate TUnit tests with PeasyPilot
- Create TUnit test suite
- Say "Génère des tests TUnit"
- Work specifically with TUnit framework

---

## Prerequisites

- Project uses **TUnit** test framework
- Project has or will add `PeasyPilot.TUnit` package
- Target code is in `src/` directory (typical layout)
- Test project follows pattern `tests/ProjectName.Tests`

---

## Workflow

```
User Selection (TUnit context)
    ↓
Analyze Target Code
    ↓
Build TUnit-Specific Test Matrix
    ↓
Generate Tests (TUnit patterns)
    ↓
Use PeasyPilot.TUnit Base Classes
    ↓
Build & Compile
    ↓
Execute Tests
    ↓
Report Results
```

---

## Step 1: Confirm TUnit Context

Before proceeding, verify:
- Project references `tunit` package
- Test project exists or will be created
- Project structure is standard (`src/`, `tests/`)
- User wants TUnit, not xUnit or NUnit

---

## Step 2: Analyze Target Code

Inspect the code being tested:
- Public methods and their signatures
- Constructor dependencies
- Return types and nullability
- Async/await patterns
- Exception handling
- Business logic branching

**Key for TUnit:** Look for state changes, dependencies, and test-relevant edge cases.

---

## Step 3: TUnit Test Class Template

All TUnit tests inherit from `PeasyPilotTestBase`:

```csharp
using TUnit.Core;
using PeasyPilot.TUnit;
using Assert = PeasyPilot.Core.Assertions.Assert;

public class UserServiceTests : PeasyPilotTestBase
{
    private UserService _sut = default!;

    [Before(Test)]
    public async Task BeforeEach()
    {
        await InitializeAsync();
        _sut = new UserService();
    }

    [Test]
    public async Task MethodName_Condition_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert
        await Task.CompletedTask;
    }
}
```

**TUnit Patterns:**
- No `[TestClass]` attribute (TUnit auto-discovers)
- `[Before(Test)]` for per-test initialization
- `[Test]` for each test method
- `[Arguments(...)]` for parameterized tests
- Inherits `PeasyPilotTestBase` for lifecycle
- Uses fluent `Assert.That(...)` from PeasyPilot
- Test methods must be `async Task`
- TUnit is modern and minimal - less boilerplate

---

## Step 4: Build Test Matrix (TUnit-Specific)

For each public method, create:

### Happy Paths
```csharp
[Test]
public async Task Create_WithValidData_ReturnsUser()
{
    var user = new TestDataFactory().Create<User>();
    var result = _sut.Create(user);
    Assert.That(result).IsNotNull();
}
```

### Boundary / Edge Cases
```csharp
[Arguments(0)]
[Arguments(-1)]
[Arguments(int.MaxValue)]
[Test]
public async Task GetById_WithSpecialValues_Handles(int id)
{
    var result = _sut.GetById(id);
    Assert.That(result).IsNotNull();
}
```

### Exception Cases
```csharp
[Test]
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
[Test]
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

## Step 5: Generate Test Data (TUnit Context)

Use `PeasyPilot.Bogus`:

```csharp
var factory = new TestDataFactory();
var user = factory.Create<User>();
var users = factory.CreateMany<User>(5);
```

For TUnit, always use `async Task` even if test logic is synchronous.

---

## Step 6: Generate Mocks (TUnit Context)

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

## Step 7: TUnit-Specific Assertions

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

Follow TUnit naming: `[ClassName]Tests.cs`

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

## TUnit-Specific Tips

1. **No [TestClass]** - TUnit auto-discovers test classes
2. **[Before(Test)] not [SetUp]** - TUnit uses modern attribute style
3. **[Test] vs [Arguments]** - use Arguments for parameterized, Test for simple
4. **Minimal boilerplate** - TUnit is designed to be lightweight
5. **Inheritance** - all classes must inherit `PeasyPilotTestBase`
6. **Always async Task** - maintain consistency with async lifecycle
7. **Modern patterns** - TUnit is built for .NET 8+
8. **Parallel execution** - TUnit runs tests in parallel by default

---

## When to Use This Skill

- Project uses **TUnit** as test framework
- Need test scaffolding for classes/services
- Want PeasyPilot-style fluent assertions
- Need mocked dependencies (Moq integration)
- Need test data generation (Bogus integration)
- Want modern, minimal test framework

When framework is unknown or user wants auto-detection, use the **generic skill** instead.

---

## Implementation Notes

This skill is a specialized variant of `peasypilot-test-generator`. It:

1. Assumes TUnit environment (no framework detection)
2. Uses `PeasyPilot.TUnit` base classes exclusively
3. Follows TUnit modern patterns strictly
4. Generates [Test] and [Arguments] attributes only
5. Stops at `READY_FOR_COMMIT` without pushing
6. Reports test coverage gaps clearly

The skill does NOT:
- Force TUnit if user wants another framework
- Modify production code
- Commit or push changes
- Create new projects without asking
