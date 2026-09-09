# PeasyPilot.Unit

Patterns Builder et utilitaires de test pour les tests unitaires.

```csharp
using PeasyPilot.Unit.Builders;

public class UserBuilder : BuilderBase<User>
{
    public UserBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }
}

var user = new UserBuilder()
    .WithName("Alice")
    .Build();
```

## Fonctionnalités
- ✅ Pattern Builder
- ✅ Factories d'objets de test
- ✅ Configuration fluente
- ✅ Helpers réutilisables

## Installation
```bash
dotnet add package PeasyPilot.Unit
```
