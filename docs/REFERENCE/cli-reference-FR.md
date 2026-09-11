# Référence CLI PeasyPilot

Référence complète pour tous les commandes de ligne de commande (CLI) PeasyPilot, options et exemples. Frameworks cibles : .NET 8, 9, et 10.

---

## Démarrage rapide

L'interface CLI de PeasyPilot est invoquée via la commande `peasypilot` :

```bash
# Afficher l'aide
peasypilot --help

# Exécuter tous les tests
peasypilot

# Exécuter les tests avec filtrage
peasypilot --filter "UserService"

# Exécuter avec analyse d'impact (fichiers modifiés)
peasypilot --changed-files "src/User.cs,src/Order.cs"

# Générer des suggestions de tests
peasypilot suggest-tests --assembly ./bin/Release/net9.0/MyApp.dll --type MyApp.UserService

# Afficher l'historique des exécutions de tests
peasypilot history
```

---

## Aperçu des commandes

PeasyPilot fournit quatre commandes principales :

1. **Exécution de tests** — Exécuter les tests avec filtrage et analyse d'impact
2. **Historique** — Afficher les enregistrements des exécutions de tests précédentes
3. **Génération de tests** — Suggérer et générer des suites de tests
4. **Aide** — Afficher les informations d'utilisation

---

## Commande : Exécution de tests

Exécutez les tests de la solution actuelle avec options de filtrage, planification et rapport.

### Syntaxe

```bash
peasypilot [options]
```

### Options

#### `--filter <name>` | `-f <name>`

Filtrer les tests par nom (correspondance de sous-chaîne insensible à la casse).

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` |
| **Requis** | Non |
| **Par défaut** | `null` (sans filtrage) |
| **Exemple** | `--filter "UserService"` |

Correspond à tous les tests contenant la sous-chaîne "UserService" dans son nom pleinement qualifié.

```bash
# Exécuter uniquement les tests avec "UserService" dans le nom
peasypilot --filter "UserService"

# Exécuter les tests correspondant à plusieurs mots-clés (insensible à la casse)
peasypilot -f "Repository"
```

#### `--changed-files <files>` | `-c <files>`

Liste séparée par des virgules des chemins de fichiers modifiés pour l'analyse d'impact. Lorsqu'elle est fournie, PeasyPilot effectue une analyse d'impact pour identifier uniquement les tests affectés par les fichiers modifiés.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (chemins séparés par des virgules) |
| **Requis** | Non |
| **Par défaut** | `null` (tous les tests planifiés) |
| **Exemple** | `--changed-files "src/User.cs,src/Order.cs"` |

Les chemins de fichiers peuvent être :
- Chemins relatifs : `src/User.cs`
- Chemins absolus : `/home/user/project/src/User.cs`
- Jokers : `src/*.cs` (traités comme des chemins littéraux)

```bash
# Exécuter uniquement les tests affectés par les changements de User.cs et Order.cs
peasypilot --changed-files "src/User.cs,src/Order.cs"

# Analyse d'impact sur Windows
peasypilot -c "src\User.cs,src\Order.cs"

# Plusieurs changements de fichiers
peasypilot -c "src/User.cs,src/UserRepository.cs,tests/UserTests.cs"
```

#### `--format <format>` | `-fmt <format>`

Format de sortie pour le rapport des résultats des tests.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (enum : `console`, `json`, `junit`) |
| **Requis** | Non |
| **Par défaut** | `console` |
| **Valeurs valides** | `console`, `json`, `junit` |

Formats supportés :

- **`console`** — Sortie console lisible par l'homme avec couleurs et résumé
- **`json`** — Format JSON lisible par machine pour consommation programmatique
- **`junit`** — Format XML JUnit pour l'intégration CI/CD (Jenkins, Azure DevOps, GitHub Actions)

```bash
# Sortie console (par défaut)
peasypilot --format console

# Sortie JSON pour analyse
peasypilot --format json

# XML JUnit pour CI/CD
peasypilot --format junit
```

#### `--output <path>` | `-o <path>`

Chemin du fichier pour enregistrer le rapport de test. Le format est auto-détecté à partir de l'extension du fichier ou de l'option `--format`.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (chemin du fichier) |
| **Requis** | Non |
| **Par défaut** | `null` (sortie console uniquement) |
| **Exemple** | `--output "results.json"` |

Le chemin de sortie peut inclure :
- Chemins relatifs : `./results/report.json`
- Chemins absolus : `/var/log/test-results.xml`
- Les répertoires sont créés s'ils n'existent pas

Si `--output` et `--format` sont tous deux spécifiés, le format doit être compatible avec l'extension du fichier. Si l'extension du fichier entre en conflit avec le format, l'extension du fichier a la priorité.

```bash
# Enregistrer le rapport JSON
peasypilot --format json --output "./reports/test-results.json"

# Enregistrer XML JUnit (auto-détecté à partir de l'extension)
peasypilot --output "./results/junit.xml"

# Plusieurs rapporteurs (console + fichier)
peasypilot --format json -o "./results/results.json"
```

### Options globales

#### `--help` | `-h` | `help`

Afficher les informations d'aide pour la CLI.

```bash
peasypilot --help
peasypilot -h
peasypilot help
```

### Codes de sortie

| Code | Signification |
|------|---------------|
| **0** | Tous les tests ont réussi |
| **1** | Un ou plusieurs tests ont échoué, ou une erreur s'est produite |

### Exemples

#### Exemple 1 : Exécution de test basique

```bash
peasypilot
```

Découvre et exécute tous les tests de la solution. La sortie est imprimée sur la console.

**Sortie attendue :**
```
[PeasyPilot CLI] Executing Test Pipeline...
[PeasyPilot CLI] Status: Passed | Discovered: 42 | Scheduled: 42 | Passed: 42 | Failed: 0
```

#### Exemple 2 : Exécution de test filtrée

```bash
peasypilot --filter "UserService"
```

Exécute uniquement les tests avec "UserService" dans leur nom.

**Sortie attendue :**
```
[PeasyPilot CLI] Executing Test Pipeline...
[PeasyPilot CLI] Status: Passed | Discovered: 42 | Scheduled: 8 | Passed: 8 | Failed: 0
```

#### Exemple 3 : Analyse d'impact

```bash
peasypilot --changed-files "src/UserService.cs,src/UserRepository.cs"
```

Analyse les tests qui dépendent des fichiers modifiés et exécute uniquement ces tests.

**Sortie attendue :**
```
[PeasyPilot CLI] Executing Test Pipeline...
Impact Analysis: 15 of 42 tests affected by changes
[PeasyPilot CLI] Status: Passed | Discovered: 42 | Scheduled: 15 | Passed: 15 | Failed: 0
```

#### Exemple 4 : Rapport JSON

```bash
peasypilot --format json --output "./results/test-report.json"
```

Exécute tous les tests et enregistre les résultats dans `./results/test-report.json` au format JSON.

**Schéma JSON :**
```json
{
  "status": "Passed",
  "discoveredCount": 42,
  "scheduledCount": 42,
  "aggregateRunResult": {
    "passed": 42,
    "failed": 0,
    "skipped": 0,
    "duration": "00:00:05.123"
  }
}
```

#### Exemple 5 : XML JUnit pour CI/CD

```bash
peasypilot --output "./build/test-results.xml"
```

Exécute les tests et génère le format XML JUnit (auto-détecté à partir de l'extension `.xml`) pour GitHub Actions, Azure Pipelines ou Jenkins.

#### Exemple 6 : Filtrage et rapport combinés

```bash
peasypilot --filter "Repository" --format json -o "./reports/repo-tests.json"
```

Exécute les tests correspondant à "Repository", génère un rapport JSON et enregistre dans `./reports/repo-tests.json`.

---

## Commande : Historique de tests

Affiche les enregistrements des exécutions de tests enregistrées précédemment.

### Syntaxe

```bash
peasypilot history
```

### Options

Aucune. La commande `history` affiche par défaut les 10 exécutions les plus récentes.

### Format de sortie

La sortie console affiche :
- **Horodatage d'exécution** — Date et heure de l'exécution du test
- **ID d'exécution** — Identifiant unique de l'exécution
- **Statut** — Résultat global (Passed, Failed, Skipped)
- **Découverts** — Nombre de tests découverts
- **Passés/Échoués** — Résultats des tests

### Exemples

```bash
peasypilot history
```

**Sortie :**
```
[PeasyPilot CLI] Test Execution History:
[2026-09-11 14:23:45] Run ID: 550e8400-e29b-41d4-a716-446655440000 | Status: Passed | Discovered: 42 | Passed: 42 | Failed: 0
[2026-09-11 10:15:32] Run ID: 550e8400-e29b-41d4-a716-446655440001 | Status: Failed | Discovered: 42 | Passed: 40 | Failed: 2
[2026-09-11 08:45:12] Run ID: 550e8400-e29b-41d4-a716-446655440002 | Status: Passed | Discovered: 42 | Passed: 42 | Failed: 0
```

---

## Commande : Génération de tests (suggest-tests)

Générez des propositions de suite de tests pour un type donné en utilisant l'analyse assistée par IA et la génération de code.

### Syntaxe

```bash
peasypilot suggest-tests --assembly <path> --type <name> [options]
```

### Options requises

#### `--assembly <path>` | `-a <path>`

Chemin de l'assembly compilé (.dll) à analyser.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (chemin du fichier) |
| **Requis** | **Oui** |
| **Exemple** | `./bin/Release/net9.0/MyApp.dll` |

Doit être un fichier d'assembly .NET valide. Le chemin peut être :
- Relatif : `./bin/Release/net9.0/MyApp.dll`
- Absolu : `/home/user/project/bin/Release/net9.0/MyApp.dll`

#### `--type <name>` | `-t <name>`

Nom du type cible pour lequel générer les tests.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (nom de type pleinement qualifié ou nom simple) |
| **Requis** | **Oui** |
| **Exemple** | `MyApp.Services.UserService` ou `UserService` |

Peut être l'un ou l'autre :
- **Nom pleinement qualifié** (préféré) : `MyApp.Services.UserService`
- **Nom simple** : `UserService` (correspond à la première occurrence)

### Options optionnelles

#### `--framework <fw>` | `-fw <fw>`

Framework de test cible pour la génération de code.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (enum : `xunit`, `nunit`, `tunit`) |
| **Requis** | Non |
| **Par défaut** | `xunit` |
| **Valeurs valides** | `xunit`, `nunit`, `tunit` |

```bash
# Générer les tests xUnit (par défaut)
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService

# Générer les tests NUnit
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fw nunit

# Générer les tests TUnit
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fw tunit
```

#### `--output-dir <dir>` | `-o <dir>`

Répertoire de sortie pour les fichiers de test générés.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (chemin du répertoire) |
| **Requis** | Non |
| **Par défaut** | `./generated-tests` |
| **Exemple** | `./tests/generated` |

Le répertoire est créé s'il n'existe pas.

```bash
# Enregistrer dans un répertoire personnalisé
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -o ./tests/generated
```

#### `--format <fmt>` | `-fmt <fmt>`

Format du fichier de sortie pour la génération de tests.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `string` (enum : `json`, `cs`, `both`) |
| **Requis** | Non |
| **Par défaut** | `json` |
| **Valeurs valides** | `json`, `cs`, `both` |

- **`json`** — Fichier de plan de test JSON (analyse + recommandations)
- **`cs`** — Fichier de code source C# (prêt à intégrer)
- **`both`** — Fichiers JSON et C#

```bash
# Générer l'analyse JSON
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fmt json

# Générer le code C#
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fmt cs

# Générer les deux
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fmt both
```

#### `--max-enum-cases <count>` | `-m <count>`

Nombre maximum de valeurs d'énumération à tester.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `int` |
| **Requis** | Non |
| **Par défaut** | `8` |
| **Plage valide** | `1–100` |
| **Exemple** | `-m 16` |

Lors de l'analyse des paramètres enum, cela limite le nombre de cas de test générés. Utile pour les grandes enums afin d'éviter l'explosion de tests.

```bash
# Tester jusqu'à 16 valeurs d'énumération
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -m 16
```

#### `--force`

Remplacer les fichiers de proposition existants sans confirmation.

| Propriété | Valeur |
|-----------|--------|
| **Type** | `bool` (drapeau) |
| **Requis** | Non |
| **Par défaut** | `false` |

Par défaut, si les fichiers de sortie existent déjà, la commande échoue. Utilisez `--force` pour remplacer.

```bash
# Remplacer les fichiers existants
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService --force
```

### Codes de sortie

| Code | Signification |
|------|---------------|
| **0** | Proposition de test générée avec succès |
| **1** | Erreur : assembly non trouvé, type non trouvé, ou permission refusée |

### Exemples

#### Exemple 1 : Générer les tests xUnit (Par défaut)

```bash
peasypilot suggest-tests --assembly ./bin/Release/net9.0/MyApp.dll --type UserService
```

Génère une proposition de test pour `UserService` sous forme de tests xUnit dans `./generated-tests/UserServiceTests.Proposed.cs`.

#### Exemple 2 : Générer les tests NUnit

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t OrderService -fw nunit
```

Génère une suite de tests NUnit pour `OrderService` avec les attributs `[TestFixture]` et `[Test]`.

#### Exemple 3 : Générer JSON et C#

```bash
peasypilot suggest-tests \
  --assembly ./bin/Release/net9.0/MyApp.dll \
  --type PaymentProcessor \
  --framework tunit \
  --output-dir ./tests/generated \
  --format both
```

Sorties :
- `./tests/generated/PaymentProcessor.testbattery.json` — Analyse du plan de test
- `./tests/generated/PaymentProcessorTests.Proposed.cs` — Code C# compatible TUnit

#### Exemple 4 : Analyse des grandes énumérations

```bash
peasypilot suggest-tests \
  -a ./bin/Release/net9.0/MyApp.dll \
  -t ReportGenerator \
  -m 20
```

Teste jusqu'à 20 valeurs de paramètres enum (au lieu du défaut 8).

---

## Précédence de configuration

Les options CLI remplacent les paramètres de fichier de configuration et les variables d'environnement.

**Précédence (du plus haut au plus bas) :**
1. Arguments de ligne de commande (`--filter`, `--format`, etc.)
2. Variables d'environnement (si supportées dans les futures versions)
3. Fichiers de configuration (global.json, appsettings.json)
4. Valeurs par défaut intégrées

Par exemple :

```bash
# CLI remplace le format par défaut
peasypilot --format json
```

---

## Dépannage

### « Assembly non trouvé »

```bash
peasypilot suggest-tests -a ./bin/Release/MyApp.dll -t UserService
Error: Assembly not found: ./bin/Release/MyApp.dll
```

**Solution :**
1. Vérifier que le chemin est correct
2. S'assurer que le projet est construit en mode Release
3. Utiliser des chemins absolus si les chemins relatifs échouent

```bash
# Reconstruire le projet d'abord
dotnet build --configuration Release

# Puis exécuter avec le chemin correct
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService
```

### « Type non trouvé »

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t NonExistentClass
Error: Type 'NonExistentClass' not found in assembly.
```

**Solution :**
1. Utiliser le nom de type pleinement qualifié : `MyApp.Services.UserService`
2. Vérifier que le type est public et exporté à partir de l'assembly
3. Vérifier que l'assembly contient les types attendus

```bash
# Utiliser le nom pleinement qualifié
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t "MyApp.Services.UserService"
```

### « Fichiers de sortie existent déjà »

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService
Error: Output files already exist. Use --force to overwrite.
```

**Solution :** Utiliser `--force` pour remplacer :

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService --force
```

### Filtre de test ne correspondant pas

```bash
peasypilot --filter "User"
# Aucun test exécuté, mais certains devraient correspondre
```

**Solution :**
1. Vérifier que le filtre correspond aux noms de tests (sous-chaîne insensible à la casse)
2. Utiliser des chaînes de filtrage plus courtes
3. Vérifier que la découverte de tests fonctionne

```bash
# Exécuter sans filtre pour vérifier que les tests existent
peasypilot

# Puis utiliser une sous-chaîne connue
peasypilot --filter "UserService"
```

---

## Environnement

- **Versions .NET supportées :** .NET 8, 9, 10
- **Plates-formes supportées :** Windows, Linux, macOS
- **Shell :** PowerShell, Bash, Command Prompt

---

## Documentation connexe

- [Référence de configuration](./configuration-reference-FR.md) — Options de configuration et précédence
- [Guide de démarrage](../GETTING-STARTED-FR.md) — Premiers pas avec PeasyPilot
- [Référence rapide de dépannage](./troubleshooting-quick-ref-FR.md) — Problèmes courants et solutions

---

**Dernière mise à jour :** 2026-09-11  
**Support de framework :** .NET 8, 9, 10
