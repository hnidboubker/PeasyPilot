# Référence Rapide de Dépannage

Problèmes courants, symptômes, étapes de diagnostic et solutions pour PeasyPilot.

---

## 1. Problèmes de Compilation et de Construction

### Symptôme
```
error CS0103: Le nom 'X' n'existe pas dans le contexte actuel
error NU1101: Impossible de trouver le package 'PeasyPilot.BDD' version 0.1.0
La construction a échoué avec le code de sortie 1.
```

### Causes Probables
- Référence manquante au package PeasyPilot
- Source de package NuGet non configurée
- Cache NuGet obsolète
- Incompatibilité de version Roslyn
- Framework cible non pris en charge

### Étapes de Diagnostic
1. Vérifiez les références du projet:
   ```bash
   dotnet list package --outdated
   ```
2. Vérifiez les sources de packages:
   ```bash
   dotnet nuget list source
   # Doit inclure https://api.nuget.org/v3/index.json
   ```
3. Vérifiez le framework cible:
   ```bash
   # Dans .csproj, doit être: <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```
4. Affichez les détails de construction:
   ```bash
   dotnet build --verbose
   ```

### Solution
**Pour les références manquantes:**
```bash
# Ajoutez une référence au package du framework
dotnet add reference ../src/PeasyPilot.Core/PeasyPilot.Core.csproj

# Ou ajoutez un package NuGet
dotnet add package PeasyPilot.BDD
```

**Pour les problèmes de source NuGet:**
```bash
# Ajoutez nuget.org s'il est manquant
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# Videz le cache NuGet
dotnet nuget locals all --clear
```

**Pour les problèmes Roslyn:**
```bash
# Mettez à jour Roslyn si nécessaire
dotnet add package Microsoft.CodeAnalysis.CSharp --version 4.9.2

# Vérifiez la version
grep "Microsoft.CodeAnalysis" *.csproj
```

### Conseils de Prévention
- Exécutez `dotnet build` avant de lancer les tests
- Utilisez `dotnet restore` si vous ajoutez de nouveaux packages
- Gardez les packages Roslyn à jour
- Épinglez les versions de packages dans la gestion des packages centralisée

---

## 2. Problèmes de Découverte et d'Exécution des Tests

### Symptôme
```
Aucun test trouvé correspondant au filtre
La découverte des tests a retourné 0 résultats
Les méthodes de test suivantes n'ont pas été exécutées:
  - MyTest.TestMethod1 (La méthode n'existe pas ou pas d'attribut [Fact])
```

### Causes Probables
- La classe de test n'hérite pas de la classe de base correcte
- Attributs manquants pour les méthodes de test ([Fact], [Test], etc.)
- Assembly de test non construit ou non trouvé
- L'expression de filtre est trop restrictive
- Classe xUnit/NUnit/TUnit incorrecte

### Étapes de Diagnostic
1. Vérifiez la structure de la classe de test:
   ```csharp
   // Correct
   public class UserTests : XUnitTestBase
   {
       [Fact]
       public void TestSomething() { }
   }
   ```
2. Vérifiez que l'assembly est construit:
   ```bash
   ls tests/MyProject.Tests/bin/Debug/net8.0/
   # Doit contenir: MyProject.Tests.dll
   ```
3. Vérifiez la configuration de découverte:
   ```bash
   peasy-pilot run --filter "*" --verbose
   ```
4. Vérifiez la visibilité de la méthode de test:
   ```csharp
   // La méthode doit être publique
   public void TestMethod() { }
   ```

### Solution
**Hérité de la classe de base correcte:**
```csharp
// xUnit
public class MyTests : XUnitTestBase { }

// NUnit
public class MyTests : NUnitTestBase { }

// TUnit
public class MyTests : TUnitTestBase { }
```

**Ajoutez les attributs obligatoires:**
```csharp
[Fact] // xUnit
[Test] // NUnit
[Test] // TUnit
public void TestMethod() { }
```

**Reconstruisez et réessayez:**
```bash
dotnet clean tests/
dotnet build tests/
peasy-pilot run --project tests/MyProject.Tests.csproj
```

### Conseils de Prévention
- Utilisez des extraits de code pour les nouvelles classes de test
- Exécutez la découverte immédiatement après la création de tests
- Utilisez `--filter "*"` pour voir tous les tests découverts
- Conservez la dénomination cohérente des classes de test (par exemple, `*Tests`)

---

## 3. Problèmes de Test d'Intégration

### Symptôme
```
INTG-0001: Initialisation de la base de données échouée
Exception: La contrainte FOREIGN KEY a échoué
La connexion à la base de données a expiré après 5000 ms
Implémentation ITestDatabaseFactory non trouvée
```

### Causes Probables
- DbContext non configuré pour la base de données en mémoire
- L'ensemencement de la base de données échoue en raison de données manquantes
- Les relations de clé étrangère ne sont pas traitées lors de la réinitialisation
- La fabrique de base de données n'est pas enregistrée dans DI
- La transaction n'est pas correctement annulée

### Étapes de Diagnostic
1. Vérifiez la configuration de DbContext:
   ```csharp
   services.AddDbContext<TestDbContext>(opt =>
       opt.UseInMemoryDatabase("test-db")
   );
   ```
2. Testez l'ensemencement de la base de données indépendamment:
   ```csharp
   public async Task TestSeeding()
   {
       var context = new TestDbContext(options);
       await context.Database.EnsureCreatedAsync();
       // Ajoutez des assertions pour vérifier les données de base
   }
   ```
3. Vérifiez les contraintes de clé étrangère:
   ```csharp
   // Assurez-vous que les entités sont supprimées dans le bon ordre
   // Entités enfants d'abord, puis entités parent
   await context.Orders.ExecuteDeleteAsync(); // Enfant
   await context.Users.ExecuteDeleteAsync();  // Parent
   ```
4. Vérifiez la configuration de DI:
   ```bash
   peasy-pilot run --debug-services
   # Doit afficher: ITestDatabaseFactory → InMemoryTestDatabaseFactory
   ```

### Solution
**Configurez DbContext:**
```csharp
public class IntegrationTest
{
    protected void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TestDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString())
        );
        
        services.AddSingleton<ITestDatabaseFactory>(
            new InMemoryTestDatabaseFactory()
        );
    }
}
```

**Implémentez ResetAsync:**
```csharp
public async Task ResetAsync()
{
    // Supprimez dans l'ordre de dépendance
    await _context.Orders.ExecuteDeleteAsync();
    await _context.Users.ExecuteDeleteAsync();
    
    // Recréez le schéma et ensemencez
    await _context.Database.EnsureDeletedAsync();
    await _context.Database.EnsureCreatedAsync();
}
```

### Conseils de Prévention
- Utilisez un nom de base de données en mémoire distinct par test
- Implémentez un ensemencement approprié dans OnModelCreating
- Testez la logique de réinitialisation avant de lancer la suite complète
- Activez la journalisation détaillée pour les opérations de base de données
- Utilisez IAsyncLifetime pour l'initialisation async appropriée

---

## 4. Scénarios BDD et Fichiers de Fonctionnalité

### Symptôme
```
BDD-0001: Fichier de feature non trouvé (features/users.feature)
BDD-0002: Erreur d'analyse Gherkin à la ligne 5
BDD-0004: Définition d'étape non trouvée (Given a user with email {email})
BDD-0006: Exécution d'étape échouée (NullReferenceException dans UserSteps)
```

### Causes Probables
- Chemin du fichier de feature incorrect ou fichier non dans le répertoire de sortie
- Erreur de syntaxe Gherkin (mot-clé manquant, indentation incorrecte)
- Définition d'étape manquante ou modèle ne correspond pas
- Exception de définition d'étape lors de l'exécution
- Problème d'encodage de caractères (UTF-16 au lieu de UTF-8)

### Étapes de Diagnostic
1. Vérifiez que le fichier de feature existe et est copié:
   ```bash
   ls features/*.feature
   cat features/users.feature | head -20
   ```
2. Vérifiez l'encodage du fichier de feature:
   ```bash
   file features/users.feature
   # Doit dire: UTF-8
   ```
3. Validez la syntaxe Gherkin:
   ```gherkin
   # Format correct
   Feature: Gestion des Utilisateurs
     Scenario: Créer un utilisateur
       Given une base de données propre
       When je crée un utilisateur "john@example.com"
       Then l'utilisateur existe
   ```
4. Vérifiez la définition d'étape:
   ```csharp
   public class UserSteps : BddStepDefinition
   {
       [Given("une base de données propre")]
       public void CleanDatabase() { }
       
       [When("je crée un utilisateur {email}")]
       public void CreateUser(string email) { }
   }
   ```

### Solution
**Copiez les fichiers de feature dans la sortie:**
```xml
<!-- Dans .csproj -->
<ItemGroup>
    <None Update="features/**/*.feature">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

**Corrigez la syntaxe Gherkin:**
- Utilisez une indentation cohérente de 2 espaces
- Vérifiez les mots-clés: Feature, Scenario, Given, When, Then, And, But
- Vérifiez les espaces réservés de paramètres: `{name}` (pas `{name:}` ou `$name`)

**Enregistrez les définitions d'étape:**
```csharp
services.AddScoped<IStepBindingResolver>(provider =>
    new StepBindingResolver(typeof(UserSteps).Assembly)
);
```

### Conseils de Prévention
- Validez la syntaxe Gherkin avant de valider
- Testez les fichiers de feature avec des étapes fictives d'abord
- Utilisez une dénomination cohérente: fichiers `*.feature`, classes `*Steps.cs`
- Activez la journalisation détaillée de la liaison d'étape: `--bdd-trace`
- Testez l'extraction de paramètres indépendamment

---

## 5. Opérations et Commandes CLI

### Symptôme
```
CLI-0001: Commande inconnue "rnu"
CLI-0002: Paramètre obligatoire manquant --project
CLI-0003: Valeur de paramètre invalide (fichier non trouvé)
Connexion refusée lors de la connexion au serveur CLI
```

### Causes Probables
- Nom de commande mal orthographié
- Paramètre obligatoire non fourni
- Le chemin du fichier n'existe pas ou est relatif
- Serveur CLI non démarré
- Incompatibilité de version CLI

### Étapes de Diagnostic
1. Vérifiez les commandes disponibles:
   ```bash
   peasy-pilot --help
   peasy-pilot <command> --help
   ```
2. Vérifiez que les chemins de fichier sont absolus ou correctement relatifs:
   ```bash
   # Utilisez ./ pour le répertoire actuel
   peasy-pilot run --project ./tests/MyTest.csproj
   ```
3. Vérifiez la version CLI:
   ```bash
   peasy-pilot --version
   ```
4. Testez la syntaxe de la commande:
   ```bash
   peasy-pilot run --help
   # Lisez les paramètres obligatoires vs optionnels
   ```

### Solution
**Utilisez la syntaxe de commande correcte:**
```bash
# Correct
peasy-pilot run --project ./MyProject.Tests.csproj --filter "namespace=MyApp.Tests"

# Incorrect (paramètre à la mauvaise position)
peasy-pilot --project ./MyProject.Tests.csproj run
```

**Fournissez les paramètres obligatoires:**
```bash
peasy-pilot run \
  --project ./tests/MyProject.Tests.csproj \
  --framework xunit \
  --environment Development
```

**Utilisez les chemins absolus si nécessaire:**
```bash
peasy-pilot run --project "G:\MonProjet\tests\MyProject.Tests.csproj"
```

### Conseils de Prévention
- Utilisez `--help` avant de lancer les nouvelles commandes
- Validez que les chemins existent avant de lancer les commandes
- Mettez à jour frequemment le CLI: `dotnet tool update --global peasy-pilot`
- Utilisez un fichier de configuration pour les options complexes:
  ```bash
  peasy-pilot run --config ./peasy-pilot.json
  ```

---

## 6. Problèmes de Performance

### Symptôme
```
Les tests s'exécutent très lentement (> 30 secondes)
L'initialisation de la base de données prend des minutes
La découverte de tests prend > 1 minute
L'utilisation de la mémoire dépasse 1 GB
```

### Causes Probables
- Trop de tests dans une seule exécution de découverte
- La base de données n'utilise pas le fournisseur en mémoire
- Grands ensembles de données dans l'ensemencement
- Utilisation inutile d'async/await
- Absence d'indexation sur les colonnes fréquemment interrogées

### Étapes de Diagnostic
1. Profilez l'exécution des tests:
   ```bash
   peasy-pilot run --project MyTests.csproj --profile
   # Affiche: temps de découverte, temps d'exécution, temps de démontage
   ```
2. Vérifiez le fournisseur de base de données:
   ```csharp
   // Doit utiliser InMemoryDatabase pour les tests
   opt.UseInMemoryDatabase("test-db")
   
   // Pas SQL Server
   opt.UseSqlServer("connection-string") // LENT
   ```
3. Surveillez l'utilisation de la mémoire:
   ```bash
   # Windows
   Get-Process dotnet | Select WorkingSet
   
   # macOS/Linux
   ps aux | grep dotnet
   ```
4. Analysez le temps d'ensemencement:
   ```csharp
   var sw = Stopwatch.StartNew();
   await context.Database.EnsureCreatedAsync();
   sw.Stop();
   Console.WriteLine($"L'ensemencement a pris {sw.ElapsedMilliseconds}ms");
   ```

### Solution
**Regroupez les tests en groupes plus petits:**
```bash
# Exécutez un sous-ensemble de tests
peasy-pilot run --filter "namespace=MyApp.Tests.Unit"
```

**Utilisez l'exécution parallèle si possible:**
```csharp
[Collection("Non-Parallel")] // Pour les tests qui doivent s'exécuter séquentiellement
public class IntegrationTest { }

// Les autres tests s'exécutent en parallèle automatiquement
```

**Optimisez l'ensemencement:**
```csharp
// Utilisez l'insertion en masse au lieu d'ajouts individuels
context.Users.AddRange(users);
await context.SaveChangesAsync();
```

**Mettez en cache les opérations coûteuses:**
```csharp
private static IEnumerable<TestData> _cachedData;

public IEnumerable<TestData> GetTestData()
{
    return _cachedData ??= LoadTestData();
}
```

### Conseils de Prévention
- Surveillez régulièrement le temps d'exécution des tests
- Utilisez exclusivement une base de données en mémoire pour les tests
- Conservez les données de base minimales (seulement ce que les tests nécessitent)
- Profilez avant d'optimiser (utilisez l'indicateur `--profile`)
- Exécutez les tests en parallèle (les frameworks le supportent)

---

## 7. Problèmes du Pipeline CI/CD

### Symptôme
```
Échec de la construction CI: "La construction a réussi mais les tests ont échoué dans le pipeline"
Délai d'expiration de restauration NuGet dans GitHub Actions
Échec de la construction de l'image Docker: "Package non trouvé"
Les tests passent localement mais échouent dans CI
```

### Causes Probables
- Source NuGet non configurée dans l'environnement CI
- Version .NET SDK différente dans CI par rapport au local
- Les tests dépendent des ressources locales (base de données, fichiers)
- Les variables d'environnement ne sont pas définies dans CI
- Conditions de concurrence dans l'exécution des tests en parallèle

### Étapes de Diagnostic
1. Vérifiez la configuration CI (.github/workflows):
   ```yaml
   - name: Setup .NET
     uses: actions/setup-dotnet@v4
     with:
       dotnet-version: '8.0.x'
   ```
2. Vérifiez les sources NuGet dans CI:
   ```bash
   # Dans le pipeline CI, ajoutez:
   dotnet nuget add source https://api.nuget.org/v3/index.json
   ```
3. Vérifiez les dépendances de ressources locales:
   ```csharp
   // Mauvais: chemins codés en dur
   var dbPath = "C:\\MyData\\test.db";
   
   // Bon: basé sur l'environnement ou en mémoire
   var dbPath = Environment.GetEnvironmentVariable("TEST_DB_PATH")
       ?? ":memory:";
   ```
4. Exécutez les tests avec la configuration CI localement:
   ```bash
   export GITHUB_WORKSPACE=$(pwd)
   dotnet test # Identique à ce que CI exécuterait
   ```

### Solution
**Configurez NuGet dans CI:**
```yaml
# .github/workflows/test.yml
- name: Restore packages
  run: |
    dotnet nuget add source https://api.nuget.org/v3/index.json \
      -n nuget.org \
      -c
    dotnet restore
```

**Utilisez une configuration consciente de l'environnement:**
```csharp
public class TestConfiguration
{
    public static string GetDatabasePath()
    {
        var isCI = !string.IsNullOrEmpty(
            Environment.GetEnvironmentVariable("CI"));
        
        if (isCI)
            return ":memory:";
        
        return Path.Combine(
            Path.GetTempPath(),
            "peasy-test.db"
        );
    }
}
```

**Activez la journalisation détaillée de CI:**
```bash
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

### Conseils de Prévention
- Testez la configuration CI localement avant d'envoyer
- Utilisez la même version .NET SDK dans CI et localement
- Évitez les chemins codés en dur ou les hypothèses
- Définissez toutes les variables d'environnement requises dans CI
- Consignez les informations détaillées pour les échecs CI
- Utilisez `--help` pour comprendre toutes les options de test

---

## Liste de Contrôle de Diagnostic Rapide

**Avant de contacter le support, vérifiez:**
- [ ] Le code se construit avec succès: `dotnet build`
- [ ] Toutes les dépendances sont installées: `dotnet restore`
- [ ] Les tests sont découverts: `peasy-pilot run --filter "*" --verbose`
- [ ] Le code d'erreur est documenté: [Référence des Codes d'Erreur](error-codes-FR.md)
- [ ] Les journaux affichent la cause racine: Activez `--verbose` ou `--debug`
- [ ] Le problème est reproductible localement
- [ ] Vous avez cherché dans la documentation un problème similaire

---

## Liens Rapides vers les Codes d'Erreur Courants

| Symptôme | Code d'Erreur | Correction |
|---------|---------------|-----------|
| Tests non découverts | CORE-0001 | Ajoutez [Fact], héritez de la classe de base |
| Échec de l'initialisation de la base de données | INTG-0001 | Configurez correctement DbContext |
| Fichier de feature non trouvé | BDD-0001 | Copiez dans la sortie, vérifiez le chemin |
| Définition d'étape manquante | BDD-0004 | Enregistrez dans DI, vérifiez le modèle |
| Commande CLI inconnue | CLI-0001 | Utilisez `--help`, vérifiez l'orthographe |
| Tests lents | (Performance) | Utilisez la base de données en mémoire, en parallèle |

---

**Besoin d'aide supplémentaire?**
- [Référence Complète des Codes d'Erreur](error-codes-FR.md)
- [Référence CLI](cli-reference-FR.md)
- [Référence de Configuration](configuration-reference-FR.md)

---

**Dernière mise à jour:** 2026-09-11  
[← Retour à REFERENCE](README.md)
