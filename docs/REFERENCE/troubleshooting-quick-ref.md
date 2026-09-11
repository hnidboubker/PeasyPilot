# Troubleshooting Quick Reference

Common issues, symptoms, diagnosis steps, and solutions for PeasyPilot.

---

## 1. Build & Compilation Issues

### Symptom
```
error CS0103: The name 'X' does not exist in the current context
error NU1101: Unable to find package 'PeasyPilot.BDD' version 0.1.0
The build failed with exit code 1.
```

### Likely Causes
- Missing project reference to PeasyPilot package
- NuGet package source not configured
- Outdated NuGet cache
- Roslyn version mismatch
- Target framework not supported

### Diagnosis Steps
1. Check project references:
   ```bash
   dotnet list package --outdated
   ```
2. Verify package sources:
   ```bash
   dotnet nuget list source
   # Should include https://api.nuget.org/v3/index.json
   ```
3. Check target framework:
   ```bash
   # In .csproj, should be: <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```
4. View build details:
   ```bash
   dotnet build --verbose
   ```

### Solution
**For missing references:**
```bash
# Add reference to framework package
dotnet add reference ../src/PeasyPilot.Core/PeasyPilot.Core.csproj

# Or add NuGet package
dotnet add package PeasyPilot.BDD
```

**For NuGet source issues:**
```bash
# Add nuget.org if missing
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# Clear NuGet cache
dotnet nuget locals all --clear
```

**For Roslyn issues:**
```bash
# Update Roslyn if needed
dotnet add package Microsoft.CodeAnalysis.CSharp --version 4.9.2

# Verify version matches
grep "Microsoft.CodeAnalysis" *.csproj
```

### Prevention Tips
- Run `dotnet build` before running tests
- Use `dotnet restore` if adding new packages
- Keep Roslyn packages up-to-date
- Pin package versions in central package management

---

## 2. Test Discovery & Execution Issues

### Symptom
```
No tests found matching filter
Test discovery returned 0 results
The following test methods did not execute:
  - MyTest.TestMethod1 (Method does not exist or no [Fact] attribute)
```

### Likely Causes
- Test class does not inherit from correct base
- Missing test method attributes ([Fact], [Test], etc.)
- Test assembly not built or not found
- Filter expression is too restrictive
- xUnit/NUnit/TUnit class incorrect

### Diagnosis Steps
1. Verify test class structure:
   ```csharp
   // Correct
   public class UserTests : XUnitTestBase
   {
       [Fact]
       public void TestSomething() { }
   }
   ```
2. Check if assembly is built:
   ```bash
   ls tests/MyProject.Tests/bin/Debug/net8.0/
   # Should contain: MyProject.Tests.dll
   ```
3. Verify discovery configuration:
   ```bash
   peasy-pilot run --filter "*" --verbose
   ```
4. Check test method visibility:
   ```csharp
   // Method must be public
   public void TestMethod() { }
   ```

### Solution
**Inherit from correct base:**
```csharp
// xUnit
public class MyTests : XUnitTestBase { }

// NUnit
public class MyTests : NUnitTestBase { }

// TUnit
public class MyTests : TUnitTestBase { }
```

**Add required attributes:**
```csharp
[Fact] // xUnit
[Test] // NUnit
[Test] // TUnit
public void TestMethod() { }
```

**Rebuild and retry:**
```bash
dotnet clean tests/
dotnet build tests/
peasy-pilot run --project tests/MyProject.Tests.csproj
```

### Prevention Tips
- Use code snippets for new test classes
- Run discovery immediately after creating tests
- Use `--filter "*"` to see all discovered tests
- Keep test class naming consistent (e.g., `*Tests`)

---

## 3. Integration Testing Issues

### Symptom
```
INTG-0001: Database initialization failed
Exception: FOREIGN KEY constraint failed
The database connection timed out after 5000ms
ITestDatabaseFactory implementation not found
```

### Likely Causes
- DbContext not configured for in-memory database
- Database seeding fails due to missing data
- Foreign key relationships not handled in reset
- Database factory not registered in DI
- Transaction not properly rolled back

### Diagnosis Steps
1. Check DbContext configuration:
   ```csharp
   services.AddDbContext<TestDbContext>(opt =>
       opt.UseInMemoryDatabase("test-db")
   );
   ```
2. Test database seeding independently:
   ```csharp
   public async Task TestSeeding()
   {
       var context = new TestDbContext(options);
       await context.Database.EnsureCreatedAsync();
       // Add assertions to verify seed data
   }
   ```
3. Check foreign key constraints:
   ```csharp
   // Ensure entities are deleted in correct order
   // Child entities first, then parent entities
   await context.Orders.ExecuteDeleteAsync(); // Child
   await context.Users.ExecuteDeleteAsync();  // Parent
   ```
4. Verify DI configuration:
   ```bash
   peasy-pilot run --debug-services
   # Should show: ITestDatabaseFactory → InMemoryTestDatabaseFactory
   ```

### Solution
**Configure DbContext:**
```csharp
public class IntegrationTest
{
    protected void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TestDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString())
        );
        
        services.AddSingleton<ITestDatabaseFactory>(
            new InMemoryTestDatabaseFactory()
        );
    }
}
```

**Implement ResetAsync:**
```csharp
public async Task ResetAsync()
{
    // Delete in dependency order
    await _context.Orders.ExecuteDeleteAsync();
    await _context.Users.ExecuteDeleteAsync();
    
    // Recreate schema and seed
    await _context.Database.EnsureDeletedAsync();
    await _context.Database.EnsureCreatedAsync();
}
```

### Prevention Tips
- Use separate in-memory database name per test
- Implement proper seeding in OnModelCreating
- Test reset logic before running full suite
- Enable detailed logging for database operations
- Use IAsyncLifetime for proper async initialization

---

## 4. BDD Scenarios & Feature Files

### Symptom
```
BDD-0001: Feature file not found (features/users.feature)
BDD-0002: Gherkin parse error on line 5
BDD-0004: Step definition not found (Given a user with email {email})
BDD-0006: Step execution failed (NullReferenceException at UserSteps)
```

### Likely Causes
- Feature file path incorrect or file not in output directory
- Gherkin syntax error (missing keyword, bad indentation)
- Step definition missing or pattern doesn't match
- Step definition exception during execution
- Character encoding issue (UTF-16 instead of UTF-8)

### Diagnosis Steps
1. Verify feature file exists and is copied:
   ```bash
   ls features/*.feature
   cat features/users.feature | head -20
   ```
2. Check feature file encoding:
   ```bash
   file features/users.feature
   # Should say: UTF-8
   ```
3. Validate Gherkin syntax:
   ```gherkin
   # Correct format
   Feature: User Management
     Scenario: Create user
       Given a clean database
       When I create a user "john@example.com"
       Then the user exists
   ```
4. Check step definition:
   ```csharp
   public class UserSteps : BddStepDefinition
   {
       [Given("a clean database")]
       public void CleanDatabase() { }
       
       [When("I create a user {email}")]
       public void CreateUser(string email) { }
   }
   ```

### Solution
**Copy feature files to output:**
```xml
<!-- In .csproj -->
<ItemGroup>
    <None Update="features/**/*.feature">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

**Fix Gherkin syntax:**
- Use consistent 2-space indentation
- Verify keywords: Feature, Scenario, Given, When, Then, And, But
- Check parameter placeholders: `{name}` (not `{name:}` or `$name`)

**Register step definitions:**
```csharp
services.AddScoped<IStepBindingResolver>(provider =>
    new StepBindingResolver(typeof(UserSteps).Assembly)
);
```

### Prevention Tips
- Validate Gherkin syntax before committing
- Test feature files with mock steps first
- Use consistent naming: `*.feature` files, `*Steps.cs` classes
- Enable detailed step binding logging: `--bdd-trace`
- Test parameter extraction independently

---

## 5. CLI Operations & Commands

### Symptom
```
CLI-0001: Unknown command "rnu"
CLI-0002: Missing required parameter --project
CLI-0003: Invalid parameter value (file not found)
Connection refused when connecting to CLI server
```

### Likely Causes
- Command name misspelled
- Required parameter not provided
- File path doesn't exist or is relative
- CLI server not started
- CLI version mismatch

### Diagnosis Steps
1. Check available commands:
   ```bash
   peasy-pilot --help
   peasy-pilot <command> --help
   ```
2. Verify file paths are absolute or relative correctly:
   ```bash
   # Use ./ for current directory
   peasy-pilot run --project ./tests/MyTest.csproj
   ```
3. Check CLI version:
   ```bash
   peasy-pilot --version
   ```
4. Test command syntax:
   ```bash
   peasy-pilot run --help
   # Read required vs optional parameters
   ```

### Solution
**Use correct command syntax:**
```bash
# Correct
peasy-pilot run --project ./MyProject.Tests.csproj --filter "namespace=MyApp.Tests"

# Incorrect (parameter in wrong position)
peasy-pilot --project ./MyProject.Tests.csproj run
```

**Provide required parameters:**
```bash
peasy-pilot run \
  --project ./tests/MyProject.Tests.csproj \
  --framework xunit \
  --environment Development
```

**Use absolute paths if needed:**
```bash
peasy-pilot run --project "G:\MyProject\tests\MyProject.Tests.csproj"
```

### Prevention Tips
- Use `--help` before running new commands
- Validate paths exist before running commands
- Update CLI frequently: `dotnet tool update --global peasy-pilot`
- Use configuration file for complex options:
  ```bash
  peasy-pilot run --config ./peasy-pilot.json
  ```

---

## 6. Performance Issues

### Symptom
```
Tests are running very slowly (> 30 seconds)
Database initialization takes minutes
Test discovery takes > 1 minute
Memory usage exceeds 1GB
```

### Likely Causes
- Too many tests in single discovery run
- Database not using in-memory provider
- Large datasets in seeding
- Unnecessary async/await usage
- Missing indexing on frequently queried columns

### Diagnosis Steps
1. Profile test execution:
   ```bash
   peasy-pilot run --project MyTests.csproj --profile
   # Shows: discovery time, execution time, teardown time
   ```
2. Check database provider:
   ```csharp
   // Should use InMemoryDatabase for tests
   opt.UseInMemoryDatabase("test-db")
   
   // Not SQL Server
   opt.UseSqlServer("connection-string") // SLOW
   ```
3. Monitor memory usage:
   ```bash
   # Windows
   Get-Process dotnet | Select WorkingSet
   
   # macOS/Linux
   ps aux | grep dotnet
   ```
4. Analyze seeding time:
   ```csharp
   var sw = Stopwatch.StartNew();
   await context.Database.EnsureCreatedAsync();
   sw.Stop();
   Console.WriteLine($"Seeding took {sw.ElapsedMilliseconds}ms");
   ```

### Solution
**Batch tests into smaller groups:**
```bash
# Run subset of tests
peasy-pilot run --filter "namespace=MyApp.Tests.Unit"
```

**Use parallel execution where possible:**
```csharp
[Collection("Non-Parallel")] // For tests that need sequential execution
public class IntegrationTest { }

// Other tests run in parallel automatically
```

**Optimize seeding:**
```csharp
// Use bulk insert instead of individual adds
context.Users.AddRange(users);
await context.SaveChangesAsync();
```

**Cache expensive operations:**
```csharp
private static IEnumerable<TestData> _cachedData;

public IEnumerable<TestData> GetTestData()
{
    return _cachedData ??= LoadTestData();
}
```

### Prevention Tips
- Monitor test execution time regularly
- Use in-memory database exclusively for tests
- Keep seed data minimal (only what tests need)
- Profile before optimizing (use `--profile` flag)
- Run tests in parallel (frameworks support this)

---

## 7. CI/CD Pipeline Issues

### Symptom
```
CI build failed: "Build succeeded but tests failed in pipeline"
NuGet restore timeout in GitHub Actions
Docker image build failed: "Package not found"
Tests pass locally but fail in CI
```

### Likely Causes
- NuGet source not configured in CI environment
- Different .NET SDK version in CI vs. local
- Tests depend on local resources (database, files)
- Environment variables not set in CI
- Race conditions in parallel test execution

### Diagnosis Steps
1. Check CI configuration (.github/workflows):
   ```yaml
   - name: Setup .NET
     uses: actions/setup-dotnet@v4
     with:
       dotnet-version: '8.0.x'
   ```
2. Verify NuGet sources in CI:
   ```bash
   # In CI pipeline, add:
   dotnet nuget add source https://api.nuget.org/v3/index.json
   ```
3. Check for local resource dependencies:
   ```csharp
   // Bad: hardcoded paths
   var dbPath = "C:\\MyData\\test.db";
   
   // Good: environment-based or in-memory
   var dbPath = Environment.GetEnvironmentVariable("TEST_DB_PATH")
       ?? ":memory:";
   ```
4. Run tests with CI configuration locally:
   ```bash
   export GITHUB_WORKSPACE=$(pwd)
   dotnet test # Same as CI would run
   ```

### Solution
**Configure NuGet in CI:**
```yaml
# .github/workflows/test.yml
- name: Restore packages
  run: |
    dotnet nuget add source https://api.nuget.org/v3/index.json \
      -n nuget.org \
      -c
    dotnet restore
```

**Use environment-aware configuration:**
```csharp
public class TestConfiguration
{
    public static string GetDatabasePath()
    {
        var isCI = !string.IsNullOrEmpty(
            Environment.GetEnvironmentVariable("CI"));
        
        if (isCI)
            return ":memory:";
        
        return Path.Combine(
            Path.GetTempPath(),
            "peasy-test.db"
        );
    }
}
```

**Enable detailed CI logging:**
```bash
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

### Prevention Tips
- Test CI configuration locally before pushing
- Use same .NET SDK version in CI and local
- Avoid hardcoded paths or assumptions
- Set all required environment variables in CI
- Log detailed information for CI failures
- Use `--help` to understand all test options

---

## Quick Diagnosis Checklist

**Before contacting support, verify:**
- [ ] Code builds successfully: `dotnet build`
- [ ] All dependencies installed: `dotnet restore`
- [ ] Tests discovered: `peasy-pilot run --filter "*" --verbose`
- [ ] Error code documented: [Error Codes Reference](error-codes.md)
- [ ] Logs show root cause: Enable `--verbose` or `--debug`
- [ ] Issue reproducible locally
- [ ] Searched documentation for similar issue

---

## Common Error Code Quick Links

| Symptom | Error Code | Fix |
|---------|-----------|-----|
| Tests not discovered | CORE-0001 | Add [Fact], inherit base class |
| Database init failed | INTG-0001 | Configure DbContext properly |
| Feature file not found | BDD-0001 | Copy to output, verify path |
| Step definition missing | BDD-0004 | Register in DI, verify pattern |
| CLI command unknown | CLI-0001 | Use `--help`, check spelling |
| Slow tests | (Performance) | Use in-memory DB, parallel |

---

**Need more help?**
- [Full Error Codes Reference](error-codes.md)
- [CLI Reference](cli-reference.md)
- [Configuration Reference](configuration-reference.md)

---

**Last Updated:** 2026-09-11  
[← Back to REFERENCE](README.md)
