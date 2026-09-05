# PeasyPilot Test Generator - NUnit

Specialized test generation skill for **NUnit** projects using PeasyPilot. Analyzes code, generates comprehensive test suites with NUnit-specific patterns, and validates output.

**Trigger:** Use when the user asks to:
- Generate NUnit tests with PeasyPilot
- Create NUnit test suite
- Say "Génère des tests NUnit"
- Work specifically with NUnit framework

---

## Prerequisites

- Project uses **NUnit 4.x** test framework
- Project has or will add `PeasyPilot.NUnit` package
- Target code is in `src/` directory (typical layout)
- Test project follows pattern `tests/ProjectName.Tests`

---

## Workflow

```
User Selection (NUnit context)
    ↓
Analyze Target Code
    ↓
Build NUnit-Specific Test Matrix
    ↓
Generate Tests (NUnit patterns)
    ↓
Use PeasyPilot.NUnit Base Classes
    ↓
Build & Compile
    ↓
Execute Tests
    ↓
Report Results
```

---

## Step 1: Confirm NUnit Context

Before proceeding, verify:
- Project references `nunit` package (version 4.x preferred)
- Test project exists or will be created
- Project structure is standard (`src/`, `tests/`)
- User wants NUnit, not xUnit or TUnit

---

## Step 2: Analyze Target Code

Inspect the code being tested:
- Public methods and their signatures
- Constructor dependencies
- Return types and nullability
- Async/await patterns
- Exception handling
- Business logic branching

**Key for NUnit:** Look for state changes, dependencies, and test-relevant edge cases.

---

## Step 3: NUnit Test Class Template

All NUnit tests inherit from `PeasyPilotTestBase`:

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;
using Assert = PeasyPilot.Core.Assertions.Assert;

[TestFixture]
public class UserServiceTests : PeasyPilotTestBase
{
    private UserService _sut = default!;

    [SetUp]
    public async Task Setup()
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

**NUnit Patterns:**
- `[TestFixture]` on class (required)
- `[SetUp]` for initialization (instead of constructor)
- `[Test]` for each test method
- `[TestCase(...)]` for parameterized tests
- Inherits `PeasyPilotTestBase` for lifecycle
- Uses fluent `Assert.That(...)` from PeasyPilot
- Test methods must be `async Task`

---

## Step 4: Build Test Matrix (NUnit-Specific)

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
[TestCase(0)]
[TestCase(-1)]
[TestCase(int.MaxValue)]
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

## Step 5: Generate Test Data (NUnit Context)

Use `PeasyPilot.Bogus`:

```csharp
var factory = new TestDataFactory();
var user = factory.Create<User>();
var users = factory.CreateMany<User>(5);
```

For NUnit, always use `async Task` even if test logic is synchronous.

---

## Step 6: Generate Mocks (NUnit Context)

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

## Step 7: NUnit-Specific Assertions

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

Follow NUnit naming: `[ClassName]Tests.cs`

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

## NUnit-Specific Tips

1. **[TestFixture] required** - class must have this attribute
2. **[SetUp] not constructor** - use [SetUp] for initialization
3. **[Test] vs [TestCase]** - use TestCase for parameterized, Test for simple
4. **Inheritance** - all classes must inherit `PeasyPilotTestBase`
5. **Always async Task** - maintain consistency with async lifecycle
6. **No [TearDown] needed** - base class handles cleanup via Dispose
7. **Sequential by default** - NUnit runs tests sequentially unless configured otherwise

---

## When to Use This Skill

- Project uses **NUnit 4.x** as test framework
- Need test scaffolding for classes/services
- Want PeasyPilot-style fluent assertions
- Need mocked dependencies (Moq integration)
- Need test data generation (Bogus integration)

When framework is unknown or user wants auto-detection, use the **generic skill** instead.

---

## Implementation Notes

This skill is a specialized variant of `peasypilot-test-generator`. It:

1. Assumes NUnit environment (no framework detection)
2. Uses `PeasyPilot.NUnit` base classes exclusively
3. Follows NUnit [TestFixture] and [SetUp] patterns strictly
4. Generates [Test] and [TestCase] attributes only
5. Stops at `READY_FOR_COMMIT` without pushing
6. Reports test coverage gaps clearly

The skill does NOT:
- Force NUnit if user wants another framework
- Modify production code
- Commit or push changes
- Create new projects without asking
