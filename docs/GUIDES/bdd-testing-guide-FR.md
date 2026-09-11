# Guide des Tests BDD

## Aperçu

Le Behavior-Driven Development (BDD) écrit les tests comme des scénarios métier. Au lieu de "tester la création d'utilisateur", vous écrivez "Étant donné que je suis un nouvel utilisateur, Quand je m'inscris, Alors je dois recevoir un email de confirmation."

**Prérequis:** [Premiers pas](../GETTING-STARTED-FR.md)  
**Durée:** 25 minutes  

---

## Pourquoi BDD Importe

BDD comble le fossé entre développeurs et stakeholders métier. Les tests se lisent comme des exigences, les rendant auto-documentés.

---

## Syntaxe Gherkin

Les fichiers feature utilisent le langage Gherkin :

```gherkin
Feature: Inscription Utilisateur
  Scenario: Inscrire avec email valide
    Given Je suis un nouvel utilisateur
    When Je soumets le formulaire d'inscription avec email "john@example.com"
    Then Je dois recevoir un email de confirmation
    And Mon compte doit être actif
```

---

## Définitions d'Étapes

Liez les étapes Gherkin au code avec les attributs :

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class UserRegistrationSteps : BddStepDefinition
{
    private string _email = null!;
    private bool _emailSent;
    private User _registeredUser = null!;
    
    [Given("Je suis un nouvel utilisateur")]
    public async Task NewUser() => await Task.CompletedTask;
    
    [When("Je soumets le formulaire d'inscription avec email {email}")]
    public async Task SubmitRegistrationWithEmail(string email)
    {
        _email = email;
        var service = new UserService();
        _registeredUser = await service.RegisterAsync(email);
        _emailSent = true;
    }
    
    [Then("Je dois recevoir un email de confirmation")]
    public async Task ConfirmationEmailSent()
    {
        Assert.True(_emailSent);
        await Task.CompletedTask;
    }
    
    [And("Mon compte doit être actif")]
    public async Task AccountIsActive()
    {
        Assert.True(_registeredUser.IsActive);
        await Task.CompletedTask;
    }
}
```

---

## Extraction de Paramètres

Extrayez les paramètres du texte d'étape :

```csharp
[Given("J'ai {count} articles dans le panier")]
public async Task ItemsInCart(int count)
{
    _cart.AddItems(count);
    await Task.CompletedTask;
}

[When("J'applique le coupon {code}")]
public async Task ApplyCoupon(string code)
{
    _discount = await _couponService.GetDiscountAsync(code);
    await Task.CompletedTask;
}

[Then("Le total doit être ${amount}")]
public async Task TotalIs(decimal amount)
{
    Assert.Equal(amount, _cart.Total);
    await Task.CompletedTask;
}
```

---

## Exemples

### Exemple 1: Connexion Utilisateur

```gherkin
Feature: Connexion Utilisateur
  Scenario: Connexion avec identifiants valides
    Given Un utilisateur avec email "john@example.com" et mdp "password123"
    When Je me connecte
    Then Je dois voir le tableau de bord
```

**Définitions :**
```csharp
public class LoginSteps : BddStepDefinition
{
    private User _user = null!;
    private bool _loginSuccess;
    
    [Given("Un utilisateur avec email {email} et mdp {password}")]
    public async Task UserExists(string email, string password)
    {
        _user = new User { Email = email, Password = password };
        await Task.CompletedTask;
    }
    
    [When("Je me connecte")]
    public async Task LogIn()
    {
        var service = new AuthService();
        _loginSuccess = await service.LoginAsync(_user.Email, _user.Password);
    }
    
    [Then("Je dois voir le tableau de bord")]
    public async Task SeeDashboard()
    {
        Assert.True(_loginSuccess);
        await Task.CompletedTask;
    }
}
```

### Exemple 2: Traitement de Commandes

```gherkin
Feature: Traitement de Commandes
  Scenario: Calculer le total avec taxes
    Given Un panier avec articles
    And Le client est en Californie
    When Je calcule le total
    Then La taxe doit être 8.25%
    And Je dois voir le résumé de commande
```

### Exemple 3: Scénarios Multiples

```gherkin
Feature: Validation de Mot de Passe
  Scenario: Accepter un mot de passe fort
    When Je saisis le mdp "SecurePass123!"
    Then Le mdp doit être valide
  
  Scenario: Rejeter un mdp faible
    When Je saisis le mdp "123"
    Then Le mdp doit être invalide
    And Je dois voir "Mdp trop court"
```

### Exemple 4: Exécuter les Tests BDD

```csharp
public class UserRegistrationBddTests
{
    [Fact]
    public async Task UserRegistration_ExecuteFeature()
    {
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/user-registration.feature");
        
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(UserRegistrationSteps));
        
        var executor = new ScenarioExecutor(resolver);
        var scenario = feature.Scenarios.First();
        var result = await executor.ExecuteAsync(scenario, null);
        
        Assert.True(result.Status == ScenarioStatus.Passed);
    }
}
```

### Exemple 5: Intégration avec BD

```csharp
public class OrderBddTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task OrderProcessing_CompleteFlow()
    {
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/order-processing.feature");
        
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(OrderSteps));
        
        var executor = new ScenarioExecutor(resolver);
        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, null);
            Assert.True(result.Status == ScenarioStatus.Passed);
        }
    }
}
```

---

## Meilleures Pratiques

✅ **FAIRE**
- Écrire les scénarios en langage métier
- Un scénario = un parcours utilisateur
- Utiliser des exemples concrets
- Garder les étapes indépendantes
- Partager le contexte entre étapes

❌ **NE PAS FAIRE**
- Écrire des scénarios techniques
- Hardcoder les données de test
- Créer des dépendances entre scénarios
- Utiliser des noms d'étape vagues
- Mélanger plusieurs workflows dans un scénario

---

## Prochaines Étapes

📖 **[Guide des Tests d'Intégration](./integration-testing-guide-FR.md)** – Ajouter les tests BD  
📖 **[Guide de Génération de Tests](./test-generation-guide-FR.md)** – Générer automatiquement  

BDD rend les tests lisibles et maintenables! 🎭
