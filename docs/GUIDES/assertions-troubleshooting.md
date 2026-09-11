# Assertions Troubleshooting Guide

## Common Issues & Solutions

### Issue 1: "Assert/NAssert/XAssert/TAssert not found"

**Error Message:**
```
error CS0103: The name 'NAssert' does not exist in the current context
```

**Root Cause:**
- Missing using statement
- Project doesn't reference correct PeasyPilot package

**Solution:**

```csharp
// Add this using statement
using PeasyPilot.NUnit;  // for NAssert
using PeasyPilot.XUnit;  // for XAssert
using PeasyPilot.TUnit;  // for TAssert
```

Verify project references in .csproj:
```xml
<ItemGroup>
    <ProjectReference Include="...\src\PeasyPilot.NUnit.csproj" />
    <!-- or -->
    <ProjectReference Include="...\src\PeasyPilot.XUnit.csproj" />
</ItemGroup>
```

---

### Issue 2: IDE IntelliSense Not Working for Aliases

**Symptom:**
- XAssert appears in red squiggly
- No autocomplete for XAssert methods
- Code compiles but IDE shows errors

**Root Cause:**
- Project file not reloaded
- NuGet cache stale
- IDE indexing not updated

**Solution:**

1. **Reload Project:**
   - Right-click project → Unload Project
   - Right-click → Reload Project

2. **Clean NuGet Cache:**
   ```bash
   dotnet nuget locals all --clear
   ```

3. **Restart IDE:**
   - Close and reopen Visual Studio

4. **Rebuild Solution:**
   ```bash
   dotnet clean && dotnet build
   ```

---

### Issue 3: Test Framework Assertion Method Not Found

**Error Message:**
```
error CS0117: 'NAssert' does not contain a definition for 'IsTrue'
```

**Root Cause:**
- Method doesn't exist in that framework's Assert
- Typo in method name
- Using wrong framework's assertion

**Solution:**

Check which methods are available:

```csharp
// NUnit assertions
NAssert.IsTrue(condition);       // ✅ Available
NAssert.That(x, Is.True);       // ✅ Available

// xUnit assertions
XAssert.True(condition);         // ✅ Available (not IsTrue)
XAssert.That(x == y);           // ❌ Not xUnit syntax

// TUnit assertions
await TAssert.That(x).IsTrue();  // ✅ Available
TAssert.That(x).IsTrue();        // ❌ Missing await
```

**Framework Method Mapping:**
| Intent | NAssert | XAssert | TAssert |
|--------|---------|---------|---------|
| Check True | IsTrue() | True() | await .IsTrue() |
| Check Equal | AreEqual() | Equal() | await .IsEqualTo() |
| Check Exception | Throws<T>() | Throws<T>() | await ThrowsAsync<T>() |

---

### Issue 4: Async Test Hanging or Timeout

**Symptom:**
- Test times out without error
- Test hangs indefinitely
- Only happens with async tests

**Root Cause:**
- Missing `await` on async assertion
- Deadlock from blocking call (`.Result`, `.Wait()`)
- Infinite async recursion

**Solution:**

```csharp
// ❌ WRONG: Missing await on TAssert
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = await service.FetchUserAsync(1);
    TAssert.That(user).IsNotNull(); // Missing await - HANGS
}

// ✅ CORRECT: await TAssert
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = await service.FetchUserAsync(1);
    await TAssert.That(user).IsNotNull(); // Correct
}

// ❌ WRONG: Blocking call (deadlock risk)
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = service.FetchUserAsync(1).Result; // DEADLOCK
    // ...
}

// ✅ CORRECT: Use await
[Test]
public async Task FetchUser_ReturnsUser()
{
    var user = await service.FetchUserAsync(1); // Correct
    // ...
}
```

---

### Issue 5: Exception Assertion Not Catching Exception

**Symptom:**
- Test expects exception but test passes
- Exception is silently caught elsewhere
- Assert.Throws() doesn't trigger

**Root Cause:**
- Exception thrown on different thread
- Exception caught before assertion
- Lambda doesn't actually execute code

**Solution:**

```csharp
// ❌ WRONG: Exception not in lambda
[Test]
public void DeleteUser_ThrowsException()
{
    var user = new User();
    service.DeleteUser(user); // Exception thrown here, not in assertion
    NAssert.Throws<NotFoundException>(() => { }); // Lambda does nothing
}

// ✅ CORRECT: Exception inside lambda
[Test]
public void DeleteUser_ThrowsException()
{
    NAssert.Throws<NotFoundException>(() =>
    {
        service.DeleteUser(null); // Exception thrown HERE
    });
}

// ❌ WRONG: Exception on different thread
[Test]
public void AsyncOperation_ThrowsException()
{
    var task = Task.Run(() =>
    {
        throw new InvalidOperationException();
    });
    
    XAssert.Throws<InvalidOperationException>(() => { }); // Won't catch
}

// ✅ CORRECT: Use async assertion
[Test]
public async Task AsyncOperation_ThrowsException()
{
    await TAssert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await service.FailingOperationAsync();
    });
}
```

---

### Issue 6: Collection Assertion Fails Unexpectedly

**Symptom:**
- `Contains()` returns false when item looks present
- Collection count wrong
- Empty assertion fails

**Root Cause:**
- Object equality not implemented
- Reference comparison instead of value comparison
- Collection modified during iteration

**Solution:**

```csharp
// ❌ WRONG: Objects don't implement Equals
[Test]
public void UserList_ContainsUser()
{
    var users = new List<User> { new User { Id = 1, Name = "John" } };
    var searchUser = new User { Id = 1, Name = "John" };
    
    NAssert.Contains(searchUser, users); // FAILS - different objects
}

// ✅ CORRECT: Override Equals or compare by ID
[Test]
public void UserList_ContainsUser()
{
    var users = new List<User> { new User { Id = 1, Name = "John" } };
    var exists = users.Any(u => u.Id == 1);
    
    NAssert.IsTrue(exists); // PASSES
}

// ❌ WRONG: Modifying collection during test
[Test]
public void Collection_IsNotEmpty()
{
    var items = new List<string> { "a", "b", "c" };
    items.Clear(); // Oops!
    
    NAssert.IsNotEmpty(items); // FAILS
}
```

---

### Issue 7: Test Passes When It Should Fail

**Symptom:**
- Assertion doesn't fail even when condition is false
- Test passes with wrong result
- Assertion was never executed

**Root Cause:**
- Assertion not actually called
- Wrong variable compared
- Exception caught silently

**Solution:**

```csharp
// ❌ WRONG: Assertion never reached
[Test]
public void CreateUser_WithValidData_ReturnsId()
{
    var result = service.CreateUser(validUser);
    if (result <= 0) return; // Early exit - assertion skipped!
    
    NAssert.That(result, Is.GreaterThan(0));
}

// ✅ CORRECT: No early exits
[Test]
public void CreateUser_WithValidData_ReturnsId()
{
    var result = service.CreateUser(validUser);
    NAssert.That(result, Is.GreaterThan(0)); // Always executed
}

// ❌ WRONG: Wrong variable compared
[Test]
public void UpdateUser_ChangesName()
{
    var originalName = user.Name;
    service.UpdateUser(user, "Jane");
    
    NAssert.AreEqual("Jane", originalName); // Comparing OLD value!
}

// ✅ CORRECT: Compare updated value
[Test]
public void UpdateUser_ChangesName()
{
    service.UpdateUser(user, "Jane");
    
    NAssert.AreEqual("Jane", user.Name); // Correct
}
```

---

### Issue 8: Multiple Assertions Hide Real Failure

**Symptom:**
- Test fails but assertion message unclear
- Don't know which assertion failed
- Hard to debug

**Solution:**

```csharp
// ❌ WRONG: Multiple assertions in one test
[Test]
public void User_IsCompletelyValid()
{
    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    NAssert.That(user.Id, Is.GreaterThan(0));
    NAssert.That(user.Name, Is.Not.Empty);
    NAssert.That(user.Email, Contains.Substring("@"));
    NAssert.That(user.Age, Is.GreaterThan(18));
    // If one fails, which one?
}

// ✅ CORRECT: One assertion per test
[Test]
public void CreateUser_WithValidData_AssignsId()
{
    var user = new User { Name = "John" };
    NAssert.That(user.Id, Is.GreaterThan(0));
}

[Test]
public void CreateUser_WithValidData_StoresName()
{
    var user = new User { Name = "John" };
    NAssert.That(user.Name, Is.EqualTo("John"));
}

[Test]
public void CreateUser_WithValidData_ValidatesEmail()
{
    var user = new User { Email = "john@example.com" };
    NAssert.That(user.Email, Contains.Substring("@"));
}
```

---

## Prevention Checklist

- [ ] Added `using PeasyPilot.[Framework];` statement
- [ ] Project references correct PeasyPilot package
- [ ] Method exists in target framework's Assert
- [ ] Async tests use `await` on async assertions
- [ ] Exception assertions include the throwing code in lambda
- [ ] Objects compared implement `Equals()` correctly
- [ ] One logical assertion per test
- [ ] Test name matches what it asserts

## Getting Help

- Check framework documentation (NUnit, xUnit, TUnit)
- Review assertion method signatures in IntelliSense
- Enable Debug logging to trace execution
- Use IDE debugger to step through assertions
