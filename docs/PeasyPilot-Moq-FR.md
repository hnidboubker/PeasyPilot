# PeasyPilot.Moq

Abstractions de factory de mock pour Moq.

```csharp
using PeasyPilot.Moq;

var factory = new MockFactory();
var mockRepo = factory.Create<IUserRepository>();
var mockLogger = factory.Create<ILogger>();
```

## Fonctionnalités
- ✅ Abstraction factory
- ✅ Mocking type-safe
- ✅ Setup automatique
- ✅ Vérification comportement

## Installation
```bash
dotnet add package PeasyPilot.Moq
```

**Utilise :** Bibliothèque Moq
