# PeasyPilot.Unit

Builder patterns and test utilities for unit testing.

```csharp
using PeasyPilot.Unit.Builders;

public class UserBuilder : BuilderBase<User>
{
    public UserBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        Instance.Email = email;
        return this;
    }
}

var user = new UserBuilder()
    .WithName("Alice")
    .WithEmail("alice@example.com")
    .Build();
```

## Features
- ✅ Builder pattern support
- ✅ Test object factories
- ✅ Fluent configuration
- ✅ Reusable test helpers

## Install
```bash
dotnet add package PeasyPilot.Unit
```
