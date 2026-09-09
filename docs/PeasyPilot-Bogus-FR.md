# PeasyPilot.Bogus

Génération de fausses données via Bogus.

```csharp
using PeasyPilot.Bogus;

var factory = new TestDataFactory();
var user = factory.Create<User>();
var users = factory.CreateMany<User>(5);
```

## Fonctionnalités
- ✅ Données réalistes
- ✅ Règles personnalisables
- ✅ Support seed
- ✅ Génération collections

## Installation
```bash
dotnet add package PeasyPilot.Bogus
```

**Utilise :** Bibliothèque Bogus
