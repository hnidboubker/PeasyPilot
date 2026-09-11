# Guide de Migration et Mise à Jour

## Aperçu

Ce guide vous aide à naviguer la mise à jour de PeasyPilot vers la dernière version tout en gérant les changements cassants, les API dépréciées et en assurant que vos tests continuent à fonctionner correctement.

**Temps estimé:** 30-60 minutes selon la complexité du projet  
**Frameworks:** xUnit, NUnit, TUnit  
**Prérequis:** [Guide de Démarrage](../GETTING-STARTED-FR.md)

---

## Table des matières

1. [Matrice de Compatibilité des Versions](#matrice-de-compatibilité-des-versions)
2. [Avant la Mise à Jour](#avant-la-mise-à-jour)
3. [Chemins de Migration](#chemins-de-migration)
4. [Changements Cassants par Version](#changements-cassants-par-version)
5. [Migration des Données et Configuration](#migration-des-données-et-configuration)
6. [Tests Après Mise à Jour](#tests-après-mise-à-jour)
7. [Politique de Dépréciation](#politique-de-dépréciation)
8. [Procédures de Restauration](#procédures-de-restauration)
9. [FAQ](#faq)
10. [Obtenir de l'Aide](#obtenir-de-laide)

---

## Matrice de Compatibilité des Versions

### Support des Frameworks .NET

| Version PeasyPilot | .NET 8.0 | .NET 9.0 | .NET 10.0 | Statut |
|---|---|---|---|---|
| 0.1.x (actuel) | ✅ | ✅ | ✅ | Actif |
| 0.2.x (à venir) | ✅ | ✅ | ✅ | Planifié |
| 1.0.x (futur) | ✅ | ✅ | ✅ | Planifié |

### Compatibilité des Versions de Packages

Tous les packages PeasyPilot sont distribués de manière synchronisée. Lors de la mise à jour, assurez-vous que tous les packages utilisent la même version :

```xml
<!-- ❌ NE PAS MÉLANGER LES VERSIONS -->
<PackageReference Include="PeasyPilot.Core" Version="0.1.5" />
<PackageReference Include="PeasyPilot.Unit" Version="0.1.3" />

<!-- ✅ ALIGNEZ LES VERSIONS -->
<PackageReference Include="PeasyPilot.Core" Version="0.1.5" />
<PackageReference Include="PeasyPilot.Unit" Version="0.1.5" />
```

### Exigences de Dépendances

| Version PeasyPilot | Min C# | Min MSBuild | Frameworks Testés |
|---|---|---|---|
| 0.1.x | 10.0 | 17.0 | xUnit 2.4+, NUnit 3.13+, TUnit 1.0+ |
| 0.2.x | 11.0 | 17.5 | xUnit 2.6+, NUnit 4.0+, TUnit 1.1+ |

---

## Avant la Mise à Jour

### 1. Vérifiez Votre Version Actuelle

```bash
# Lister les packages PeasyPilot installés
dotnet package list PeasyPilot
```

### 2. Consultez les Notes de Sortie

Visitez la page [GitHub Releases](https://github.com/hnidboubker/PeasyPilot/releases) pour :
- Les nouvelles fonctionnalités
- Les changements cassants
- Les déprécations
- Les guides de migration

### 3. Sauvegardez Votre Code

```bash
# Créer une branche de sauvegarde sûre
git checkout -b backup/before-upgrade-0.1.5
git push origin backup/before-upgrade-0.1.5
```

### 4. Exécutez les Tests Actuels

Assurez-vous que tous les tests passent avant la mise à jour :

```bash
dotnet build
dotnet test
```

**Statut:** Tous les tests doivent passer avant de continuer.

---

## Chemins de Migration

### Chemin 1 : Mise à Jour Simple de Patch (0.1.4 → 0.1.5)

**Temps:** 5 minutes | **Risque:** Faible | **Changements cassants:** Aucun

#### Étape 1 : Mettre à jour les packages NuGet

```bash
dotnet package update --upgrade-dependency "PeasyPilot*"
```

Ou dans le Gestionnaire de Packages Visual Studio :
```
Update-Package PeasyPilot* -IncludePrerelease
```

#### Étape 2 : Recompiler

```bash
dotnet clean
dotnet build
```

#### Étape 3 : Exécuter les tests

```bash
dotnet test
```

**Résultat attendu:** Tous les tests passent sans modifications de code.

---

### Chemin 2 : Mise à Jour de Version Mineure (0.1.x → 0.2.x)

**Temps:** 15-30 minutes | **Risque:** Faible à Moyen | **Changements cassants:** Oui, avec guidance

#### Étape 1 : Mettre à jour les packages

```bash
dotnet package update --upgrade-dependency "PeasyPilot*" --version-range "0.2"
```

#### Étape 2 : Examinez les changements cassants

Voir [Changements Cassants par Version](#changements-cassants-par-version) pour des conseils détaillés.

#### Étape 3 : Mettez à jour votre code

Utilisez le guide de migration ci-dessous pour mettre à jour votre code afin d'utiliser les nouvelles API.

#### Étape 4 : Vérifiez la compilation

```bash
dotnet build
```

Si la compilation échoue, voir [FAQ](#faq) pour les problèmes courants.

#### Étape 5 : Exécutez les tests

```bash
dotnet test
```

---

### Chemin 3 : Mise à Jour de Version Majeure (0.x → 1.0)

**Temps:** 1-2 heures | **Risque:** Élevé | **Changements cassants:** Majeurs

Les mises à jour de version majeure nécessitent des modifications de code importantes. Planifiez en conséquence :

1. **Planification:** Dédier du temps focalisé (pas pendant les sprints urgents)
2. **Branche:** Créer une branche dédiée à la mise à jour
3. **Examen:** Lire toutes les sections [Changements Cassants](#changements-cassants-par-version)
4. **Migration:** Suivre les guides de migration tier par tier
5. **Test:** Tests approfondis requis
6. **Révision:** Révision du code avant fusion à main

#### Exemple : Processus de Mise à Jour Majeure

```bash
# Créer une branche de mise à jour
git checkout -b feat/upgrade-to-1.0
git push origin feat/upgrade-to-1.0

# Mettre à jour les packages
dotnet package update --upgrade-dependency "PeasyPilot*" --version-range "1.0"

# Apporter les modifications de code (suivre les guides ci-dessous)
# ...

# Tester en détail
dotnet test

# Créer une demande de fusion pour révision
# ... (créer PR dans GitHub)
```

---

## Changements Cassants par Version

### 0.1.5 → 0.2.0

#### Changement 1 : ITestFixture Renommé en ITestContext

**Impact:** Moyen  
**Packages affectés:** PeasyPilot.Core, tous les packages framework

**Ancienne API :**
```csharp
using PeasyPilot.Core;

public class UserRepositoryTests : XUnitTestFixture
{
    public void Test_Method()
    {
        // Méthodes ITestFixture
        var context = Fixture.GetContext();
    }
}
```

**Nouvelle API :**
```csharp
using PeasyPilot.Core;

public class UserRepositoryTests : XUnitTestContext
{
    public void Test_Method()
    {
        // Méthodes ITestContext
        var context = Context.GetContext();
    }
}
```

**Étapes de migration :**
1. Remplacer `XUnitTestFixture` par `XUnitTestContext`
2. Remplacer la propriété `Fixture` par `Context`
3. Mettre à jour les noms de méthodes : `Fixture.GetContext()` → `Context.GetContext()`

**Correction automatisée (recherche et remplacement) :**
```regex
Chercher:    Fixture\.
Remplacer:   Context.
```

---

#### Changement 2 : Méthodes Dépréciées Supprimées

**Impact:** Faible  
**Packages affectés:** PeasyPilot.Unit

Les méthodes dépréciées suivantes de 0.1.x sont supprimées :

- `TestBuilder.WithTimeout()` → Utiliser l'attribut `[Timeout(ms)]` à la place
- `TestBuilder.WithIgnore()` → Utiliser l'attribut `[Skip("reason")]` à la place
- `AssertThat.IsEqual()` → Utiliser `Assert.Equal()` directement depuis xUnit

**Exemple de migration :**
```csharp
// ❌ ANCIEN (0.1.x)
[Fact]
public void Test_Method()
{
    var builder = new TestBuilder()
        .WithTimeout(5000)
        .WithIgnore("Not ready yet");
}

// ✅ NOUVEAU (0.2.x)
[Fact(Timeout = 5000)]
[Skip("Not ready yet")]
public void Test_Method()
{
    // Votre test
}
```

---

#### Changement 3 : Les Modèles de Liaison BDD Step Changent

**Impact:** Élevé pour les utilisateurs BDD, Faible pour les autres  
**Packages affectés:** PeasyPilot.BDD

Les modèles de liaison d'étapes sont maintenant plus strictes avec une meilleure validation :

**Ancien modèle :**
```csharp
[Given("I have {count} users")]
public void CreateUsers(string count)
{
    var num = int.Parse(count); // Conversion manuelle
}
```

**Nouveau modèle :**
```csharp
[Given("I have {count:int} users")]
public void CreateUsers(int count)
{
    // Conversion de type automatique
}
```

**Paramètres de type supportés :**
- `{name}` - string (défaut)
- `{count:int}` - entier
- `{amount:decimal}` - décimal
- `{enabled:bool}` - booléen
- `{date:date}` - DateTime

**Checklist de migration :**
- [ ] Mettre à jour les modèles d'étapes avec des indices de type explicites
- [ ] Supprimer les conversions de type manuelles des méthodes d'étapes
- [ ] Tester avec `dotnet test` pour s'assurer que les modèles correspondent encore

---

### 0.2.0 → 1.0.0 (Futur)

À documenter lors de la sortie de 1.0.0. Abonnez-vous aux [notifications de sortie](https://github.com/hnidboubker/PeasyPilot/releases) pour les mises à jour.

---

## Migration des Données et Configuration

### Migration des Résultats de Tests

Si vous stockez les résultats de tests (via PeasyPilot.Coverage), la migration est automatique pour les mises à jour de patch.

Pour les mises à jour mineures/majeures, les schémas de résultats de tests peuvent changer :

```csharp
// Format 0.1.x
{
  "testName": "UserRepository_CreateUser_Success",
  "duration": 45,
  "passed": true
}

// Format 0.2.x (compatible avec les versions antérieures)
{
  "id": "unique-id",
  "testName": "UserRepository_CreateUser_Success",
  "duration": 45,
  "passed": true,
  "tags": ["unit", "repository"],
  "coveredTypes": ["User", "UserRepository"]
}
```

**Aucune action requise:** L'ancien format est automatiquement mis à niveau lors de la première lecture.

---

### Fichiers de Configuration

Si vous utilisez `PeasyPilot.CLI` avec un fichier de configuration (`peasy.config.json`) :

**Format 0.1.x :**
```json
{
  "testFilter": "Category=Unit",
  "parallel": true,
  "timeout": 30000
}
```

**Format 0.2.x :**
```json
{
  "discovery": {
    "filter": "Category=Unit",
    "includeSkipped": false
  },
  "execution": {
    "parallel": true,
    "timeout": 30000,
    "retries": 0
  }
}
```

**Migration:** Votre ancienne configuration fonctionnera mais déclenchera des avertissements de dépréciation. Mettez à jour à votre convenance.

---

### Extensions Personnalisées

Si vous avez créé des extensions personnalisées (ITestDiscovery, ITestOrchestrator) :

**Avant la mise à jour :**
1. Examinez les interfaces d'extension dans [Architecture](../.agents/05_ARCHITECTURE.md)
2. Consultez la [Référence API](../REFERENCE/core-package-reference.md)
3. Testez en détail après la mise à jour

**Après la mise à jour :**
```csharp
// Votre implémentation personnalisée
public class CustomTestOrchestrator : ITestOrchestrator
{
    // Les détails d'implémentation peuvent avoir changé
    // Examiné la définition d'interface pour les mises à jour
}
```

---

## Tests Après Mise à Jour

### Checklist de Validation

Utilisez cette checklist après chaque mise à jour :

- [ ] Tous les packages mis à jour vers la même version
- [ ] Le projet se compile sans erreurs (`dotnet build`)
- [ ] Tous les tests passent (`dotnet test`)
- [ ] Aucun avertissement du compilateur à propos des API dépréciées
- [ ] Les tests spécifiques aux frameworks passent (xUnit/NUnit/TUnit)
- [ ] Les tests d'intégration passent (base de données, fixtures HTTP)
- [ ] Les tests BDD exécutent correctement les scénarios
- [ ] Les benchmarks de performance se situent à 5% de la base de référence
- [ ] Le pipeline CI/CD passe

### Tests de Régression

Exécutez des suites de tests focalisées pour détecter les régressions :

```bash
# Tests unitaires uniquement
dotnet test --filter "Category=Unit"

# Tests d'intégration uniquement
dotnet test --filter "Category=Integration"

# Tests BDD uniquement
dotnet test --filter "Category=BDD"

# Exécuter avec sortie détaillée
dotnet test --verbosity detailed
```

### Tests de Performance

Comparer les temps d'exécution des tests avant/après mise à jour :

```bash
# Avant la mise à jour (sur la branche de sauvegarde)
git checkout backup/before-upgrade-0.1.5
dotnet test --logger "console;verbosity=minimal" > results-before.txt

# Après la mise à jour
git checkout feat/upgrade-to-0.2.x
dotnet test --logger "console;verbosity=minimal" > results-after.txt

# Comparer (examen manuel)
```

**Variance acceptable:** Les tests ne doivent pas être 5%+ plus lents en raison des modifications du framework.

---

## Politique de Dépréciation

### Comment Fonctionne la Dépréciation

1. **Annoncer:** Fonctionnalité marquée comme `[Obsolete]` dans le code
2. **Période de grâce:** Minimum 2 versions mineures avant suppression
3. **Documentation:** Guide de dépréciation publié avec alternatives
4. **Suppression:** Fonctionnalité supprimée après la période de grâce

### Calendrier de Dépréciation

**0.1.x :**
- `TestBuilder.WithTimeout()` - Déprécié, utiliser l'attribut `[Timeout]`

**0.2.x :**
- Marqué pour suppression dans 0.4.x

**0.4.x :**
- `TestBuilder.WithTimeout()` supprimé

### Trouver les API Dépréciées

Lorsque vous mettez à jour, votre IDE signalera le code déprécié :

```csharp
// ⚠️ Avertissement du compilateur CS0618 dans Visual Studio
public void OldMethod()
{
    builder.WithTimeout(5000); // 'WithTimeout' est obsolète
}
```

**Action:** Utiliser l'alternative suggérée (affichée dans l'infobulle).

---

## Procédures de Restauration

### Restauration Rapide (Même Jour)

Si la mise à jour cause des problèmes critiques :

```bash
# 1. Revenir à la branche précédente
git checkout main
git reset --hard <previous-commit-hash>

# 2. Réinstaller les packages
dotnet clean
dotnet package restore

# 3. Vérifier
dotnet build
dotnet test
```

### Restauration Documentée (Même Semaine)

Si vous avez découvert des problèmes après la validation :

```bash
# 1. Créer une branche de restauration
git checkout -b fix/rollback-from-0.2.0

# 2. Annuler le commit de mise à jour
git revert <upgrade-commit-hash>

# 3. Tester en détail
dotnet build
dotnet test

# 4. Fusionner et documenter
# ... créer PR avec explication
```

### Rétrogradation Manuelle du Package

Pour rétrograder un package spécifique :

```bash
# Rétrograder vers une version spécifique
dotnet package update PeasyPilot.Core --version 0.1.5

# Vérifier que tous les packages correspondent
dotnet package list PeasyPilot
```

---

## FAQ

### Q : Puis-je sauter des versions mineures ? (0.1.x → 0.3.x directement ?)

**R :** Généralement oui, mais non recommandé. Chaque version mineure documente les changements cassants. Si vous sautez 0.2.x :
1. Vous manquerez la documentation 0.2.x
2. Vous pouvez sauter les étapes de migration recommandées
3. Le dépannage devient plus difficile

**Recommandation:** Mettre à jour progressivement (0.1.5 → 0.2.0 → 0.3.0).

---

### Q : Mes tests existants vont-ils se casser ?

**R :** Cela dépend des API que vous utilisez :

| Scénario | Risque | Action |
|---|---|---|
| Tests unitaires uniquement, pas de BDD | Faible | La mise à jour rapide du patch devrait fonctionner |
| Utilisation intensive de BDD | Moyen | Examiner [Changements Cassants](#changements-cassants-par-version) |
| Extensions personnalisées | Élevé | Examiner en détail la documentation des interfaces |

**Approche sûre:** Toujours exécuter la suite de tests complète après la mise à jour.

---

### Q : Comment savoir si j'utilise des API dépréciées ?

**R :** Trois façons :

1. **Avertissements de compilation:** Exécuter `dotnet build` et chercher les avertissements CS0618
2. **Infobulles d'IDE:** Le code déprécié s'affiche avec une ligne dans VS/VS Code
3. **Documentation:** Consultez la section [Dépréciation](#politique-de-dépréciation) de ce guide

**Commande pour trouver tous :**
```bash
dotnet build --no-incremental 2>&1 | grep "CS0618"
```

---

### Q : Et si les tests passent mais le comportement a changé ?

**R :** C'est rare mais possible. Pour détecter les changements de comportement :

1. **Tests d'intégration:** Ceux-ci détectent la plupart des changements de comportement
2. **Tests E2E:** Exécuter sur les bases de données/services réels
3. **Test manuel:** Vérifier les chemins critiques
4. **Monitoring:** Surveiller les journaux d'application après le déploiement

**Si des changements de comportement sont détectés :**
- Signaler le problème sur [GitHub](https://github.com/hnidboubker/PeasyPilot/issues)
- Utiliser la procédure de restauration ci-dessus
- Fournir un cas de test reproduisant le problème

---

### Q : Combien de temps une version est-elle supportée ?

**R :** Calendrier de support :

| Version | Sortie | Dernière | Fin du Support |
|---|---|---|---|
| 0.1.x | 2026-09 | 0.1.5 | 2027-03 (6 mois) |
| 0.2.x | TBD | - | - |
| 1.0.x | TBD | - | LTS (3 ans) |

Les versions **LTS (Long Term Support)** reçoivent des corrections de sécurité au-delà de la sortie.

---

### Q : Puis-je utiliser PeasyPilot avec .NET 7 ou antérieur ?

**R :** Non. PeasyPilot cible .NET 8+, ce qui active les fonctionnalités C# modernes. La mise à jour de votre projet vers .NET 8 est requise.

---

### Q : Et les versions sur NuGet.org ?

**R :** Toutes les versions sont publiées sur [NuGet.org](https://www.nuget.org/packages/PeasyPilot.Core/) :

```bash
# Voir toutes les versions disponibles
dotnet package search PeasyPilot.Core --exact-match

# Installer une version spécifique
dotnet add package PeasyPilot.Core --version 0.1.5
```

---

## Obtenir de l'Aide

### Ressources

- **Documentation:** Dossier [docs/](../)
- **Exemples:** Dossier [samples/](../../samples/)
- **Problèmes:** [GitHub Issues](https://github.com/hnidboubker/PeasyPilot/issues)
- **Discussions:** [GitHub Discussions](https://github.com/hnidboubker/PeasyPilot/discussions)

### Signaler des Problèmes

Si vous rencontrez des problèmes lors de la mise à jour :

1. **Reproduire:** Créer un cas de test minimal montrant le problème
2. **Documenter:** Noter votre version, votre version .NET et les étapes à reproduire
3. **Signaler:** [Créer un problème GitHub](https://github.com/hnidboubker/PeasyPilot/issues/new) avec :
   - Version PeasyPilot (avant et après)
   - Version .NET
   - Message d'erreur/stacktrace
   - Exemple de code minimal

### Poser des Questions

- [GitHub Discussions](https://github.com/hnidboubker/PeasyPilot/discussions) pour les questions générales
- [GitHub Issues](https://github.com/hnidboubker/PeasyPilot/issues) pour les bugs uniquement

---

## Résumé

La mise à jour de PeasyPilot est simple :

1. **Sauvegarder** votre code (`git branch`)
2. **Mettre à jour** les packages (`dotnet package update`)
3. **Examiner** les changements cassants (si mineur/majeur)
4. **Tester** en détail (`dotnet test`)
5. **Déployer** en confiance

**Temps total pour une mise à jour typique:** 15-30 minutes

Pour obtenir de l'aide, reportez-vous à ce guide ou aux [discussions GitHub](https://github.com/hnidboubker/PeasyPilot/discussions).

Bonne mise à jour ! 🚀
