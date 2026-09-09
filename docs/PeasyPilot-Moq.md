# PeasyPilot.Moq

Mock factory abstractions for Moq.

```csharp
using PeasyPilot.Moq;

var factory = new MockFactory();
var mockRepo = factory.Create<IUserRepository>();
var mockLogger = factory.Create<ILogger>();
```

## Features
- ✅ Factory abstraction
- ✅ Type-safe mocking
- ✅ Automatic setup
- ✅ Behavior verification

## Install
```bash
dotnet add package PeasyPilot.Moq
```

**Uses:** Moq library
