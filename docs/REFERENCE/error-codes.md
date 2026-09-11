# Error Codes Reference

Complete error code reference for PeasyPilot framework and packages.

## Error Code Format

Error codes follow the pattern: **PPPP-NNNN**

- **PPPP** — Package code (4 letters)
- **NNNN** — Error number (0000-9999)

### Package Codes

| Code | Package | Description |
|------|---------|-------------|
| CORE | PeasyPilot.Core | Core abstractions, discovery, orchestration |
| UNIT | PeasyPilot.Unit | Unit testing utilities |
| INTG | PeasyPilot.Integration | Integration testing fixtures |
| BDD  | PeasyPilot.BDD | Behavior-Driven Development |
| MOQ  | PeasyPilot.Moq | Mock factory abstractions |
| BOGUS | PeasyPilot.Bogus | Fake data generation |
| COV  | PeasyPilot.Coverage | Coverage reporting |
| CLI  | PeasyPilot.CLI | Command-line interface |
| TST  | PeasyPilot.TestAssistant | Test generation & analysis |
| MCP  | PeasyPilot.Mcp | MCP server & transport |
| XUNIT | PeasyPilot.XUnit | xUnit framework adapter |
| NUNIT | PeasyPilot.NUnit | NUnit framework adapter |
| TUNIT | PeasyPilot.TUnit | TUnit framework adapter |

---

## Core Package (CORE)

### CORE-0001 — Test Not Discovered

**Message:** "No tests found matching the filter or discovery criteria."

**Cause:**
- Test class does not inherit from correct base class
- Test method missing [Fact] or [Fact(Skip=...)] attribute (xUnit)
- Test method missing [Test] attribute (NUnit)
- Assembly not referenced or not built

**Solution:**
1. Verify test class inherits from the appropriate framework base class
2. Ensure test methods have the correct attribute ([Fact], [Test], etc.)
3. Check that the test assembly is built: `dotnet build tests/`
4. Verify the assembly is referenced in discovery configuration
5. Run discovery with verbose logging: `--verbose` or `--debug`

**Example Error Output:**
```
CORE-0001: No tests found.
Discovery filter: namespace=MyApp.Tests
Checked: 1 assembly (5 classes, 0 valid test methods)
```

**Related Codes:** XUNIT-0010, NUNIT-0010, TUNIT-0010

---

### CORE-0002 — Orchestration Context Not Initialized

**Message:** "TestContext is null or not properly initialized."

**Cause:**
- TestOrchestrator not invoked before accessing context
- Fixture initialization failed silently
- Dependency injection container not configured

**Solution:**
1. Ensure TestOrchestrator.Initialize() is called before test execution
2. Check fixture setup methods (Setup, [OneTimeSetUp], etc.)
3. Verify DI container configuration in IntegrationTestFixture
4. Add initialization logging to diagnose silent failures

**Example Error Output:**
```
CORE-0002: TestContext initialization failed.
Stack: at PeasyPilot.Core.TestCase.Execute()
Expected: context.ServiceProvider != null
Actual: null
```

**Related Codes:** INTG-0001, INTG-0002

---

### CORE-0003 — Test Result Reporter Not Found

**Message:** "No ITestReporter implementation found in service collection."

**Cause:**
- Reporter not registered in DI container
- Wrong reporter implementation injected
- Service registration overwritten

**Solution:**
1. Register reporter in ConfigureServices:
   ```csharp
   services.AddSingleton<ITestReporter>(
       new ConsoleTestReporter()
   );
   ```
2. Check registration order in Startup/Program.cs
3. Verify no duplicate registrations override your configuration

**Example Error Output:**
```
CORE-0003: Service resolution failed.
Type: PeasyPilot.Core.Reporting.ITestReporter
Status: Not registered
Available reporters: (none)
```

**Related Codes:** CORE-0005, INTG-0003

---

### CORE-0004 — Invalid Test Impact Analysis

**Message:** "Impact analysis failed or returned unexpected results."

**Cause:**
- Source code path not found or inaccessible
- Roslyn compilation failed
- Dependency graph incomplete

**Solution:**
1. Verify source paths exist and are readable
2. Run `dotnet build` to ensure compilation succeeds
3. Check for Roslyn errors in build output
4. Enable impact analysis debug mode: `--impact-debug`

**Example Error Output:**
```
CORE-0004: Impact analysis failed.
Reason: Source path not found
Path: G:\src\MyApp\Services
Status: Directory does not exist
```

**Related Codes:** TST-0005, TST-0006

---

### CORE-0005 — Service Dependency Not Registered

**Message:** "Service of type 'X' not registered in dependency injection container."

**Cause:**
- Required service missing from ConfigureServices
- Interface/implementation mismatch
- Lifetime scope issue (Transient vs. Singleton)

**Solution:**
1. Add service registration:
   ```csharp
   services.AddScoped<IMyService, MyService>();
   ```
2. Ensure interfaces match what's being resolved
3. Check service lifetime requirements
4. Use `services.DescribeRegistrations()` to debug

**Example Error Output:**
```
CORE-0005: Service not registered.
Requested: PeasyPilot.Core.ITestDatabase (Scoped)
Hint: Did you call ConfigureIntegrationTesting()?
```

**Related Codes:** INTG-0003, INTG-0004

---

## Integration Testing (INTG)

### INTG-0001 — InMemory Database Initialization Failed

**Message:** "In-memory database could not be created or configured."

**Cause:**
- Entity Framework DbContext not configured
- Database seeding method threw exception
- Memory constraints or connection timeout

**Solution:**
1. Ensure DbContext is registered:
   ```csharp
   services.AddDbContext<TestDbContext>(
       opt => opt.UseInMemoryDatabase("test-db")
   );
   ```
2. Verify seeding logic in `OnModelCreating()` or `SeedAsync()`
3. Check for connection string configuration
4. Add try-catch around seeding to capture errors

**Example Error Output:**
```
INTG-0001: Database initialization failed.
DbContext: MyAppContext
Error: The property 'Id' on type 'User' is not mapped
Provider: in-memory
```

**Related Codes:** INTG-0002, INTG-0004

---

### INTG-0002 — Database Reset Failed During Teardown

**Message:** "Could not reset database state between test runs."

**Cause:**
- Database transaction not committed or rolled back
- Foreign key constraints prevent deletion
- ResetAsync() method exception

**Solution:**
1. Implement ResetAsync() on your database fixture:
   ```csharp
   public async Task ResetAsync()
   {
       await _context.Database.EnsureDeletedAsync();
       await _context.Database.EnsureCreatedAsync();
   }
   ```
2. Check foreign key relationships and delete order
3. Add explicit transaction rollback before reset
4. Test reset logic independently

**Example Error Output:**
```
INTG-0002: Database reset failed.
Operation: EnsureDeletedAsync
Error: FOREIGN KEY constraint failed
Table: Orders, Column: UserId
```

**Related Codes:** INTG-0001, INTG-0003

---

### INTG-0003 — Test Database Factory Not Configured

**Message:** "ITestDatabaseFactory implementation not found."

**Cause:**
- Factory not registered in DI container
- Wrong factory type registered (SQL instead of InMemory)
- Factory.Create() returned null

**Solution:**
1. Register factory in ConfigureIntegrationTesting:
   ```csharp
   services.AddSingleton<ITestDatabaseFactory>(
       new InMemoryTestDatabaseFactory()
   );
   ```
2. Verify factory.Create() implementation
3. Check that database options are passed correctly
4. Use explicit type specification if multiple implementations exist

**Example Error Output:**
```
INTG-0003: Database factory not registered.
Available factories: (none)
Expected: ITestDatabaseFactory
Hint: Call ConfigureIntegrationTesting(services)
```

**Related Codes:** INTG-0001, CORE-0005

---

### INTG-0004 — WebApplicationTestFactory Configuration Invalid

**Message:** "WebApplicationFactory<T> could not configure test server."

**Cause:**
- Startup class or Program.cs incompatible
- Host builder configuration missing
- Generic type T does not have IHost support

**Solution:**
1. Ensure Program.cs creates a WebApplication:
   ```csharp
   var app = builder.Build();
   // ... configure middleware
   await app.RunAsync();
   ```
2. Verify WebApplicationTestFactory<Program> (or Startup class) is used
3. Override ConfigureWebHost if custom configuration needed:
   ```csharp
   protected override void ConfigureWebHost(IWebHostBuilder builder)
   {
       builder.ConfigureServices(services => { /* ... */ });
   }
   ```
4. Check for exceptions in middleware pipeline

**Example Error Output:**
```
INTG-0004: WebApplicationFactory initialization failed.
Generic type: MyApp.Program
Error: No public Program type found
Hint: Ensure Program.cs is in root namespace
```

**Related Codes:** INTG-0005, INTG-0006

---

### INTG-0005 — HTTP Test Request Failed

**Message:** "HTTP request to test server failed or timed out."

**Cause:**
- Test server not started
- Invalid endpoint path
- Request timeout or connection reset
- Missing authentication token

**Solution:**
1. Verify server is created: `using var factory = new WebApplicationTestFactory<Program>();`
2. Check endpoint path matches controller routing
3. Increase timeout if test is slow: `httpClient.Timeout = TimeSpan.FromSeconds(10);`
4. Add authentication if endpoint requires it:
   ```csharp
   httpClient.DefaultRequestHeaders.Authorization = 
       new AuthenticationHeaderValue("Bearer", token);
   ```
5. Enable detailed logging: `--verbose --http-trace`

**Example Error Output:**
```
INTG-0005: HTTP request failed.
Method: GET
Endpoint: /api/users/1
Status: Connection timeout after 5000ms
```

**Related Codes:** INTG-0004, INTG-0006

---

### INTG-0006 — Test Fixture Lifetime Scope Mismatch

**Message:** "Service lifetime does not match fixture scope."

**Cause:**
- Singleton service accessed in test scope
- Scoped service shared across test methods
- Transaction isolation level incorrect

**Solution:**
1. Use correct service lifetime for test environment:
   - Singleton: Shared state between tests (database connections)
   - Scoped: Created per test method (HTTP requests)
   - Transient: New instance each time
2. Use IAsyncLifetime for async initialization:
   ```csharp
   public class MyTest : IAsyncLifetime
   {
       public async Task InitializeAsync() { }
       public async Task DisposeAsync() { }
   }
   ```
3. Implement ResetAsync() for stateful services:
   ```csharp
   public class MyFixture : IAsyncLifetime, IResettable
   {
       public async Task ResetAsync() { }
   }
   ```

**Example Error Output:**
```
INTG-0006: Lifetime scope violation.
Service: MyRepository (Singleton)
Scope: Test method (should be Scoped)
Effect: State pollution between tests
```

**Related Codes:** INTG-0001, INTG-0002

---

## BDD Package (BDD)

### BDD-0001 — Feature File Not Found

**Message:** "Feature file could not be located or loaded."

**Cause:**
- File path incorrect or relative path miscalculated
- Feature file not copied to output directory
- Directory permissions prevent reading

**Solution:**
1. Verify feature file exists:
   ```bash
   ls docs/features/*.feature
   ```
2. Ensure file is copied to build output:
   ```xml
   <ItemGroup>
       <None Update="features/**/*.feature">
           <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
       </None>
   </ItemGroup>
   ```
3. Use absolute paths or verify working directory
4. Check file encoding is UTF-8 (not UTF-16 or BOM)

**Example Error Output:**
```
BDD-0001: Feature file not found.
Path: features/users.feature
Searched: G:\Project\features\users.feature
Status: File does not exist
```

**Related Codes:** BDD-0002, BDD-0003

---

### BDD-0002 — Feature File Parse Error

**Message:** "Gherkin feature file syntax is invalid."

**Cause:**
- Missing "Feature:" keyword
- Indentation is incorrect (not 2 or 4 spaces)
- Invalid step keyword (Given/When/Then)
- Syntax error in scenario outline parameters

**Solution:**
1. Verify Gherkin syntax:
   ```gherkin
   Feature: User Registration
     Scenario: Register with valid email
       Given a user registration form
       When the user enters "user@example.com"
       Then the account is created
   ```
2. Check indentation consistency (all spaces or all tabs)
3. Ensure only valid keywords: Given, When, Then, And, But, Scenario, Feature
4. Validate parameter syntax: `<name>` for placeholders

**Example Error Output:**
```
BDD-0002: Gherkin parse error.
File: features/users.feature
Line: 5
Error: Unknown step keyword "Given:" (should be "Given")
```

**Related Codes:** BDD-0001, BDD-0004

---

### BDD-0003 — Feature File Character Encoding Issue

**Message:** "Feature file contains invalid characters or encoding."

**Cause:**
- File encoded as UTF-16 instead of UTF-8
- Special characters not properly encoded (accents, symbols)
- BOM (Byte Order Mark) present in file

**Solution:**
1. Verify file encoding is UTF-8:
   ```bash
   file features/users.feature
   # Output should contain "UTF-8"
   ```
2. Convert if needed:
   ```bash
   # Windows PowerShell
   (Get-Content features/users.feature) | 
   Set-Content -Encoding UTF8 features/users.feature
   ```
3. Remove BOM if present (VS Code: change encoding to "UTF-8" in status bar)
4. Use proper Unicode characters for special letters (é, ñ, etc.)

**Example Error Output:**
```
BDD-0003: Character encoding error.
File: features/users.feature
Detected: UTF-16 LE
Expected: UTF-8
Hint: Use "Save with Encoding" (UTF-8 without BOM)
```

**Related Codes:** BDD-0001, BDD-0002

---

### BDD-0004 — Step Definition Not Found

**Message:** "No step definition matches the given step in the feature file."

**Cause:**
- Step pattern does not match step text in feature
- Step definition class not decorated with [Given], [When], [Then]
- Pattern regex is incorrect or too strict
- Step definition assembly not referenced or not scanned

**Solution:**
1. Verify step definition exists:
   ```csharp
   public class UserSteps : BddStepDefinition
   {
       [Given("a user with email {email}")]
       public void CreateUser(string email) { }
   }
   ```
2. Check pattern matches feature file step exactly:
   ```gherkin
   Given a user with email "john@example.com"
   # Pattern must match: "a user with email {email}"
   ```
3. Verify step definition assembly is referenced in test project
4. Register step definitions in DI container if using resolver
5. Check parameter names match regex groups: `{email}` → `email`

**Example Error Output:**
```
BDD-0004: Step definition not found.
Step: "Given a user exists"
Pattern: "a user with email {email}"
Status: No match
Checked assemblies: 2 (8 step definitions)
```

**Related Codes:** BDD-0005, BDD-0006

---

### BDD-0005 — Step Binding Parameter Extraction Failed

**Message:** "Could not extract parameter values from step text."

**Cause:**
- Parameter name in pattern does not match capture group
- Type conversion failed (string to int, decimal, etc.)
- Regex pattern is malformed or missing groups

**Solution:**
1. Ensure parameter names in pattern match method signature:
   ```csharp
   [Given("I have {count:int} items")]
   public void SetupItems(int count) { } // count must match
   ```
2. Verify type conversion is supported (int, decimal, bool, string)
3. Test regex pattern in isolation:
   ```csharp
   var pattern = @"I have (?<count>\d+) items";
   var match = Regex.Match("I have 5 items", pattern);
   assert(match.Success && match.Groups["count"].Value == "5");
   ```
4. Add custom type converters if needed:
   ```csharp
   public class EmailConverter : IParameterConverter
   {
       public object Convert(string value) => new Email(value);
   }
   ```

**Example Error Output:**
```
BDD-0005: Parameter extraction failed.
Step: "I have 5 items"
Pattern: "I have {count} items"
Error: Cannot convert "5" to type System.Int32
```

**Related Codes:** BDD-0004, BDD-0006

---

### BDD-0006 — Step Execution Exception

**Message:** "Step executed but threw an exception."

**Cause:**
- Assertion failed in step definition
- Business logic error (database, API, etc.)
- Null reference or argument validation

**Solution:**
1. Check step definition implementation for errors:
   ```csharp
   [When("the user logs in")]
   public void UserLogsIn()
   {
       var user = _repository.GetUser("john@example.com");
       Assert.NotNull(user); // Catches if user doesn't exist
       _service.Login(user);
   }
   ```
2. Add try-catch and detailed error messages
3. Verify test data is set up correctly in Given steps
4. Check for external service failures (database, API)
5. Enable step execution tracing: `--bdd-trace`

**Example Error Output:**
```
BDD-0006: Step execution failed.
Step: "When the user logs in"
Exception: NullReferenceException
Message: User "john@example.com" not found
Stack: at UserSteps.UserLogsIn() line 45
```

**Related Codes:** BDD-0004, BDD-0005, INTG-0001

---

## CLI Package (CLI)

### CLI-0001 — Command Not Recognized

**Message:** "Unknown command or invalid syntax."

**Cause:**
- Command name misspelled
- Command requires specific version
- Command not available in current context

**Solution:**
1. List available commands:
   ```bash
   peasy-pilot --help
   peasy-pilot <command> --help
   ```
2. Check command syntax:
   ```bash
   # Correct
   peasy-pilot run --project MyProject.csproj
   
   # Incorrect
   peasy-pilot --project MyProject.csproj run
   ```
3. Update PeasyPilot to latest version: `dotnet tool update --global peasy-pilot`
4. Check help for required vs. optional parameters

**Example Error Output:**
```
CLI-0001: Unknown command.
Input: "peasy-pilot rnu"
Did you mean: "peasy-pilot run"
Use: "peasy-pilot --help" for available commands
```

**Related Codes:** CLI-0002, CLI-0003

---

### CLI-0002 — Missing Required Parameter

**Message:** "Required parameter not provided."

**Cause:**
- Parameter is required but not specified
- Parameter uses wrong format (e.g., value expected but only flag given)
- Configuration file missing

**Solution:**
1. Check command help for required parameters:
   ```bash
   peasy-pilot run --help
   ```
2. Provide required parameters:
   ```bash
   # Example requires --project
   peasy-pilot run --project MyProject.csproj
   ```
3. Use configuration file if preferred:
   ```bash
   peasy-pilot run --config peasy-pilot.json
   ```
4. Check for typos in parameter names

**Example Error Output:**
```
CLI-0002: Missing required parameter.
Command: run
Required: --project <path>
Hint: peasy-pilot run --project src/MyProject.csproj
```

**Related Codes:** CLI-0001, CLI-0003

---

### CLI-0003 — Invalid Parameter Value

**Message:** "Parameter value is invalid or not in expected format."

**Cause:**
- File path does not exist
- Number out of valid range
- Invalid enum value
- Malformed filter expression

**Solution:**
1. Verify file paths exist:
   ```bash
   peasy-pilot run --project ./MyProject.csproj # Use ./
   ```
2. Check enum values:
   ```bash
   # Valid: Development, Staging, Production
   peasy-pilot run --environment Development
   ```
3. Validate filter syntax:
   ```bash
   # Valid: "namespace=MyApp.Tests"
   peasy-pilot run --filter "namespace=MyApp.Tests"
   ```
4. Check number ranges in help documentation

**Example Error Output:**
```
CLI-0003: Invalid parameter value.
Parameter: --project
Value: "./NonExistent.csproj"
Status: File not found
```

**Related Codes:** CLI-0001, CLI-0002

---

## Test Assistant Package (TST)

### TST-0001 — Code Analysis Engine Failed

**Message:** "Could not analyze code structure or extract metadata."

**Cause:**
- Roslyn compilation failed
- Type resolution failed
- Source file not found or not readable

**Solution:**
1. Ensure code compiles: `dotnet build`
2. Check file paths are correct and readable
3. Verify project references are complete
4. Enable detailed Roslyn diagnostics:
   ```csharp
   var analyzer = new CSharpCodeAnalyzer(enableDiagnostics: true);
   ```
5. Check for unsupported language features (C# version mismatch)

**Example Error Output:**
```
TST-0001: Code analysis failed.
File: Services/UserService.cs
Error: CS0103 - Type 'UserRepository' does not exist
Likely: Missing reference or using statement
```

**Related Codes:** TST-0002, TST-0003

---

### TST-0002 — Test Plan Generation Failed

**Message:** "Could not generate test plan from analyzed code."

**Cause:**
- Analysis incomplete or returned null
- Method has no parameters or scenarios to test
- Test quality scorer returned invalid scores

**Solution:**
1. Verify code analysis succeeded (see TST-0001)
2. Check that method is not abstract or extern
3. Ensure method has at least 1 parameter or dependency
4. Check TestPlanBuilder configuration:
   ```csharp
   var builder = new TestPlanBuilder();
   var plan = await builder.GenerateAsync(methodModel);
   ```
5. Enable tracing: `--test-plan-debug`

**Example Error Output:**
```
TST-0002: Test plan generation failed.
Method: EmptyMethod()
Reason: No parameters or scenarios to test
Hint: Add parameters or dependencies to generate test plan
```

**Related Codes:** TST-0001, TST-0003

---

### TST-0003 — Test Code Generation Failed

**Message:** "Could not generate test code from plan."

**Cause:**
- Plan is incomplete or null
- Template syntax error
- Invalid namespace or class name
- Unsupported test framework

**Solution:**
1. Verify plan generation succeeded (see TST-0002)
2. Check template syntax in code generator:
   ```csharp
   var generator = new TestCodeGenerator("xunit"); // valid: xunit, nunit, tunit
   ```
3. Ensure target method is valid for testing
4. Check namespace syntax (no special characters)
5. Validate output path is writable

**Example Error Output:**
```
TST-0003: Test code generation failed.
Framework: unknown
Supported: xunit, nunit, tunit
Error: Template syntax error on line 25
```

**Related Codes:** TST-0001, TST-0002

---

## MCP Package (MCP)

### MCP-0001 — MCP Server Connection Failed

**Message:** "Could not establish MCP server connection."

**Cause:**
- Server not started or crashed
- Port already in use
- Network/firewall blocking connection
- Protocol version mismatch

**Solution:**
1. Start MCP server explicitly:
   ```bash
   peasy-pilot mcp start --port 5000
   ```
2. Check if port is available:
   ```bash
   netstat -an | grep 5000 # Windows
   lsof -i :5000 # macOS/Linux
   ```
3. Use different port if needed:
   ```bash
   peasy-pilot mcp start --port 5001
   ```
4. Check server logs for errors:
   ```bash
   peasy-pilot mcp logs --follow
   ```
5. Enable connection tracing: `--mcp-trace`

**Example Error Output:**
```
MCP-0001: Connection failed.
Server: localhost:5000
Error: Connection refused (ECONNREFUSED)
Hint: Start server with "peasy-pilot mcp start"
```

**Related Codes:** MCP-0002, MCP-0003

---

### MCP-0002 — MCP Protocol Error

**Message:** "MCP message format or protocol violation."

**Cause:**
- Malformed JSON in message
- Required field missing
- Version mismatch between client and server
- Unsupported message type

**Solution:**
1. Verify MCP version compatibility:
   ```bash
   peasy-pilot --version
   # Should match server version
   ```
2. Check message format:
   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "analyze",
     "params": { /* ... */ }
   }
   ```
3. Ensure all required fields are present
4. Enable protocol debugging:
   ```bash
   peasy-pilot mcp status --debug
   ```

**Example Error Output:**
```
MCP-0002: Protocol error.
Message: {"method":"analyze"}
Error: Missing required field "jsonrpc"
```

**Related Codes:** MCP-0001, MCP-0003

---

### MCP-0003 — MCP Tool Execution Failed

**Message:** "MCP tool invocation returned an error."

**Cause:**
- Tool parameters are invalid
- Tool implementation threw exception
- Tool not registered or found
- Timeout during execution

**Solution:**
1. Check tool parameters match signature:
   ```bash
   peasy-pilot mcp tools # List all tools
   peasy-pilot mcp tools analyze --help # Help for tool
   ```
2. Verify parameters are correct:
   ```bash
   peasy-pilot mcp call analyze --file src/Services/UserService.cs
   ```
3. Increase timeout if tool is slow:
   ```bash
   peasy-pilot --mcp-timeout 30000 # 30 seconds
   ```
4. Check server logs for detailed errors:
   ```bash
   peasy-pilot mcp logs --tool analyze
   ```

**Example Error Output:**
```
MCP-0003: Tool execution failed.
Tool: analyze
Error: File not found: "src/Services/UserService.cs"
Status: Parameter validation failed
```

**Related Codes:** MCP-0001, MCP-0002

---

## Summary

For additional help:
- [Troubleshooting Quick Reference](troubleshooting-quick-ref.md) — Common issues and diagnostics
- [CLI Reference](cli-reference.md) — Command syntax and options
- [Configuration Reference](configuration-reference.md) — Settings and environment variables

---

**Last Updated:** 2026-09-11  
[← Back to REFERENCE](README.md)
