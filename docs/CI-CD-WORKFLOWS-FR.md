# Workflows CI/CD

Ce document décrit les workflows GitHub Actions utilisés dans PeasyPilot pour l'intégration continue et le déploiement.

## Aperçu

PeasyPilot utilise plusieurs workflows GitHub Actions pour assurer la qualité du code, la couverture des tests et les releases fiables.

| Workflow | Fichier | Déclencheur | Objectif |
|----------|---------|-------------|---------|
| Build and Test | `build-and-test.yml` | Push/PR sur main, develop, phase/* | Exécuter des tests complets sur plusieurs versions .NET |
| Coverage | `coverage.yml` | Push sur main | Générer et rapporter la couverture du code |
| Publish | `publish.yml` | Tag de release | Publier les packages sur NuGet |
| Release | `release.yml` | Tag de release | Créer les releases GitHub avec les notes |
| Test Failures | `test-failures-report.yml` | Fin de build-and-test | Rapporter les échecs de test comme issues GitHub |

## Workflows Détaillés

### Build and Test (`build-and-test.yml`) ⭐

**Workflow principal** - Valide chaque push et pull request.

**Déclencheurs :**
- Push vers les branches `main`, `develop`, ou `phase/**`
- Pull requests ciblant `main` ou `develop`

**Tests sur Matrice :**
- .NET 8.0
- .NET 9.0
- .NET 10.0

**Étapes :**
1. **Checkout** – Cloner le dépôt
2. **Setup .NET** – Installer la version .NET spécifiée
3. **Restore** – Restaurer les dépendances NuGet
4. **Build** – Compiler la solution (configuration Release)
   - `continue-on-error: true` – N'empêche pas l'étape de test si la compilation a des avertissements
5. **Run Tests** – Exécuter tous les tests avec enregistrement de la sortie console
   - Résultats capturés dans `test-output.log`
   - `continue-on-error: true` – Permet la capture des échecs pour le rapport
6. **Capture Test Output** – Extraire les 50 dernières lignes des résultats
7. **Check for Test Failures** – Analyser la sortie pour les échecs
   - Crée `.test-errors/test-failures.md` si des échecs détectés
8. **Upload Test Failures** – Archiver le rapport d'erreur comme artefact
9. **Display Failure Report** – Imprimer les échecs sur la console

**Job Secondaire - Logs :**
- S'exécute si le workflow se termine (succès ou échec)
- Télécharge les artefacts d'erreur de test
- Affiche le résumé de l'exécution CI/CD

### Coverage (`coverage.yml`)

**Déclencheurs :**
- Push vers la branche `main`

**Objectif :**
- Générer les rapports de couverture du code (format Cobertura XML)
- Télécharger les artefacts vers les services de couverture
- Suivre les tendances de couverture

### Publish (`publish.yml`)

**Déclencheurs :**
- Création d'une release GitHub avec tag

**Objectif :**
- Construire tous les packages en mode Release
- Pousser les fichiers `.nupkg` et `.snupkg` sur NuGet
- Ignorer les versions dupliquées

### Release (`release.yml`)

**Déclencheurs :**
- Création d'une release GitHub avec tag

**Objectif :**
- Générer automatiquement les notes de release à partir des commits
- Attacher les packages NuGet (`.nupkg`, `.snupkg`)
- Créer une release professionnelle sur GitHub

### Test Failures (`test-failures-report.yml`)

**Déclencheurs :**
- Quand `build-and-test.yml` se termine

**Objectif :**
- Analyser les artefacts d'erreur de test du job de build
- Créer des issues GitHub pour les échecs
- Étiqueter automatiquement les issues avec `test-failure` et `automated`

## Bonnes Pratiques

### Nommage des Branches
- `main` – Code prêt pour la production
- `develop` – Branche d'intégration pour les fonctionnalités
- `phase/**` – Branches de fonctionnalités avec tests automatisés
- Les branches de fonctionnalités ne déclenchent PAS de workflows sauf si elles commencent par `phase/`

### Pull Requests
- Toujours cibler `main` ou `develop`
- Les workflows s'exécutent automatiquement
- Doivent passer tous les vérifications avant fusion

### Tags et Releases
- Format du tag : `v*.*.*` (ex : `v1.2.3`)
- Pousser un tag → Les workflows Publish et Release se déclenchent
- Les packages sont publiés sur NuGet automatiquement

### Gestion des Erreurs
- Les erreurs de compilation ne bloquent PAS l'exécution des tests
  - Utiliser `continue-on-error: true` dans l'étape de build
  - Permet l'analyse des résultats de test même avec les avertissements
- Les erreurs de test ne bloquent PAS l'achèvement du workflow
  - Capturées et rapportées
  - Issues créées pour le suivi

## Surveillance

### Vérifier le Statut du Workflow
1. Aller à l'onglet **Actions** dans le dépôt GitHub
2. Cliquer sur le nom du workflow pour voir les exécutions récentes
3. Cliquer sur une exécution spécifique pour voir les logs

### Problèmes Courants

**La compilation échoue mais les tests s'exécutent :**
- C'est attendu avec `continue-on-error: true`
- Vérifier la sortie de compilation pour les avertissements/erreurs
- Les tests peuvent toujours réussir si les erreurs de compilation sont dans l'infrastructure de test

**Les tests expirent :**
- Vérifier les logs du runner Ubuntu (étape test)
- Peut indiquer une boucle infinie ou une opération async suspendue

**La couverture ne se met pas à jour :**
- Vérifier que `.github/workflows/coverage.yml` est correct
- Vérifier si le push cible la branche `main`

**La publication échoue :**
- Vérifier que le jeton API NuGet est valide
- Vérifier que les noms de fichiers `.nupkg` correspondent au format attendu
- S'assurer que la version n'est pas déjà publiée

## Maintenance

### Mise à Jour des Actions de Workflow
Les workflows utilisent des versions d'actions épinglées pour la reproductibilité :
```yaml
- uses: actions/checkout@v4
- uses: actions/setup-dotnet@v4
- uses: actions/upload-artifact@v4
```

Pour mettre à jour :
1. Vérifier la marketplace GitHub Actions pour la version la plus récente
2. Mettre à jour la version dans le fichier de workflow
3. Tester dans une branche de fonctionnalité avant fusion vers main

### Ajouter de Nouvelles Étapes
Lors de l'ajout d'étapes aux workflows :
1. Tester localement avec l'outil `act` (si possible)
2. Créer une branche de fonctionnalité pour les changements de workflow
3. Pousser vers une branche `phase/` pour déclencher une exécution de test
4. Vérifier que le workflow réussit avant fusion

## Exemple : Test Local avec `act`

Vous pouvez simuler des workflows localement en utilisant l'outil `act` :

```bash
# Installer act : https://github.com/nektos/act

# Exécuter le workflow build-and-test localement
act push -j build-and-test

# Exécuter un workflow spécifique
act -l  # Lister tous les workflows
```

## Secrets et Variables d'Environnement

### Secrets Requis
- `NUGET_API_KEY` – Pour publier sur NuGet (défini dans les paramètres du dépôt)

### Variables d'Environnement
Définies dans les fichiers de workflow :
```yaml
env:
  SOLUTION: easy-peasy.slnx
  CONFIGURATION: Release
```

## Améliorations Futures

- [ ] Ajouter un workflow de benchmarking de performance
- [ ] Ajouter une analyse de sécurité (SAST)
- [ ] Ajouter des vérifications de mise à jour des dépendances (Dependabot)
- [ ] Ajouter des vérifications de qualité du code (SonarQube)
- [ ] Ajouter un workflow de test de charge
