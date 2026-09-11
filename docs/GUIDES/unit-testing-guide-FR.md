# Guide des Tests Unitaires

## Aperçu

Dans ce guide, vous apprendrez à :
- Écrire des tests unitaires propres et focalisés avec PeasyPilot
- Utiliser les assertions fluides efficacement
- Appliquer le pattern builder pour les données de test
- Intégrer le mocking avec Moq
- Structurer les tests selon les meilleures pratiques
- Tester les cas limites et les conditions d'erreur

**Prérequis:** Terminer [Premiers pas](../GETTING-STARTED-FR.md)  
**Durée estimée:** 30 minutes  
**Frameworks:** xUnit, NUnit, TUnit  
**Exemples de code:** 8 exemples fonctionnels

---

## Pourquoi les Tests Unitaires Importent

Les tests unitaires sont la première ligne de défense de votre stratégie de test. Ils vérifient que les méthodes et classes individuelles fonctionnent correctement en isolation, détectant les bugs au début et fournissant une confiance lors du refactorisation.

Un test unitaire bien écrit est :
- **Rapide** – S'exécute en millisecondes, sans appels à la base de données
- **Isolé** – Teste un comportement à la fois
- **Répétable** – Même résultat à chaque fois
- **Auto-vérificateur** – Pas de vérification manuelle nécessaire
- **Maintenable** – L'intention est claire, facile à comprendre

---

## Concepts Clés

### 1. Pattern Arrange-Act-Assert (AAA)

Chaque test unitaire suit trois phases distinctes :

```csharp
[Fact]
public void Add_WithPositiveNumbers_ReturnSum()
{
    // ARRANGE: Configurer les données de test et les préconditions
    var calculator = new Calculator();
    int a = 5;
    int b = 3;
    int expected = 8;
    
    // ACT: Exécuter le code testé
    int result = calculator.Add(a, b);
    
    // ASSERT: Vérifier le résultat
    Assert.Equal(expected, result);
}
```

**Arrange** prépare. **Act** exécute. **Assert** valide.

### 2. Convention de Nommage

Les noms de test doivent clairement décrire ce qui est testé et le résultat attendu :

```
[MethodName]_[Condition]_[ExpectedBehavior]
```

**Bons exemples:**
- `Add_WithPositiveNumbers_ReturnsSum`
- `Divide_ByZero_ThrowsArgumentException`
- `GetUser_WithValidId_ReturnsUser`

### 3. Responsabilité Unique

Chaque test doit vérifier **un seul comportement**. Si un test a plusieurs assertions, demandez-vous : "Ces assertions testent-elles le même comportement ?"

---

## Bien Démarrer: Tests Unitaires de Base

### Exemple 1: Simple Assertion

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MonApp.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    [Fact]
    public void Add_WithTwoNumbers_ReturnSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        int result = calculator.Add(5, 3);
        
        // Assert
        Assert.Equal(8, result);
    }
}
```

Exécutez avec :
```bash
dotnet test
```

### Exemple 2: Assertions Multiples Liées

```csharp
[Fact]
public void CreateUser_WithValidEmail_ReturnsUserWithEmailSet()
{
    var userService = new UserService();
    string email = "john@example.com";
    
    var user = userService.CreateUser(email);
    
    Assert.NotNull(user);
    Assert.Equal(email, user.Email);
    Assert.True(user.IsActive);
}
```

### Exemple 3: Tester les Exceptions

```csharp
[Fact]
public void WithdrawMoney_InsufficientFunds_ThrowsException()
{
    var account = new BankAccount(balance: 100);
    
    var exception = Assert.Throws<InsufficientFundsException>(
        () => account.Withdraw(500)
    );
    
    Assert.Equal("Solde insuffisant.", exception.Message);
}
```

### Exemple 4: Pattern Builder pour Objets Complexes

```csharp
public class UserBuilder
{
    private string _email = "test@example.com";
    private string _name = "Test User";
    private bool _isActive = true;
    
    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithName(string name) { _name = name; return this; }
    public UserBuilder Inactive() { _isActive = false; return this; }
    
    public User Build() => new User(_email, _name) { IsActive = _isActive };
}

// Utilisation :
[Fact]
public void SendWelcomeEmail_WithInactiveUser_DoesNotSend()
{
    var user = new UserBuilder()
        .WithEmail("john@example.com")
        .Inactive()
        .Build();
    
    var emailService = new EmailService();
    emailService.SendWelcomeEmail(user);
    
    Assert.False(emailService.WasEmailSent(user.Email));
}
```

### Exemple 5: Mocking avec Moq

```csharp
using PeasyPilot.Moq;

[Fact]
public void CreateOrder_CallsInventoryService()
{
    var mockInventory = new MockFactory().Create<IInventoryService>();
    var orderService = new OrderService(mockInventory);
    var product = new Product { Id = 1, Name = "Widget" };
    
    var order = orderService.CreateOrder(product, quantity: 5);
    
    mockInventory.Verify(
        x => x.ReserveStock(product.Id, 5),
        Times.Once
    );
}
```

### Exemple 6: Tests Paramétrés

```csharp
[Theory]
[InlineData(5, 3, 8)]
[InlineData(0, 0, 0)]
[InlineData(-5, 3, -2)]
public void Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
{
    var calculator = new Calculator();
    int result = calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### Exemple 7: Tests Asynchrones

```csharp
[Fact]
public async Task FetchUserAsync_WithValidId_ReturnsUser()
{
    var userRepository = new UserRepository();
    var user = await userRepository.GetUserAsync(userId: 1);
    
    Assert.NotNull(user);
    Assert.Equal("John Doe", user.Name);
}
```

### Exemple 8: Utiliser les Fixtures

```csharp
public class OrderServiceTests : PeasyPilotTestBase
{
    private OrderService _orderService = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _orderService = new OrderService();
    }
    
    [Fact]
    public void CreateOrder_WithValidProduct_Succeeds()
    {
        var order = _orderService.CreateOrder(productId: 1, quantity: 5);
        Assert.NotNull(order);
    }
}
```

---

## Meilleures Pratiques

✅ **FAIRE**
- Un test = un comportement
- Noms explicites
- Tests rapides (pas d'I/O, pas de BDD)
- Tests indépendants
- Noms de variables significatifs

❌ **NE PAS FAIRE**
- Tester plusieurs comportements dans un test
- Utiliser des nombres magiques
- Créer une configuration de test complexe
- Ignorer les cas limites
- Utiliser `Thread.Sleep`

---

## Prochaines Étapes

📖 **[Guide des Tests d'Intégration](./integration-testing-guide-FR.md)** – Tester avec les vraies BD  
📖 **[Guide BDD](./bdd-testing-guide-FR.md)** – Écrire des tests en langage métier  
📖 **[Guide de Génération de Tests](./test-generation-guide-FR.md)** – Générer les tests automatiquement  
📖 **[Guide des Adaptateurs de Framework](./framework-adapters-guide-FR.md)** – Choisir entre xUnit/NUnit/TUnit  

Les bons tests unitaires = confiance dans le code! 🚀

---

**[← Retour aux Guides d'Apprentissage](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./unit-testing-guide.md)**
