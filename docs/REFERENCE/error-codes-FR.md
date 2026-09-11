# Codes d'Erreur — Référence

Référence complète des codes d'erreur du framework PeasyPilot et ses packages.

## Format des Codes d'Erreur

Les codes d'erreur suivent le format : **PPPP-NNNN**

- **PPPP** — Code du package (4 lettres)
- **NNNN** — Numéro d'erreur (0000-9999)

### Codes des Packages

| Code | Package | Description |
|------|---------|-------------|
| CORE | PeasyPilot.Core | Abstractions principales, découverte, orchestration |
| UNIT | PeasyPilot.Unit | Utilitaires de test unitaire |
| INTG | PeasyPilot.Integration | Fixtures de test d'intégration |
| BDD  | PeasyPilot.BDD | Développement Piloté par le Comportement |
| MOQ  | PeasyPilot.Moq | Abstractions de fabrique de mocks |
| BOGUS | PeasyPilot.Bogus | Génération de données fictives |
| COV  | PeasyPilot.Coverage | Rapports de couverture |
| CLI  | PeasyPilot.CLI | Interface en ligne de commande |
| TST  | PeasyPilot.TestAssistant | Génération et analyse de tests |
| MCP  | PeasyPilot.Mcp | Serveur MCP et transport |
| XUNIT | PeasyPilot.XUnit | Adaptateur de framework xUnit |
| NUNIT | PeasyPilot.NUnit | Adaptateur de framework NUnit |
| TUNIT | PeasyPilot.TUnit | Adaptateur de framework TUnit |

---

## Package Core (CORE)

### CORE-0001 — Test Non Découvert

**Message:** "Aucun test trouvé correspondant aux critères de filtre ou de découverte."

**Cause:**
- La classe de test n'hérite pas de la classe de base correcte
- La méthode de test ne dispose pas de l'attribut [Fact] ou [Fact(Skip=...)] (xUnit)
- La méthode de test ne dispose pas de l'attribut [Test] (NUnit)
- Assembly non référencé ou non compilé

**Solution:**
1. Vérifiez que la classe de test hérite de la classe de base appropriée du framework
2. Assurez-vous que les méthodes de test ont l'attribut correct ([Fact], [Test], etc.)
3. Vérifiez que l'assembly de test est construit : `dotnet build tests/`
4. Vérifiez que l'assembly est référencé dans la configuration de découverte
5. Exécutez la découverte avec journalisation détaillée : `--verbose` ou `--debug`

**Exemple de sortie d'erreur:**
```
CORE-0001: Aucun test trouvé.
Filtre de découverte: namespace=MyApp.Tests
Vérifié: 1 assembly (5 classes, 0 méthodes de test valides)
```

**Codes associés:** XUNIT-0010, NUNIT-0010, TUNIT-0010

---

### CORE-0002 — Contexte d'Orchestration Non Initialisé

**Message:** "TestContext est null ou n'a pas été correctement initialisé."

**Cause:**
- TestOrchestrator non invoqué avant l'accès au contexte
- L'initialisation de la fixture a échoué silencieusement
- Le conteneur d'injection de dépendances n'est pas configuré

**Solution:**
1. Assurez-vous que TestOrchestrator.Initialize() est appelé avant l'exécution du test
2. Vérifiez les méthodes de configuration de fixture (Setup, [OneTimeSetUp], etc.)
3. Vérifiez la configuration du conteneur DI dans IntegrationTestFixture
4. Ajoutez une journalisation d'initialisation pour diagnostiquer les défaillances silencieuses

**Exemple de sortie d'erreur:**
```
CORE-0002: Initialisation du TestContext échouée.
Stack: at PeasyPilot.Core.TestCase.Execute()
Attendu: context.ServiceProvider != null
Réel: null
```

**Codes associés:** INTG-0001, INTG-0002

---

### CORE-0003 — Rapporteur de Résultats de Test Non Trouvé

**Message:** "Aucune implémentation ITestReporter trouvée dans la collection de services."

**Cause:**
- Le rapporteur n'est pas enregistré dans le conteneur DI
- La mauvaise implémentation de rapporteur est injectée
- L'enregistrement du service est écrasé

**Solution:**
1. Enregistrez le rapporteur dans ConfigureServices:
   ```csharp
   services.AddSingleton<ITestReporter>(
       new ConsoleTestReporter()
   );
   ```
2. Vérifiez l'ordre d'enregistrement dans Startup/Program.cs
3. Vérifiez qu'aucun enregistrement en double ne remplace votre configuration

**Exemple de sortie d'erreur:**
```
CORE-0003: Résolution du service échouée.
Type: PeasyPilot.Core.Reporting.ITestReporter
Statut: Non enregistré
Rapporteurs disponibles: (aucun)
```

**Codes associés:** CORE-0005, INTG-0003

---

### CORE-0004 — Analyse d'Impact de Test Invalide

**Message:** "L'analyse d'impact a échoué ou a retourné des résultats inattendus."

**Cause:**
- Chemin du code source non trouvé ou inaccessible
- La compilation Roslyn a échoué
- Graphe de dépendance incomplet

**Solution:**
1. Vérifiez que les chemins source existent et sont lisibles
2. Exécutez `dotnet build` pour vérifier que la compilation réussit
3. Vérifiez les erreurs Roslyn dans la sortie de compilation
4. Activez le mode debug d'analyse d'impact: `--impact-debug`

**Exemple de sortie d'erreur:**
```
CORE-0004: Analyse d'impact échouée.
Raison: Chemin source non trouvé
Chemin: G:\src\MyApp\Services
Statut: Le répertoire n'existe pas
```

**Codes associés:** TST-0005, TST-0006

---

### CORE-0005 — Dépendance de Service Non Enregistrée

**Message:** "Le service de type 'X' n'est pas enregistré dans le conteneur d'injection de dépendances."

**Cause:**
- Service requis manquant de ConfigureServices
- Incompatibilité d'interface/implémentation
- Problème de portée de durée de vie (Transient vs. Singleton)

**Solution:**
1. Ajoutez l'enregistrement du service:
   ```csharp
   services.AddScoped<IMyService, MyService>();
   ```
2. Assurez-vous que les interfaces correspondent à ce qui est résolu
3. Vérifiez les exigences de durée de vie du service
4. Utilisez `services.DescribeRegistrations()` pour déboguer

**Exemple de sortie d'erreur:**
```
CORE-0005: Service non enregistré.
Demandé: PeasyPilot.Core.ITestDatabase (Scoped)
Indice: Avez-vous appelé ConfigureIntegrationTesting() ?
```

**Codes associés:** INTG-0003, INTG-0004

---

## Test d'Intégration (INTG)

### INTG-0001 — Initialisation de la Base de Données En Mémoire Échouée

**Message:** "La base de données en mémoire n'a pas pu être créée ou configurée."

**Cause:**
- Entity Framework DbContext non configuré
- La méthode d'ensemencement de la base de données a levé une exception
- Contraintes de mémoire ou délai d'expiration de connexion

**Solution:**
1. Assurez-vous que DbContext est enregistré:
   ```csharp
   services.AddDbContext<TestDbContext>(
       opt => opt.UseInMemoryDatabase("test-db")
   );
   ```
2. Vérifiez la logique d'ensemencement dans `OnModelCreating()` ou `SeedAsync()`
3. Vérifiez la configuration de la chaîne de connexion
4. Ajoutez try-catch autour de l'ensemencement pour capturer les erreurs

**Exemple de sortie d'erreur:**
```
INTG-0001: Initialisation de la base de données échouée.
DbContext: MyAppContext
Erreur: La propriété 'Id' sur le type 'User' n'est pas mappée
Fournisseur: en mémoire
```

**Codes associés:** INTG-0002, INTG-0004

---

### INTG-0002 — Réinitialisation de la Base de Données Échouée Pendant le Démontage

**Message:** "Impossible de réinitialiser l'état de la base de données entre les exécutions de test."

**Cause:**
- La transaction de base de données n'est pas validée ou annulée
- Les contraintes de clé étrangère empêchent la suppression
- Exception de la méthode ResetAsync()

**Solution:**
1. Implémentez ResetAsync() sur votre fixture de base de données:
   ```csharp
   public async Task ResetAsync()
   {
       await _context.Database.EnsureDeletedAsync();
       await _context.Database.EnsureCreatedAsync();
   }
   ```
2. Vérifiez les relations de clé étrangère et l'ordre de suppression
3. Ajoutez un rollback de transaction explicite avant la réinitialisation
4. Testez la logique de réinitialisation indépendamment

**Exemple de sortie d'erreur:**
```
INTG-0002: Réinitialisation de la base de données échouée.
Opération: EnsureDeletedAsync
Erreur: La contrainte FOREIGN KEY a échoué
Table: Orders, Colonne: UserId
```

**Codes associés:** INTG-0001, INTG-0003

---

### INTG-0003 — Fabrique de Base de Données de Test Non Configurée

**Message:** "Implémentation ITestDatabaseFactory non trouvée."

**Cause:**
- La fabrique n'est pas enregistrée dans le conteneur DI
- Le type de fabrique erroné est enregistré (SQL au lieu d'InMemory)
- Factory.Create() a retourné null

**Solution:**
1. Enregistrez la fabrique dans ConfigureIntegrationTesting:
   ```csharp
   services.AddSingleton<ITestDatabaseFactory>(
       new InMemoryTestDatabaseFactory()
   );
   ```
2. Vérifiez l'implémentation de factory.Create()
3. Vérifiez que les options de base de données sont passées correctement
4. Utilisez une spécification de type explicite s'il existe plusieurs implémentations

**Exemple de sortie d'erreur:**
```
INTG-0003: Fabrique de base de données non enregistrée.
Fabriques disponibles: (aucune)
Attendu: ITestDatabaseFactory
Indice: Appelez ConfigureIntegrationTesting(services)
```

**Codes associés:** INTG-0001, CORE-0005

---

### INTG-0004 — Configuration de WebApplicationTestFactory Invalide

**Message:** "WebApplicationFactory<T> n'a pas pu configurer le serveur de test."

**Cause:**
- La classe Startup ou Program.cs est incompatible
- La configuration du générateur d'hôte est manquante
- Le type générique T ne dispose pas de support IHost

**Solution:**
1. Assurez-vous que Program.cs crée une WebApplication:
   ```csharp
   var app = builder.Build();
   // ... configurer les middlewares
   await app.RunAsync();
   ```
2. Vérifiez que WebApplicationTestFactory<Program> (ou classe Startup) est utilisé
3. Remplacez ConfigureWebHost si une configuration personnalisée est nécessaire:
   ```csharp
   protected override void ConfigureWebHost(IWebHostBuilder builder)
   {
       builder.ConfigureServices(services => { /* ... */ });
   }
   ```
4. Vérifiez les exceptions dans le pipeline middleware

**Exemple de sortie d'erreur:**
```
INTG-0004: Initialisation de WebApplicationFactory échouée.
Type générique: MyApp.Program
Erreur: Aucun type Program public trouvé
Indice: Assurez-vous que Program.cs est dans l'espace de noms racine
```

**Codes associés:** INTG-0005, INTG-0006

---

### INTG-0005 — Requête HTTP de Test Échouée

**Message:** "La requête HTTP au serveur de test a échoué ou a dépassé le délai d'attente."

**Cause:**
- Le serveur de test n'est pas démarré
- Chemin de point de terminaison invalide
- Délai d'expiration de la requête ou réinitialisation de connexion
- Jeton d'authentification manquant

**Solution:**
1. Vérifiez que le serveur est créé: `using var factory = new WebApplicationTestFactory<Program>();`
2. Vérifiez que le chemin de point de terminaison correspond au routage du contrôleur
3. Augmentez le délai d'expiration si le test est lent: `httpClient.Timeout = TimeSpan.FromSeconds(10);`
4. Ajoutez l'authentification si le point de terminaison l'exige:
   ```csharp
   httpClient.DefaultRequestHeaders.Authorization = 
       new AuthenticationHeaderValue("Bearer", token);
   ```
5. Activez la journalisation détaillée: `--verbose --http-trace`

**Exemple de sortie d'erreur:**
```
INTG-0005: Requête HTTP échouée.
Méthode: GET
Point de terminaison: /api/users/1
Statut: Délai d'expiration de connexion après 5000 ms
```

**Codes associés:** INTG-0004, INTG-0006

---

## Package BDD (BDD)

### BDD-0001 — Fichier Feature Non Trouvé

**Message:** "Le fichier de feature n'a pas pu être localisé ou chargé."

**Cause:**
- Chemin de fichier incorrect ou chemin relatif mal calculé
- Fichier de feature non copié dans le répertoire de sortie
- Les permissions d'annuaire empêchent la lecture

**Solution:**
1. Vérifiez que le fichier de feature existe:
   ```bash
   ls docs/features/*.feature
   ```
2. Assurez-vous que le fichier est copié dans la sortie de compilation:
   ```xml
   <ItemGroup>
       <None Update="features/**/*.feature">
           <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
       </None>
   </ItemGroup>
   ```
3. Utilisez des chemins absolus ou vérifiez le répertoire de travail
4. Vérifiez que l'encodage du fichier est UTF-8 (pas UTF-16 ou BOM)

**Exemple de sortie d'erreur:**
```
BDD-0001: Fichier de feature non trouvé.
Chemin: features/users.feature
Recherché: G:\Projet\features\users.feature
Statut: Le fichier n'existe pas
```

**Codes associés:** BDD-0002, BDD-0003

---

### BDD-0002 — Erreur d'Analyse du Fichier Feature

**Message:** "La syntaxe du fichier de feature Gherkin est invalide."

**Cause:**
- Mot-clé "Feature:" manquant
- L'indentation est incorrecte (pas 2 ou 4 espaces)
- Mot-clé d'étape invalide (Given/When/Then)
- Erreur de syntaxe dans les paramètres du plan de scénario

**Solution:**
1. Vérifiez la syntaxe Gherkin:
   ```gherkin
   Feature: Inscription des Utilisateurs
     Scenario: Inscrire avec un email valide
       Given un formulaire d'inscription utilisateur
       When l'utilisateur saisit "user@example.com"
       Then le compte est créé
   ```
2. Vérifiez la cohérence de l'indentation (tous les espaces ou tous les onglets)
3. Assurez-vous que seuls les mots-clés valides sont utilisés: Given, When, Then, And, But, Scenario, Feature
4. Validez la syntaxe des paramètres: `<name>` pour les espaces réservés

**Exemple de sortie d'erreur:**
```
BDD-0002: Erreur d'analyse Gherkin.
Fichier: features/users.feature
Ligne: 5
Erreur: Mot-clé d'étape inconnu "Given:" (doit être "Given")
```

**Codes associés:** BDD-0001, BDD-0004

---

### BDD-0003 — Problème d'Encodage de Caractères du Fichier Feature

**Message:** "Le fichier de feature contient des caractères invalides ou un codage incorrect."

**Cause:**
- Le fichier est codé en UTF-16 au lieu de UTF-8
- Les caractères spéciaux ne sont pas correctement codés (accents, symboles)
- BOM (Byte Order Mark) présent dans le fichier

**Solution:**
1. Vérifiez que l'encodage du fichier est UTF-8:
   ```bash
   file features/users.feature
   # La sortie doit contenir "UTF-8"
   ```
2. Convertir si nécessaire:
   ```bash
   # Windows PowerShell
   (Get-Content features/users.feature) | 
   Set-Content -Encoding UTF8 features/users.feature
   ```
3. Supprimez le BOM s'il est présent (VS Code: changez l'encodage en "UTF-8" dans la barre d'état)
4. Utilisez des caractères Unicode appropriés pour les lettres spéciales (é, ñ, etc.)

**Exemple de sortie d'erreur:**
```
BDD-0003: Erreur d'encodage de caractères.
Fichier: features/users.feature
Détecté: UTF-16 LE
Attendu: UTF-8
Indice: Utilisez "Enregistrer avec l'encodage" (UTF-8 sans BOM)
```

**Codes associés:** BDD-0001, BDD-0002

---

### BDD-0004 — Définition d'Étape Non Trouvée

**Message:** "Aucune définition d'étape ne correspond à l'étape donnée dans le fichier de feature."

**Cause:**
- Le modèle d'étape ne correspond pas au texte d'étape dans la feature
- La classe de définition d'étape n'est pas décorée avec [Given], [When], [Then]
- Le regex du modèle est incorrect ou trop strict
- L'assembly de définition d'étape n'est pas référencé ou non analysé

**Solution:**
1. Vérifiez que la définition d'étape existe:
   ```csharp
   public class UserSteps : BddStepDefinition
   {
       [Given("a user with email {email}")]
       public void CreateUser(string email) { }
   }
   ```
2. Vérifiez que le modèle correspond exactement à l'étape du fichier de feature:
   ```gherkin
   Given a user with email "john@example.com"
   # Le modèle doit correspondre à: "a user with email {email}"
   ```
3. Vérifiez que l'assembly de définition d'étape est référencé dans le projet de test
4. Enregistrez les définitions d'étape dans le conteneur DI si vous utilisez un résolveur
5. Vérifiez que les noms de paramètres correspondent aux groupes regex: `{email}` → `email`

**Exemple de sortie d'erreur:**
```
BDD-0004: Définition d'étape non trouvée.
Étape: "Given a user exists"
Modèle: "a user with email {email}"
Statut: Aucune correspondance
Assemblies vérifiés: 2 (8 définitions d'étape)
```

**Codes associés:** BDD-0005, BDD-0006

---

### BDD-0005 — Extraction du Paramètre de Liaison d'Étape Échouée

**Message:** "Impossible d'extraire les valeurs de paramètre du texte d'étape."

**Cause:**
- Le nom du paramètre dans le modèle ne correspond pas au groupe de capture
- La conversion de type a échoué (string to int, decimal, etc.)
- Le modèle regex est mal formé ou sans groupes

**Solution:**
1. Assurez-vous que les noms de paramètres dans le modèle correspondent à la signature de la méthode:
   ```csharp
   [Given("I have {count:int} items")]
   public void SetupItems(int count) { } // count doit correspondre
   ```
2. Vérifiez que la conversion de type est prise en charge (int, decimal, bool, string)
3. Testez le modèle regex isolément:
   ```csharp
   var pattern = @"I have (?<count>\d+) items";
   var match = Regex.Match("I have 5 items", pattern);
   assert(match.Success && match.Groups["count"].Value == "5");
   ```
4. Ajoutez des convertisseurs de type personnalisés si nécessaire:
   ```csharp
   public class EmailConverter : IParameterConverter
   {
       public object Convert(string value) => new Email(value);
   }
   ```

**Exemple de sortie d'erreur:**
```
BDD-0005: Extraction du paramètre échouée.
Étape: "I have 5 items"
Modèle: "I have {count} items"
Erreur: Impossible de convertir "5" en type System.Int32
```

**Codes associés:** BDD-0004, BDD-0006

---

### BDD-0006 — Exception d'Exécution d'Étape

**Message:** "L'étape a été exécutée mais a levé une exception."

**Cause:**
- L'assertion a échoué dans la définition d'étape
- Erreur de logique métier (base de données, API, etc.)
- Référence nulle ou validation d'argument

**Solution:**
1. Vérifiez l'implémentation de la définition d'étape pour les erreurs:
   ```csharp
   [When("the user logs in")]
   public void UserLogsIn()
   {
       var user = _repository.GetUser("john@example.com");
       Assert.NotNull(user); // Capture si l'utilisateur n'existe pas
       _service.Login(user);
   }
   ```
2. Ajoutez try-catch et des messages d'erreur détaillés
3. Vérifiez que les données de test sont correctement configurées dans les étapes Given
4. Vérifiez les défaillances de service externe (base de données, API)
5. Activez le traçage d'exécution d'étape: `--bdd-trace`

**Exemple de sortie d'erreur:**
```
BDD-0006: Exécution d'étape échouée.
Étape: "When the user logs in"
Exception: NullReferenceException
Message: L'utilisateur "john@example.com" non trouvé
Stack: at UserSteps.UserLogsIn() line 45
```

**Codes associés:** BDD-0004, BDD-0005, INTG-0001

---

## Package CLI (CLI)

### CLI-0001 — Commande Non Reconnue

**Message:** "Commande inconnue ou syntaxe invalide."

**Cause:**
- Le nom de la commande est mal orthographié
- La commande nécessite une version spécifique
- La commande n'est pas disponible dans le contexte actuel

**Solution:**
1. Répertoriez les commandes disponibles:
   ```bash
   peasy-pilot --help
   peasy-pilot <command> --help
   ```
2. Vérifiez la syntaxe de la commande:
   ```bash
   # Correct
   peasy-pilot run --project MyProject.csproj
   
   # Incorrect
   peasy-pilot --project MyProject.csproj run
   ```
3. Mettez à jour PeasyPilot vers la dernière version: `dotnet tool update --global peasy-pilot`
4. Vérifiez l'aide pour les paramètres obligatoires vs. optionnels

**Exemple de sortie d'erreur:**
```
CLI-0001: Commande inconnue.
Entrée: "peasy-pilot rnu"
Avez-vous voulu dire: "peasy-pilot run"
Utilisez: "peasy-pilot --help" pour les commandes disponibles
```

**Codes associés:** CLI-0002, CLI-0003

---

### CLI-0002 — Paramètre Obligatoire Manquant

**Message:** "Le paramètre obligatoire n'a pas été fourni."

**Cause:**
- Le paramètre est obligatoire mais non spécifié
- Le paramètre utilise un mauvais format (par exemple, valeur attendue mais seulement un drapeau donné)
- Le fichier de configuration est manquant

**Solution:**
1. Vérifiez l'aide de la commande pour les paramètres obligatoires:
   ```bash
   peasy-pilot run --help
   ```
2. Fournissez les paramètres obligatoires:
   ```bash
   # L'exemple nécessite --project
   peasy-pilot run --project MyProject.csproj
   ```
3. Utilisez un fichier de configuration si préféré:
   ```bash
   peasy-pilot run --config peasy-pilot.json
   ```
4. Vérifiez les fautes de frappe dans les noms de paramètres

**Exemple de sortie d'erreur:**
```
CLI-0002: Paramètre obligatoire manquant.
Commande: run
Obligatoire: --project <path>
Indice: peasy-pilot run --project src/MyProject.csproj
```

**Codes associés:** CLI-0001, CLI-0003

---

### CLI-0003 — Valeur de Paramètre Invalide

**Message:** "La valeur du paramètre est invalide ou n'est pas au format attendu."

**Cause:**
- Le chemin du fichier n'existe pas
- Le nombre est en dehors de la plage valide
- Valeur d'énumération invalide
- Expression de filtre mal formée

**Solution:**
1. Vérifiez que les chemins de fichier existent:
   ```bash
   peasy-pilot run --project ./MyProject.csproj # Utilisez ./
   ```
2. Vérifiez les valeurs d'énumération:
   ```bash
   # Valides: Development, Staging, Production
   peasy-pilot run --environment Development
   ```
3. Validez la syntaxe du filtre:
   ```bash
   # Valide: "namespace=MyApp.Tests"
   peasy-pilot run --filter "namespace=MyApp.Tests"
   ```
4. Vérifiez les plages de nombres dans la documentation d'aide

**Exemple de sortie d'erreur:**
```
CLI-0003: Valeur de paramètre invalide.
Paramètre: --project
Valeur: "./NonExistent.csproj"
Statut: Fichier non trouvé
```

**Codes associés:** CLI-0001, CLI-0002

---

## Résumé

Pour obtenir de l'aide supplémentaire:
- [Référence de Dépannage Rapide](troubleshooting-quick-ref-FR.md) — Problèmes courants et diagnostics
- [Référence CLI](cli-reference-FR.md) — Syntaxe des commandes et options
- [Référence de Configuration](configuration-reference-FR.md) — Paramètres et variables d'environnement

---

**Dernière mise à jour:** 2026-09-11  
[← Retour à REFERENCE](README.md)
