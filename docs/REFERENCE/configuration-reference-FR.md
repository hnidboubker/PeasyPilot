# Référence de Configuration PeasyPilot

Référence complète pour toutes les options de configuration PeasyPilot, paramètres et règles de précédence. Frameworks cibles : .NET 8, 9, et 10.

---

## Démarrage rapide

La configuration de PeasyPilot est gérée par plusieurs sources avec un ordre de précédence clair :

```csharp
// Exemple : Configurer les options de test par programmation
var testOptions = new TestOptions
{
    Environment = "Integration",
    EnableLogging = true
};

// Exemple : Configurer les options de pipeline
var pipelineOptions = new TestPipelineOptions
{
    Filter = new NameTestFilter("UserService"),
    RunDiagnosticsOnFailure = true,
    Reporters = new[] { new ConsoleReporter(), new JsonFileReporter("./results.json") }
};
```

---

## Sources de configuration

PeasyPilot reconnaît la configuration à partir des sources suivantes (dans l'ordre de précédence) :

1. **Configuration par programmation** (précédence la plus élevée) — Paramètres de code directs
2. **Variables d'environnement** — Variables d'environnement système
3. **appsettings.json** — Fichier de paramètres d'application
4. **appsettings.{Environment}.json** — Paramètres spécifiques à l'environnement
5. **global.json** — Paramètres SDK globaux
6. **Valeurs par défaut intégrées** (précédence la plus faible) — Défauts codés en dur

---

## Précédence de configuration

Les paramètres sont appliqués dans cet ordre (du plus haut au plus bas) :

```
Configuration par programmation
    ↓
Variables d'environnement
    ↓
appsettings.{Environment}.json (ex. appsettings.Test.json)
    ↓
appsettings.json
    ↓
global.json
    ↓
Valeurs par défaut intégrées
```

La première valeur définie gagne. Par exemple, si `Environment` est défini dans `appsettings.json` et via une configuration de code, la configuration de code a la priorité.

---

## Paramètres de base

### TestOptions

Configuration pour le comportement général de l'environnement de test.

#### Environment

Le nom de l'environnement de test.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` |
| **Par défaut** | `"Development"` |
| **Valeurs valides** | `"Development"`, `"Testing"`, `"Integration"`, `"Staging"`, `"Production"` |
| **Sources de configuration** | Code, Variable d'environnement, appsettings.json |

Contrôle quel fichier de paramètres est chargé : si défini sur `"Testing"`, PeasyPilot charge `appsettings.Testing.json`.

**Configuration par programmation :**
```csharp
var options = new TestOptions { Environment = "Integration" };
```

**Variable d'environnement :**
```bash
# PowerShell
$env:PeasyPilot__Environment = "Testing"

# Bash
export PeasyPilot__Environment="Testing"
```

**appsettings.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Testing"
  }
}
```

**Environnements recommandés :**
- **Development** — Développement local, débogage activé, sortie complète
- **Testing** — Pipelines CI/CD, journalisation minimale, sortie structurée
- **Integration** — Suites de tests d'intégration, support des fixtures de base de données
- **Staging** — Validation de pré-production, suivi des performances

#### EnableLogging

Activer ou désactiver la sortie du journal des tests.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `bool` |
| **Par défaut** | `true` |
| **Sources de configuration** | Code, Variable d'environnement, appsettings.json |

Lorsqu'elle est activée, les détails de l'exécution des tests sont enregistrés sur la console et le fichier journal. Utile pour déboguer les échecs de tests.

**Configuration par programmation :**
```csharp
var options = new TestOptions { EnableLogging = true };
```

**Variable d'environnement :**
```bash
# PowerShell
$env:PeasyPilot__EnableLogging = "true"

# Bash
export PeasyPilot__EnableLogging="true"
```

**appsettings.json :**
```json
{
  "PeasyPilot": {
    "EnableLogging": true
  }
}
```

### TestPipelineOptions

Configuration pour le comportement d'exécution du pipeline de test.

#### ChangedFiles

Liste séparée par des virgules des fichiers modifiés pour l'analyse d'impact.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `IReadOnlyCollection<string>` |
| **Par défaut** | `null` |
| **Sources de configuration** | Code, Argument CLI |

Lorsqu'elle est définie, PeasyPilot effectue une analyse d'impact pour déterminer les tests affectés par les fichiers modifiés, en exécutant uniquement ces tests.

**Configuration par programmation :**
```csharp
var options = new TestPipelineOptions
{
    ChangedFiles = new[] { "src/User.cs", "src/UserRepository.cs" }
};
```

**Argument CLI :**
```bash
peasypilot --changed-files "src/User.cs,src/Order.cs"
```

#### Filter

Filtre de test pour sélectionner les tests à exécuter.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `ITestFilter` |
| **Par défaut** | `null` (pas de filtrage) |
| **Implémentation** | `NameTestFilter` (correspondance de sous-chaîne) |
| **Sources de configuration** | Code, Argument CLI |

Filtre les tests par nom en utilisant la correspondance de sous-chaîne insensible à la casse.

**Configuration par programmation :**
```csharp
var filter = new NameTestFilter("UserService");
var options = new TestPipelineOptions { Filter = filter };
```

**Argument CLI :**
```bash
peasypilot --filter "UserService"
```

#### RunDiagnosticsOnFailure

Activer les diagnostics automatiques lorsque les tests échouent.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `bool` |
| **Par défaut** | `true` |
| **Sources de configuration** | Code |

Lorsque défini sur true, des informations de diagnostic détaillées (traces de pile, sortie du journal, métriques de performance) sont collectées lorsque les tests échouent.

**Configuration par programmation :**
```csharp
var options = new TestPipelineOptions { RunDiagnosticsOnFailure = true };
```

#### Reporters

Collection de rapporteurs de test pour générer la sortie.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `IReadOnlyCollection<ITestReporter>` |
| **Par défaut** | `[]` (vide) |
| **Sources de configuration** | Code |

**Rapporteurs disponibles :**
- `ConsoleReporter` — Imprime les résultats sur la console
- `JsonFileReporter` — Sortie JSON du fichier
- `JUnitXmlReporter` — Format XML JUnit (intégration CI/CD)
- `HtmlFileReporter` — Rapport HTML interactif
- `CiAnnotationReporter` — Annotations GitHub Actions/Azure Pipelines

**Configuration par programmation :**
```csharp
var reporters = new List<ITestReporter>
{
    new ConsoleReporter(),
    new JsonFileReporter("./results.json"),
    new JUnitXmlReporter("./junit.xml")
};

var options = new TestPipelineOptions { Reporters = reporters };
```

#### Diagnostics

Collection de fournisseurs de diagnostic pour l'analyse des échecs de test.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `IReadOnlyCollection<ITestDiagnostic>` |
| **Par défaut** | `[]` (vide) |
| **Sources de configuration** | Code |

Les fournisseurs de diagnostic analysent les échecs de test et suggèrent les causes racines.

**Configuration par programmation :**
```csharp
var diagnostics = new List<ITestDiagnostic>
{
    new DefaultDiagnostic(),
    new PerformanceTracker()
};

var options = new TestPipelineOptions { Diagnostics = diagnostics };
```

---

## Fichiers de configuration

### appsettings.json

Fichier principal de configuration d'application pour les paramètres de test.

**Emplacement :** `./appsettings.json` (racine du projet)

**Format :** JSON

**Exemple appsettings.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Development",
    "EnableLogging": true,
    "TestDatabase": {
      "Engine": "InMemory",
      "ResetBetweenTests": true
    },
    "Discovery": {
      "AssemblyPattern": "**.Tests.dll",
      "IncludeFrameworks": [ "xunit", "nunit", "tunit" ]
    },
    "Reporting": {
      "DefaultFormat": "console",
      "ConsoleVerbosity": "detailed"
    }
  }
}
```

### appsettings.{Environment}.json

Configuration spécifique à l'environnement, chargée après `appsettings.json`. Les paramètres ici remplacent `appsettings.json`.

**Exemples :**
- `appsettings.Development.json` — Paramètres de développement local
- `appsettings.Testing.json` — Paramètres du pipeline CI/CD
- `appsettings.Production.json` — Paramètres de production/staging

**Exemple appsettings.Testing.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Testing",
    "EnableLogging": false,
    "TestDatabase": {
      "Engine": "InMemory",
      "ResetBetweenTests": true
    },
    "Reporting": {
      "DefaultFormat": "json",
      "OutputPath": "./build/test-results.json"
    }
  }
}
```

Lors de l'exécution dans l'environnement Testing :
```bash
# Définir l'environnement avant d'exécuter
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test

# Ou spécifier dans appsettings.json
```

### global.json

Fichier de configuration du SDK .NET (partagé avec l'ensemble de la solution).

**Emplacement :** `./ global.json` (racine du dépôt)

**Format :** JSON

**Exemple global.json :**
```json
{
  "sdk": {
    "version": "10.0"
  }
}
```

Ce fichier est automatiquement détecté par les outils .NET et ne contient pas de paramètres spécifiques à PeasyPilot, mais détermine quelle version du SDK .NET est utilisée pour la compilation et l'exécution des tests.

---

## Variables d'environnement

PeasyPilot reconnaît les variables d'environnement avec le préfixe `PeasyPilot__` (double tiret bas).

### Configuration des variables d'environnement

**PowerShell :**
```powershell
$env:PeasyPilot__Environment = "Testing"
$env:PeasyPilot__EnableLogging = "true"
```

**Bash :**
```bash
export PeasyPilot__Environment="Testing"
export PeasyPilot__EnableLogging="true"
```

**Invite de commandes Windows :**
```cmd
set PeasyPilot__Environment=Testing
set PeasyPilot__EnableLogging=true
```

**Dans les pipelines CI/CD (GitHub Actions) :**
```yaml
env:
  PeasyPilot__Environment: Testing
  PeasyPilot__EnableLogging: "false"
```

### Variables d'environnement supportées

| Variable | Type | Par défaut | Exemple |
|----------|------|-----------|---------|
| `PeasyPilot__Environment` | string | `"Development"` | `Testing` |
| `PeasyPilot__EnableLogging` | bool | `true` | `false` |
| `PeasyPilot__LogLevel` | string | `"Information"` | `"Debug"` |
| `PeasyPilot__TestTimeout` | int | `30000` | `60000` |

---

## Exemples de configuration

### Environnement Development

Développement local avec journalisation complète et sortie console :

**appsettings.Development.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Development",
    "EnableLogging": true,
    "Logging": {
      "LogLevel": "Debug"
    },
    "Reporting": {
      "ConsoleVerbosity": "detailed"
    }
  }
}
```

**Exécution :**
```bash
dotnet test
```

### Environnement Testing (CI/CD)

Journalisation minimale, sortie structurée pour les systèmes CI/CD :

**appsettings.Testing.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Testing",
    "EnableLogging": false,
    "TestDatabase": {
      "Engine": "InMemory"
    },
    "Reporting": {
      "DefaultFormat": "json",
      "OutputPath": "./build/test-results.json"
    }
  }
}
```

**Exécution :**
```bash
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test
```

### Test d'intégration

Support des fixtures de base de données et paramètres spécifiques à l'intégration :

**appsettings.Integration.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Integration",
    "EnableLogging": true,
    "TestDatabase": {
      "Engine": "SqlServer",
      "ConnectionString": "Server=(local);Database=PeasyPilot_Tests;Integrated Security=true;",
      "ResetBetweenTests": true
    },
    "Discovery": {
      "IncludeIntegrationTests": true
    }
  }
}
```

**Exécution :**
```bash
$env:ASPNETCORE_ENVIRONMENT = "Integration"
dotnet test
```

### Validation production/staging

Suivi des performances et rapports complets :

**appsettings.Staging.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Staging",
    "EnableLogging": true,
    "Reporting": {
      "DefaultFormat": "html",
      "OutputPath": "./reports/test-report.html"
    },
    "Performance": {
      "EnableTracking": true,
      "ThresholdMs": 5000
    }
  }
}
```

**Exécution :**
```bash
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet test
```

---

## Exemples de priorité de configuration

### Exemple 1 : Remplacement d'environnement

**Scénario :** `appsettings.json` définit `EnableLogging: true`, mais vous voulez le désactiver pour cette exécution.

```bash
# Définir une variable d'environnement (a la priorité sur appsettings.json)
$env:PeasyPilot__EnableLogging = "false"
dotnet test

# La journalisation est maintenant désactivée, même si appsettings.json dit true
```

### Exemple 2 : Configuration par programmation

**Scénario :** La configuration de code remplace tous les paramètres basés sur des fichiers.

```csharp
// Configuration par programmation (précédence la plus élevée)
var options = new TestOptions { Environment = "Integration" };

// Cela a la priorité sur :
// - appsettings.Integration.json
// - Variable d'environnement
// - appsettings.json
// - global.json
// - valeurs par défaut
```

### Exemple 3 : Fichiers spécifiques à l'environnement

**Scénario :** Les environnements Development et Testing utilisent des paramètres différents.

**appsettings.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Development",
    "EnableLogging": true
  }
}
```

**appsettings.Testing.json :**
```json
{
  "PeasyPilot": {
    "Environment": "Testing",
    "EnableLogging": false
  }
}
```

**Exécution locale :**
```bash
# Utilise appsettings.json (Development)
dotnet test
```

**Exécution en CI/CD :**
```bash
# Définir l'environnement sur Testing, qui charge appsettings.Testing.json
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test
```

---

## Règles de validation

### Environment

- **Valeurs autorisées :** `"Development"`, `"Testing"`, `"Integration"`, `"Staging"`, `"Production"`
- **Insensible à la casse :** `"development"` = `"Development"`
- **Valeurs invalides :** Valeur par défaut sur `"Development"` avec avertissement

### EnableLogging

- **Valeurs autorisées :** `true`, `false`
- **Chaînes insensibles à la casse :** `"true"`, `"false"`, `"True"`, `"False"`
- **Valeurs invalides :** Valeur par défaut sur `true` avec avertissement

### TestPipelineOptions

- **Filter :** Doit être une implémentation valide de `ITestFilter`
- **Reporters :** Doit implémenter l'interface `ITestReporter`
- **Diagnostics :** Doit implémenter l'interface `ITestDiagnostic`
- **ChangedFiles :** Doit être des chemins de fichier valides (relatif ou absolu)

---

## Dépannage

### La configuration n'est pas appliquée

**Symptôme :** Définir une valeur dans `appsettings.json` mais les tests utilisent toujours la valeur par défaut.

**Diagnostic :**
1. Vérifier que l'environnement correct est défini
2. Vérifier le format du fichier (JSON valide)
3. Vérifier la précédence — le code programmatique remplace les fichiers

**Solution :**
```bash
# Vérifier l'environnement actuel
echo $env:ASPNETCORE_ENVIRONMENT

# Vérifier que le fichier appsettings existe et est un JSON valide
cat ./appsettings.json
```

### « Impossible de charger appsettings.json »

**Symptôme :** L'interface CLI signale que le fichier de configuration est introuvable.

**Solution :**
1. Vérifier que `appsettings.json` existe à la racine du projet
2. S'assurer que le chemin du fichier est correct
3. Vérifier les permissions du fichier (doit être lisible)

```bash
# Vérifier que le fichier existe
ls ./appsettings.json

# Ou sur Windows
dir appsettings.json
```

### Fichier spécifique à l'environnement non chargé

**Symptôme :** Définir `ASPNETCORE_ENVIRONMENT=Testing` mais `appsettings.Testing.json` n'est pas chargé.

**Solution :**
1. Vérifier que le fichier existe : `appsettings.Testing.json`
2. S'assurer que la variable d'environnement est définie correctement avant d'exécuter les tests
3. Redémarrer votre terminal/IDE après avoir défini les variables d'environnement

```bash
# Vérifier que la variable d'environnement est définie
echo $env:ASPNETCORE_ENVIRONMENT

# Essayer avec l'environnement explicite
dotnet test --configuration Release
```

---

## Meilleures pratiques

1. **Utiliser appsettings.json pour les valeurs par défaut** — Définir les valeurs par défaut sensibles dans `appsettings.json`
2. **Utiliser appsettings.{Environment}.json pour les remplacements** — Remplacer les valeurs par défaut par environnement
3. **Utiliser les variables d'environnement pour CI/CD** — Définir les variables dans la configuration du pipeline CI/CD
4. **Utiliser la configuration de code avec parcimonie** — Uniquement pour les paramètres dynamiques déterminés à l'exécution
5. **Garder la journalisation activée en Development** — Aide à déboguer les échecs de tests localement
6. **Garder la journalisation désactivée en Testing (CI/CD)** — Réduit le bruit et accélère les pipelines
7. **Utiliser les formats structurés (JSON)** — Plus facile à analyser et à analyser en CI/CD

---

## Documentation connexe

- [Référence CLI](./cli-reference-FR.md) — Référence de l'interface de ligne de commande
- [Guide de démarrage](../GETTING-STARTED-FR.md) — Configuration en pratique
- [Référence rapide de dépannage](./troubleshooting-quick-ref-FR.md) — Problèmes de configuration courants

---

**Dernière mise à jour :** 2026-09-11  
**Support de framework :** .NET 8, 9, 10
