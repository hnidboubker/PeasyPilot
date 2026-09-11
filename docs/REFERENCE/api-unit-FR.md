# Référence API PeasyPilot.Unit

## Aperçu

`PeasyPilot.Unit` fournit des utilitaires orientés builder et des fixtures partagées conçus spécifiquement pour les flux de test unitaire. Il simplifie la création d'objets de test, gère les données de test communes et fournit une base cohérente pour les fixtures de test unitaire sur les frameworks xUnit, NUnit et TUnit.

**Responsabilités principales :**
- Implémentation du pattern builder pour la création d'objets de test
- Classe de base de fixture de test unitaire
- Génération et validation de données de test
- Méthodes d'extension de builder
- API fluente pour construire les scénarios de test

**Cibles :** .NET 8.0, 9.0, 10.0

---

## Abstractions Principales

### BuilderBase<T>

Classe de base pour implémenter le pattern builder pour la création d'objets de test.

```csharp
namespace PeasyPilot.Unit.Builders;

/// <summary>
/// Classe de base pour construire des objets de test en utilisant le pattern builder.
/// </summary>
/// <typeparam name="T">Le type d'objet en cours de construction.</typeparam>
public abstract class BuilderBase<T> where T : class
{
    /// <summary>
    /// Obtient ou définit l'instance en cours de construction.
    /// </summary>
    protected T Instance { get; set; } = Activator.CreateInstance<T>()!;

    /// <summary>
    /// Construit et retourne l'instance.
    /// </summary>
    /// <returns>L'instance construite.</returns>
    public virtual T Build() => Instance;

    /// <summary>
    /// Réinitialise le builder à un état frais.
    /// </summary>
    /// <returns>Cette instance builder pour le chaînage.</returns>
    public virtual BuilderBase<T> Reset()
    {
        Instance = Activator.CreateInstance<T>()!;
        return this;
    }
}
```

**Objectif :** Fournit un pattern builder fluent pour la création d'objets de test avec capacité de réinitialisation.

**Exemple : Créer un Builder Personnalisé**
```csharp
using PeasyPilot.Unit.Builders;

public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; } = true;
}

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

    public UserBuilder WithAge(int age)
    {
        Instance.Age = age;
        return this;
    }

    public UserBuilder AsInactive()
    {
        Instance.IsActive = false;
        return this;
    }

    public new UserBuilder Reset()
    {
        base.Reset();
        return this;
    }
}

// Utilisation :
[Fact]
public void TestUserBuilder()
{
    var builder = new UserBuilder();
    
    var user1 = builder
        .WithName("John Doe")
        .WithEmail("john@example.com")
        .WithAge(30)
        .Build();

    Assert.Equal("John Doe", user1.Name);

    // Réinitialiser et construire un utilisateur différent
    var user2 = builder
        .Reset()
        .WithName("Jane Smith")
        .WithEmail("jane@example.com")
        .Build();

    Assert.Equal("Jane Smith", user2.Name);
}
```

---

## Classes Principales

### UnitTestFixture

Classe de base pour les tests unitaires fournissant une configuration commune et des utilitaires.

```csharp
namespace PeasyPilot.Unit.Fixtures;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

/// <summary>
/// Fixture de base pour les tests unitaires fournissant une configuration commune et des utilitaires.
/// </summary>
public abstract class UnitTestFixture
{
    /// <summary>
    /// Obtient le contexte de test pour cette fixture.
    /// </summary>
    protected ITestContext TestContext { get; }

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="UnitTestFixture"/>.
    /// </summary>
    protected UnitTestFixture()
    {
        TestContext = new TestContext();
    }

    /// <summary>
    /// Obtient ou crée une valeur dans le contexte de test.
    /// </summary>
    /// <typeparam name="T">Le type de la valeur.</typeparam>
    /// <param name="key">La clé de cache.</param>
    /// <param name="factory">La fonction factory pour créer la valeur.</param>
    /// <returns>La valeur en cache ou nouvellement créée.</returns>
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) where T : class
    {
        return TestContext.GetOrAdd(key, factory);
    }
}
```

**Objectif :** Fournit un contexte de test thread-safe et le partage de données entre les méthodes de test unitaire.

**Exemple : Créer une Fixture de Test Unitaire**
```csharp
using PeasyPilot.Unit.Fixtures;
using PeasyPilot.Core.Abstractions;

public class TestsCalculatrice : UnitTestFixture
{
    [Fact]
    public void Add_DeuxNombres_RetourneLaSomme()
    {
        // Obtenir ou créer une instance de calculatrice
        var calculator = GetOrCreateTestData("calc", () => new Calculator());

        var result = calculator.Add(2, 3);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Subtract_DeuxNombres_RetourneLaDifférence()
    {
        // Réutilise la même instance de calculatrice
        var calculator = GetOrCreateTestData("calc", () => new Calculator());

        var result = calculator.Subtract(5, 3);

        Assert.Equal(2, result);
    }
}

public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
}
```

---

## Classes d'Assistance

### TestDataHelper

Méthodes d'assistance statiques pour les opérations de données de test.

```csharp
namespace PeasyPilot.Unit.Helpers;

/// <summary>
/// Méthodes d'assistance pour les opérations de test unitaire.
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Valide qu'un objet n'est pas nul et remplit les critères de base.
    /// </summary>
    /// <typeparam name="T">Le type de l'objet.</typeparam>
    /// <param name="obj">L'objet à valider.</param>
    /// <returns>Vrai si valide ; sinon, faux.</returns>
    public static bool IsValidTestObject<T>(T? obj) where T : class
    {
        return obj != null;
    }

    /// <summary>
    /// Crée une copie superficielle de l'objet donné en utilisant la réflexion.
    /// </summary>
    /// <typeparam name="T">Le type de l'objet.</typeparam>
    /// <param name="obj">L'objet à copier.</param>
    /// <returns>Une copie superficielle de l'objet.</returns>
    public static T ShallowCopy<T>(T obj) where T : class
    {
        var type = obj.GetType();
        if (type.IsValueType)
            return obj;

        var copy = Activator.CreateInstance(type) as T;
        if (copy == null)
            throw new InvalidOperationException($"Échec de la création d'une copie de {typeof(T).Name}");

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(obj);
                property.SetValue(copy, value);
            }
        }

        return copy;
    }
}
```

**Objectif :** Méthodes utilitaires courantes pour la manipulation et la validation des données de test.

**Exemple : Utiliser TestDataHelper**
```csharp
using PeasyPilot.Unit.Helpers;

public class TestsTestDataHelper
{
    [Fact]
    public void IsValidTestObject_AvecObjetValide_RetournelVrai()
    {
        var user = new User { Name = "John" };

        bool isValid = TestDataHelper.IsValidTestObject(user);

        Assert.True(isValid);
    }

    [Fact]
    public void IsValidTestObject_AvecNull_RetourneFaux()
    {
        User? user = null;

        bool isValid = TestDataHelper.IsValidTestObject(user);

        Assert.False(isValid);
    }

    [Fact]
    public void ShallowCopy_CopieObjet_SanModifierL'Original()
    {
        var original = new User { Name = "John", Age = 30 };

        var copy = TestDataHelper.ShallowCopy(original);
        copy.Name = "Jane";

        Assert.Equal("John", original.Name);
        Assert.Equal("Jane", copy.Name);
    }
}

public class User
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

---

## Méthodes d'Extension

### BuilderExtensions

Méthodes d'extension pour la fluidité et la chaînabilité du builder.

```csharp
namespace PeasyPilot.Unit.Extensions;

/// <summary>
/// Méthodes d'extension pour les builders.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    /// Crée une nouvelle instance d'un builder.
    /// </summary>
    /// <typeparam name="TBuilder">Le type du builder.</typeparam>
    /// <returns>Une nouvelle instance de builder.</returns>
    public static TBuilder Create<TBuilder>() where TBuilder : class, new()
    {
        return new TBuilder();
    }

    /// <summary>
    /// Enchaîne plusieurs configurations de builder ensemble.
    /// </summary>
    /// <typeparam name="T">Le type du builder.</typeparam>
    /// <param name="builder">L'instance du builder.</param>
    /// <param name="configure">L'action de configuration.</param>
    /// <returns>Le builder configuré.</returns>
    public static T Configure<T>(this T builder, Action<T> configure) where T : class
    {
        configure(builder);
        return builder;
    }
}
```

**Exemple : Utiliser BuilderExtensions**
```csharp
using PeasyPilot.Unit.Extensions;

public class CommandeBuilder : BuilderBase<Commande>
{
    public CommandeBuilder WithItems(int count)
    {
        Instance.ItemCount = count;
        return this;
    }

    public CommandeBuilder WithTotal(decimal amount)
    {
        Instance.Total = amount;
        return this;
    }

    public new CommandeBuilder Reset()
    {
        base.Reset();
        return this;
    }
}

public class Commande
{
    public int ItemCount { get; set; }
    public decimal Total { get; set; }
}

// Utilisation avec les extensions :
[Fact]
public void BuildCommande_AvecExtensions()
{
    var builder = BuilderExtensions.Create<CommandeBuilder>();
    
    var commande = builder
        .Configure(b => b.WithItems(5).WithTotal(99.99m))
        .Build();

    Assert.Equal(5, commande.ItemCount);
    Assert.Equal(99.99m, commande.Total);
}
```

---

## Motifs Courants

### Motif 1 : Builder avec Valeurs Par Défaut

```csharp
using PeasyPilot.Unit.Builders;

public class ProduitBuilder : BuilderBase<Produit>
{
    public override Produit Build()
    {
        // Définir les valeurs par défaut si non fournie
        if (Instance.Name == string.Empty)
            Instance.Name = "Produit Par Défaut";
        
        if (Instance.Price <= 0)
            Instance.Price = 9.99m;

        return base.Build();
    }

    public ProduitBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }

    public ProduitBuilder WithPrice(decimal price)
    {
        Instance.Price = price;
        return this;
    }
}

public class Produit
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

[Fact]
public void ProduitBuilder_AppliqueLeValeurParDéfaut()
{
    var produit = new ProduitBuilder().Build();

    Assert.Equal("Produit Par Défaut", produit.Name);
    Assert.Equal(9.99m, produit.Price);
}
```

### Motif 2 : Fixture avec Configuration Partagée

```csharp
using PeasyPilot.Unit.Fixtures;

public class TestsServiceUtilisateur : UnitTestFixture
{
    private IServiceUtilisateur _service;
    private List<Utilisateur> _testUsers;

    public TestsServiceUtilisateur()
    {
        ConfigurerLesDonnéesPartagées();
    }

    private void ConfigurerLesDonnéesPartagées()
    {
        _service = GetOrCreateTestData("service", () =>
            new ServiceUtilisateur());

        _testUsers = GetOrCreateTestData("users", () =>
            new List<Utilisateur>
            {
                new Utilisateur { Id = 1, Name = "Alice" },
                new Utilisateur { Id = 2, Name = "Bob" }
            });
    }

    [Fact]
    public void GetUser_RetourneL'UtilisateurExistant()
    {
        var user = _service.GetUser(1);

        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
    }

    [Fact]
    public void CreateUser_AjouteNouvelUtilisateur()
    {
        var newUser = new Utilisateur { Id = 3, Name = "Charlie" };
        _service.AddUser(newUser);

        Assert.Contains(_testUsers, u => u.Name == "Charlie");
    }
}

public class Utilisateur
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IServiceUtilisateur
{
    Utilisateur? GetUser(int id);
    void AddUser(Utilisateur user);
}
```

### Motif 3 : Builder pour Objets Complexes

```csharp
using PeasyPilot.Unit.Builders;

public class DemandeurHttpBuilder : BuilderBase<DemandeHttp>
{
    public DemandeurHttpBuilder WithMethod(string method)
    {
        Instance.Method = method;
        return this;
    }

    public DemandeurHttpBuilder WithPath(string path)
    {
        Instance.Path = path;
        return this;
    }

    public DemandeurHttpBuilder WithHeader(string key, string value)
    {
        Instance.Headers ??= new Dictionary<string, string>();
        Instance.Headers[key] = value;
        return this;
    }

    public DemandeurHttpBuilder WithBody(string body)
    {
        Instance.Body = body;
        return this;
    }

    public new DemandeurHttpBuilder Reset()
    {
        base.Reset();
        Instance.Headers = null;
        return this;
    }
}

public class DemandeHttp
{
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = "/";
    public Dictionary<string, string>? Headers { get; set; }
    public string Body { get; set; } = string.Empty;
}

[Fact]
public void DemandeurHttpBuilder_ConstruitDemandeComplexe()
{
    var request = new DemandeurHttpBuilder()
        .WithMethod("POST")
        .WithPath("/api/users")
        .WithHeader("Content-Type", "application/json")
        .WithHeader("Authorization", "Bearer token123")
        .WithBody("{\"name\": \"John\"}")
        .Build();

    Assert.Equal("POST", request.Method);
    Assert.Equal("/api/users", request.Path);
    Assert.Equal(2, request.Headers?.Count);
    Assert.NotEmpty(request.Body);
}
```

---

## Configuration

### Configuration de l'Injection de Dépendances

```csharp
using Microsoft.Extensions.DependencyInjection;

public class ConfigurationTestUnitaire
{
    public static IServiceCollection AddUnitTestSupport(
        this IServiceCollection services)
    {
        // Aucun enregistrement explicite requis - les fixtures et les builders 
        // sont utilisés directement sans conteneur DI
        return services;
    }
}
```

---

## Résumé de Référence

| Composant | Objectif | Utilisation |
|-----------|----------|------------|
| BuilderBase<T> | Classe de base pour le pattern builder | Héritage pour les builders personnalisés |
| UnitTestFixture | Fixture de base pour les tests unitaires | Héritage pour les classes de test |
| TestDataHelper | Méthodes utilitaires pour les données de test | Appels de méthode statique |
| BuilderExtensions | Méthodes d'extension pour les builders | Appels de méthode d'extension |

---

## Référence Rapide : Pattern Builder Courant

```csharp
// 1. Créer un builder personnalisé
public class MyObjectBuilder : BuilderBase<MyObject>
{
    public MyObjectBuilder WithProperty(string value)
    {
        Instance.Property = value;
        return this;
    }
}

// 2. Utiliser l'API fluente
var obj = new MyObjectBuilder()
    .WithProperty("value1")
    .WithProperty("value2")
    .Build();

// 3. Réinitialiser et réutiliser
var obj2 = new MyObjectBuilder()
    .Reset()
    .WithProperty("different")
    .Build();
```

---

## Voir Aussi

- **GETTING-STARTED-FR.md** — Guide de démarrage rapide
- **unit-testing-guide-FR.md** — Guide complet des tests unitaires
- **api-core-FR.md** — API PeasyPilot.Core
- **api-integration-FR.md** — API PeasyPilot.Integration
