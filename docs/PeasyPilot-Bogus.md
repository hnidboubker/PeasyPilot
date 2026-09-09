# PeasyPilot.Bogus

Fake data generation via Bogus.

```csharp
using PeasyPilot.Bogus;

var factory = new TestDataFactory();
var user = factory.Create<User>();
var users = factory.CreateMany<User>(5);
```

## Features
- ✅ Realistic fake data
- ✅ Customizable rules
- ✅ Seeding support
- ✅ Collection generation

## Install
```bash
dotnet add package PeasyPilot.Bogus
```

**Uses:** Bogus library
