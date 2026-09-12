# Dépannage de la Découverte des Tests Visual Studio

## Problème

L'Explorateur de Tests Visual Studio n'affiche pas PeasyPilot.Core.Tests, mais :
- ✅ Les tests se compilent avec succès
- ✅ `dotnet test` exécute tous les tests
- ✅ Les tests réussissent sur tous les frameworks (.NET 8, 9, 10)

**Pourquoi ?** C'est probablement un problème de cache Visual Studio ou d'adaptateur, pas une configuration du projet.

---

## Solutions

### Solution 1 : Rafraîchir l'Explorateur de Tests Visual Studio (Quick Fix)

1. Ouvrir **Test Explorer** (Menu → Tester → Explorateur de Tests)
2. Cliquer sur **Exécuter tous les tests** ou **Ctrl+R, A**
3. Attendre la découverte et l'exécution des tests
4. Fermer et rouvrir Visual Studio si les tests n'apparaissent toujours pas

**⏱️ Temps :** 30 secondes

---

### Solution 2 : Effacer le Cache Visual Studio (Efficace)

Cela résout souvent les problèmes de détection d'adaptateur :

#### Option A : Supprimer le Dossier Cache
```cmd
REM Fermer Visual Studio d'abord
rmdir "%USERPROFILE%\.vs" /s /q
```

#### Option B : Nettoyer la Solution
Dans Visual Studio :
1. **Générer** → **Nettoyer la solution**
2. Supprimer les dossiers `bin/` et `obj/` manuellement :
   ```cmd
   cd G:\MCS\Github\apps\PeasyPilot
   rmdir tests\PeasyPilot.Core.Tests\bin /s /q
   rmdir tests\PeasyPilot.Core.Tests\obj /s /q
   ```
3. **Générer** → **Regénérer la solution**
4. Rouvrir l'Explorateur de Tests

**⏱️ Temps :** 1-2 minutes

---

### Solution 3 : Vérifier l'Installation de l'Adaptateur xUnit (Vérification Config)

L'Explorateur de Tests dépend du package `xunit.runner.visualstudio` :

1. Vérifier `Directory.Packages.props` :
   ```xml
   <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.0" />
   ```

2. Vérifier dans `.csproj` :
   ```xml
   <PackageReference Include="xunit.runner.visualstudio">
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
     <PrivateAssets>all</PrivateAssets>
   </PackageReference>
   ```

3. **Vérifier l'installation :**
   ```cmd
   cd G:\MCS\Github\apps\PeasyPilot
   dotnet restore tests/PeasyPilot.Core.Tests/PeasyPilot.Core.Tests.csproj
   ```

**Statut dans ce projet :** ✅ Déjà configuré correctement

---

### Solution 4 : Exécuter les Tests en Ligne de Commande (Alternative)

L'Explorateur de Tests Visual Studio est pratique mais pas obligatoire. Utilisez plutôt l'interface CLI :

```bash
# Lister les tests (découverte détaillée)
dotnet test tests/PeasyPilot.Core.Tests --list-tests

# Exécuter une classe de test spécifique
dotnet test tests/PeasyPilot.Core.Tests --filter "FullyQualifiedName~FrameworkAdapterTests"

# Exécuter avec sortie détaillée
dotnet test tests/PeasyPilot.Core.Tests -v d

# Exécuter un seul framework
dotnet test tests/PeasyPilot.Core.Tests -f net10.0

# Exporter les résultats JUnit
dotnet test tests/PeasyPilot.Core.Tests --logger "trx;LogFileName=test-results.trx"
```

**⏱️ Temps :** Immédiat | **Fiabilité :** 100%

---

### Solution 5 : Régénérer les Fichiers de Projet Visual Studio

Parfois les métadonnées du fichier projet deviennent obsolètes :

```cmd
cd G:\MCS\Github\apps\PeasyPilot

# Générer un nouveau cache de projet
dotnet build tests/PeasyPilot.Core.Tests/PeasyPilot.Core.Tests.csproj /p:ContinuousIntegrationBuild=false

# Forcer une régénération complète
dotnet build --no-incremental
```

Ensuite :
1. Fermer Visual Studio
2. Rouvrir la solution
3. Ouvrir l'Explorateur de Tests
4. Cliquer sur "Exécuter tous les tests"

---

## État Actuel du Projet

✅ **La configuration est correcte :**
- `xunit.runner.visualstudio` v3.0.0 installé
- Les tests ont l'attribut `[Fact]`
- Multi-ciblage : net8.0, net9.0, net10.0
- `IsPackable=false` défini correctement

✅ **Les tests s'exécutent avec succès :**
```
Tests réussis : « G:\MCS\Github\apps\PeasyPilot\tests\PeasyPilot.Core.Tests\bin\Debug\net8.0\... »
Tests réussis : « G:\MCS\Github\apps\PeasyPilot\tests\PeasyPilot.Core.Tests\bin\Debug\net9.0\... »
Tests réussis : « G:\MCS\Github\apps\PeasyPilot\tests\PeasyPilot.Core.Tests\bin\Debug\net10.0\... »
```

**Problème :** Affichage dans l'Explorateur de Tests Visual Studio uniquement (problème d'enregistrement d'adaptateur)

---

## Pourquoi Cela se Produit

### Causes Principales :
1. **Corruption du cache Visual Studio** → Solution #2 corrige cela
2. **xunit.runner.visualstudio non enregistré** → Solution #3 vérifie la configuration
3. **Build pas en mode Debug** → Solution #5 regénère
4. **Recompilation du projet manquante** → Solution #2 (Nettoyer la solution)

### Pourquoi l'Interface CLI Fonctionne Mais Pas VS :
- **`dotnet test`** utilise le runner xunit.console (ne nécessite pas l'adaptateur Visual Studio)
- **L'Explorateur de Tests Visual Studio** dépend de l'enregistrement de l'adaptateur spécifique à VS
- Les adaptateurs échouent parfois à se charger sans un effacement complet du cache

---

## Approche Recommandée

Pour ce projet, utilisez **l'approche CLI (Solution #4)** :

```bash
# Exécuter tous les tests
dotnet test

# Exécuter un test spécifique
dotnet test --filter "FrameworkAdapterTests"

# Mode surveillance (regénération en cas de changement)
dotnet watch test
```

**Avantages :**
- ✅ Pas de problèmes de cache Visual Studio
- ✅ Fonctionne sur les pipelines CI/CD
- ✅ Cohérent entre Windows/Linux/Mac
- ✅ Plus rapide que l'Explorateur de Tests VS
- ✅ Meilleur pour les scripts

---

## Si l'Explorateur de Tests VS est Obligatoire

Utilisez **Test Explorer > Configurer les Paramètres d'Exécution** :

1. Test Explorer → **Paramètres** (⚙️ icône)
2. Sélectionner le fichier **Run Settings** (ou en créer un) :
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <RunSettings>
     <RunConfiguration>
       <MaxCpuCount>4</MaxCpuCount>
       <TargetFrameworkVersion>net10.0</TargetFrameworkVersion>
     </RunConfiguration>
   </RunSettings>
   ```
3. Enregistrer et rafraîchir

---

## Résumé des Commandes

### Diagnostics Rapides
```bash
# Vérifier la découverte des tests
dotnet test --list-tests

# Exécuter les tests avec l'adaptateur xunit
dotnet test -v n

# Vérifier l'installation de l'adaptateur
dotnet nuget locals all --clear
dotnet restore
```

### Effacement Complet du Cache (Option Ultime)
```cmd
REM Fermer Visual Studio
del /s /q "%USERPROFILE%\.vs"
del /s /q "%USERPROFILE%\.nuget\packages\xunit*"
dotnet nuget locals all --clear
cd G:\MCS\Github\apps\PeasyPilot
dotnet restore
REM Rouvrir Visual Studio
```

---

## Quand Utiliser Chaque Solution

| Solution | Utiliser Quand | Temps |
|----------|----------------|-------|
| #1 (Rafraîchir) | Première tentative | 30s |
| #2 (Effacer Cache) | Rafraîchir ne fonctionne pas | 1-2m |
| #3 (Vérifier Config) | Problèmes d'adaptateur persistent | 2m |
| #4 (CLI) | Vous n'avez pas besoin de l'interface VS | immédiat |
| #5 (Regénérer) | Dernier recours | 5m |

---

## Voir Aussi

- [Workflows CI/CD](./CI-CD-WORKFLOWS-FR.md) — Comment les tests sont découverts en CI
- [Référence CLI](./REFERENCE/cli-reference.md) — Options de `dotnet test`
- [Guide de Démarrage](./GETTING-STARTED-FR.md) — Configuration initiale
