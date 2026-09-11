# Référence API PeasyPilot.Bogus

## Aperçu

`PeasyPilot.Bogus` fournit une abstraction de génération de données simulées rationalisant construite sur la populaire bibliothèque Bogus. Il simplifie la création de données de test en offrant une interface indépendante du framework qui abstrait la complexité de l'API Bogus et permet une intégration transparente avec les tests unitaires, les tests d'intégration et les scénarios BDD.

**Responsabilités principales :**
- Abstraction de génération de données simulées via ITestDataFactory
- Intégration de bibliothèque Bogus sans couplage strict
- Génération de données d'une seule instance et de collections
- Motifs de création de données de test indépendants du framework
- Support pour toutes les capacités de générateur Bogus
- Intégration transparente du conteneur DI
- Personnalisation et graine de données de test

**Cibles :** .NET 8.0, 9.0, 10.0

**Dépendances :** Bogus 35.x+ (abstraite)

---

## Abstractions Principales

### ITestDataFactory

Interface centrale pour générer de données simulées de test de manière indépendante du framework.

```csharp
namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Usine pour créer de données simulées de test.
/// </summary>
public interface ITestDataFactory
{
    /// <summary>
    /// Crée une seule instance simulée du type spécifié.
    /// </summary>
    /// <typeparam name="T">Le type à générer.</typeparam>
    /// <returns>Une instance simulée.</returns>
    T Create<T>() where T : class;

    /// <summary>
    /// Crée plusieurs instances simulées du type spécifié.
    /// </summary>
    /// <typeparam name="T">Le type à générer.</typeparam>
    /// <param name="count">Le nombre d'instances à générer.</param>
    /// <returns>Une collection d'instances simulées.</returns>
    IReadOnlyCollection<T> CreateMany<T>(int count) where T : class;
}
```

**Objectif :** Fournit un point d'abstraction unique pour toute génération de données simulées, permettant les implémentations interchangeables (Bogus, AutoFixture, etc.) sans changer le code de test.

---

### TestDataFactory

Implémentation concrète utilisant Bogus 35.x+.

```csharp
namespace PeasyPilot.Bogus;

/// <summary>
/// Usine de données de test par défaut utilisant Bogus pour générer des données de test aléatoires.
///
/// Important : Bogus remplit uniquement les propriétés explicitement configurées via RuleFor().
/// Les propriétés sans règles restent à leurs valeurs par défaut (chaîne vide, 0, null, faux).
///
/// Pour remplir toutes les propriétés, configurez les règles explicitement lors de la création d'instances.
/// </summary>
public class TestDataFactory : ITestDataFactory
{
    /// <summary>
    /// Crée une seule instance simulée du type spécifié.
    /// </summary>
    public T Create<T>() where T : class => new Faker<T>().Generate();

    /// <summary>
    /// Crée plusieurs instances simulées du type spécifié.
    /// </summary>
    public IReadOnlyCollection<T> CreateMany<T>(int count) where T : class
    {
        var faker = new Faker<T>();
        return Enumerable.Range(0, count).Select(_ => faker.Generate()).ToList();
    }
}
```

**Objectif :** Utilise `Faker<T>` de Bogus pour générer des données simulées réalistes avec une configuration minimale, parfait pour les tests unitaires et d'intégration.

---

## Motifs Principaux

### Intégration Bogus Basique

TestDataFactory enveloppe `Faker<T>` de Bogus pour fournir une interface simple et sans état :

```csharp
// Usage basique - toutes les propriétés non configurées obtiennent des valeurs par défaut
var faker = new Faker<User>();
var user = faker.Generate();

// Configuration personnalisée - RuleFor() pour remplir des propriétés spécifiques
var configuredFaker = new Faker<User>()
    .RuleFor(u => u.Name, f => f.Person.FullName())
    .RuleFor(u => u.Email, f => f.Internet.Email())
    .RuleFor(u => u.Age, f => f.Random.Int(18, 65));
var customUser = configuredFaker.Generate();
```

### Localisation du Générateur Faker

Bogus supporte plusieurs locales pour des données réalistes :

```csharp
// Générer des noms et adresses français
var frFaker = new Faker<User>("fr_FR");

// Générer des données allemandes
var deFaker = new Faker<User>("de_DE");

// Générer des données japonaises
var jpFaker = new Faker<User>("ja_JP");
```

### Intégration avec les Conteneurs DI

```csharp
public static class TestDataFactoryServiceExtensions
{
    /// <summary>
    /// Enregistre l'implémentation ITestDataFactory.
    /// </summary>
    public static IServiceCollection AddTestDataFactory(
        this IServiceCollection services)
    {
        services.AddSingleton<ITestDataFactory, TestDataFactory>();
        return services;
    }
}
```

---

## Exemples Concrets

### Exemple 1 : Génération Basique de Données Simulées

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Core.Abstractions;

public class BasicFakeDataTest
{
    private readonly ITestDataFactory _factory = new TestDataFactory();

    [Fact]
    public void TestGenerateSingleUser()
    {
        var user = _factory.Create<User>();

        Assert.NotNull(user);
        // Les propriétés sans RuleFor explicite auront des valeurs par défaut
    }

    [Fact]
    public void TestGenerateMultipleUsers()
    {
        var users = _factory.CreateMany<User>(5);

        Assert.Equal(5, users.Count);
        Assert.All(users, u => Assert.NotNull(u));
    }

    [Fact]
    public void TestGenerateDifferentTypes()
    {
        var user = _factory.Create<User>();
        var product = _factory.Create<Product>();
        var order = _factory.Create<Order>();

        Assert.NotNull(user);
        Assert.NotNull(product);
        Assert.NotNull(order);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public decimal Total { get; set; }
}
```

### Exemple 2 : Configuration Personnalisée du Générateur

```csharp
using Bogus;

public class CustomFakerConfigurationTest
{
    [Fact]
    public void TestConfiguredUserData()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.IndexFaker)
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80));

        var user = userFaker.Generate();

        Assert.NotEmpty(user.Name);
        Assert.NotEmpty(user.Email);
        Assert.InRange(user.Age, 18, 80);
    }

    [Fact]
    public void TestConfiguredProductData()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 1000)));

        var product = productFaker.Generate();

        Assert.NotEmpty(product.Name);
        Assert.True(product.Price > 0);
    }

    [Fact]
    public void TestConfiguredOrderData()
    {
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker)
            .RuleFor(o => o.UserId, f => f.Random.Int(1, 1000))
            .RuleFor(o => o.ProductIds, f => 
                f.Make(f.Random.Int(1, 5), () => f.Random.Int(1, 500)).ToList())
            .RuleFor(o => o.Total, f => decimal.Parse(f.Commerce.Price(10, 10000)));

        var order = orderFaker.Generate();

        Assert.NotEmpty(order.ProductIds);
        Assert.True(order.Total > 0);
    }
}
```

### Exemple 3 : Générateur Faker avec Injection de Dépendances

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public class FakerDiTest
{
    [Fact]
    public void TestTestDataFactoryWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestDataFactory, TestDataFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<ITestDataFactory>();

        var user = factory.Create<User>();
        var users = factory.CreateMany<User>(3);

        Assert.NotNull(user);
        Assert.Equal(3, users.Count);
    }

    [Fact]
    public void TestInjectTestDataFactoryIntoService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestDataFactory, TestDataFactory>();
        services.AddScoped<UserTestDataService>();
        var serviceProvider = services.BuildServiceProvider();

        var userService = serviceProvider.GetRequiredService<UserTestDataService>();

        var user = userService.GenerateTestUser();

        Assert.NotNull(user);
    }
}

public class UserTestDataService
{
    private readonly ITestDataFactory _factory;

    public UserTestDataService(ITestDataFactory factory)
    {
        _factory = factory;
    }

    public User GenerateTestUser() => _factory.Create<User>();

    public IReadOnlyCollection<User> GenerateTestUsers(int count) 
        => _factory.CreateMany<User>(count);
}
```

### Exemple 4 : Génération de Données Internet

```csharp
using Bogus;

public class InternetDataTest
{
    [Fact]
    public void TestInternetFakerData()
    {
        var faker = new Faker();

        var email = faker.Internet.Email();
        var username = faker.Internet.UserName();
        var password = faker.Internet.Password();
        var ipAddress = faker.Internet.IpAddress();
        var url = faker.Internet.Url();
        var domain = faker.Internet.DomainName();

        Assert.NotEmpty(email);
        Assert.Contains("@", email);
        Assert.NotEmpty(username);
        Assert.NotEmpty(password);
        Assert.NotEmpty(ipAddress);
        Assert.NotEmpty(url);
        Assert.Contains(".", domain);
    }

    [Fact]
    public void TestPersonFakerData()
    {
        var faker = new Faker();

        var firstName = faker.Person.FirstName();
        var lastName = faker.Person.LastName();
        var fullName = faker.Person.FullName();
        var email = faker.Person.Email;
        var phone = faker.Person.Phone;

        Assert.NotEmpty(firstName);
        Assert.NotEmpty(lastName);
        Assert.NotEmpty(fullName);
        Assert.NotEmpty(email);
        Assert.NotEmpty(phone);
    }
}
```

### Exemple 5 : Génération de Données Commerce

```csharp
using Bogus;

public class CommerceDataTest
{
    [Fact]
    public void TestCommerceData()
    {
        var faker = new Faker();

        var productName = faker.Commerce.ProductName();
        var productDescription = faker.Commerce.ProductDescription();
        var department = faker.Commerce.Department();
        var price = faker.Commerce.Price(10, 500);
        var category = faker.Commerce.Categories(1).First();

        Assert.NotEmpty(productName);
        Assert.NotEmpty(productDescription);
        Assert.NotEmpty(department);
        Assert.NotEmpty(price);
        Assert.NotEmpty(category);
    }

    [Fact]
    public void TestGenerateProductCatalog()
    {
        var faker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(5, 500)));

        var products = Enumerable.Range(0, 10)
            .Select(_ => faker.Generate())
            .ToList();

        Assert.Equal(10, products.Count);
        Assert.All(products, p => 
        {
            Assert.NotEmpty(p.Name);
            Assert.True(p.Price > 0);
        });
    }
}
```

### Exemple 6 : Données de Date et Heure

```csharp
using Bogus;

public class DateTimeDataTest
{
    [Fact]
    public void TestDateTimeGeneration()
    {
        var faker = new Faker();

        var pastDate = faker.Date.Past();
        var futureDate = faker.Date.Future();
        var recentDate = faker.Date.Recent();
        var birthdayDate = faker.Date.Between(DateTime.Now.AddYears(-80), DateTime.Now.AddYears(-18));

        Assert.True(pastDate < DateTime.Now);
        Assert.True(futureDate > DateTime.Now);
        Assert.True(recentDate >= DateTime.Now.AddDays(-7));
        Assert.InRange(birthdayDate.Year, DateTime.Now.Year - 80, DateTime.Now.Year - 18);
    }

    [Fact]
    public void TestTimeDataGeneration()
    {
        var faker = new Faker();

        var randomTime = faker.Date.Soon().TimeOfDay;
        var randomTimespan = faker.Date.Timespan();

        Assert.NotEqual(default(TimeSpan), randomTime);
        Assert.NotEqual(default(TimeSpan), randomTimespan);
    }
}
```

### Exemple 7 : Générateur Avec Graine pour Reproductibilité

```csharp
using Bogus;

public class SeededFakerTest
{
    [Fact]
    public void TestSeededFakerReproducibility()
    {
        Randomizer.Seed(12345);
        var faker1 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email());
        var user1 = faker1.Generate();

        Randomizer.Seed(12345);
        var faker2 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email());
        var user2 = faker2.Generate();

        // La même graine produit les mêmes données
        Assert.Equal(user1.Name, user2.Name);
        Assert.Equal(user1.Email, user2.Email);
    }

    [Fact]
    public void TestDifferentSeedsProduceDifferentData()
    {
        Randomizer.Seed(111);
        var faker1 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName());
        var user1 = faker1.Generate();

        Randomizer.Seed(222);
        var faker2 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName());
        var user2 = faker2.Generate();

        // Les graines différentes produisent des données différentes
        Assert.NotEqual(user1.Name, user2.Name);
    }
}
```

### Exemple 8 : Générateur Localisé

```csharp
using Bogus;

public class LocalizedFakerTest
{
    [Fact]
    public void TestFrenchFakerData()
    {
        var faker = new Faker("fr_FR");

        var firstName = faker.Person.FirstName();
        var lastName = faker.Person.LastName();
        var email = faker.Internet.Email();

        // Noms français et données
        Assert.NotEmpty(firstName);
        Assert.NotEmpty(lastName);
        Assert.NotEmpty(email);
    }

    [Fact]
    public void TestGermanFakerData()
    {
        var faker = new Faker("de_DE");

        var firstName = faker.Person.FirstName();
        var company = faker.Company.CompanyName();

        // Noms allemands et données de compagnie
        Assert.NotEmpty(firstName);
        Assert.NotEmpty(company);
    }

    [Fact]
    public void TestJapaneseFakerData()
    {
        var faker = new Faker("ja_JP");

        var firstName = faker.Person.FirstName();
        var lastName = faker.Person.LastName();

        // Noms japonais
        Assert.NotEmpty(firstName);
        Assert.NotEmpty(lastName);
    }
}
```

### Exemple 9 : Génération d'Objets Complexes

```csharp
using Bogus;

public class ComplexObjectGenerationTest
{
    [Fact]
    public void TestGenerateComplexOrder()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 1000)));

        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker)
            .RuleFor(o => o.UserId, f => f.Random.Int(1, 1000))
            .RuleFor(o => o.ProductIds, f => 
                f.Make(f.Random.Int(1, 5), () => f.Random.Int(1, 500)).ToList())
            .RuleFor(o => o.Total, f => decimal.Parse(f.Commerce.Price(50, 10000)));

        var order = orderFaker.Generate();

        Assert.NotEmpty(order.ProductIds);
        Assert.True(order.Total > 0);
    }
}
```

### Exemple 10 : Test d'Intégration avec Factory de Données de Test

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Integration;

public class IntegrationTestWithTestDataFactory
{
    [Fact]
    public async Task TestCreateUserWithFakedData()
    {
        var factory = new TestDataFactory();
        
        // Générer des données de test
        var testUser = factory.Create<User>();
        var moreUsers = factory.CreateMany<User>(5);

        // Utiliser dans le test d'intégration
        Assert.NotNull(testUser);
        Assert.Equal(5, moreUsers.Count);
    }

    [Fact]
    public async Task TestBulkCreateWithFakeData()
    {
        var factory = new TestDataFactory();
        
        var testUsers = factory.CreateMany<User>(100);

        Assert.Equal(100, testUsers.Count);
        Assert.All(testUsers, u => Assert.NotNull(u));
    }
}
```

---

## Bonnes Pratiques

1. **Utiliser l'Interface ITestDataFactory :** Injecter toujours `ITestDataFactory` pour la flexibilité
2. **Configurer les Règles Explicitement :** Utiliser `RuleFor()` pour les propriétés qui importent
3. **Garder les Défauts Simples :** S'appuyer sur les défauts Bogus pour les propriétés non configurées
4. **Graine pour Reproductibilité :** Utiliser `Randomizer.Seed()` quand des données déterministes sont nécessaires
5. **Exploiter les Locales :** Utiliser différentes locales pour tester des scénarios internationaux
6. **Éviter la Sur-Configuration :** Configurer uniquement ce que le test utilise réellement

---

## Considérations de Performances

- **Création d'Objet :** Rapide (microsecondes par objet pour les types simples)
- **Génération de Collection :** Mise à l'échelle linéaire avec le paramètre count
- **Mémoire :** Surcharge minimale; génère des milliers d'objets efficacement
- **Graine :** Coût de configuration unique; aucun impact sur la génération suivante
- **Localisation :** Différence de performance minimale entre les locales

---

## Fournisseurs Faker Courants

```
faker.Person          # Noms, emails, numéros de téléphone, dates d'anniversaire
faker.Internet        # URLs, noms d'utilisateur, mots de passe, adresses IP
faker.Commerce        # Produits, prix, catégories, départements
faker.Company         # Noms de compagnie, phrases d'accroche, déclarations BS
faker.Date            # Dates passées/futures/récentes, timespan
faker.Random          # Nombres aléatoires, booléens, éléments d'tableaux
faker.Finance         # Numéros de compte, cartes de crédit, codes IBAN
faker.Address         # Rues, villes, pays, codes postaux
faker.Phone           # Numéros de téléphone (différents formats)
faker.Email           # Emails, noms d'utilisateur
faker.Lorem           # Mots lorem ipsum, phrases, paragraphes
faker.Hacker          # Vocabulaire hacker (abbr, adjectif, nom)
```

---

## Voir Aussi

- [API PeasyPilot.Core](api-core-FR.md)
- [API PeasyPilot.Unit](api-unit-FR.md)
- [Guide de Test Unitaire](../GUIDES/unit-testing-guide-FR.md)
- [Documentation Bogus](https://github.com/bchavez/Bogus)

