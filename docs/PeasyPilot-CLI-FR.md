# PeasyPilot.CLI

Exécuteur de tests en ligne de commande avec filtrage et programmation.

```bash
dotnet pesypilot run --filter "*.Tests" --schedule daily
dotnet pesypilot run --impact-analysis
```

## Fonctionnalités
- ✅ Filtrage par nom/catégorie
- ✅ Programmation
- ✅ Analyse d'impact
- ✅ Rapports JSON/JUnit
- ✅ Intégration CI/CD

## Utilisation

```bash
# Exécuter tous les tests
dotnet pesypilot run

# Filtrer les tests
dotnet pesypilot run --filter "*UserTests"

# Programmer les tests
dotnet pesypilot run --schedule "0 2 * * *"

# Analyse d'impact
dotnet pesypilot run --impact-analysis
```

## Installation
```bash
dotnet tool install PeasyPilot.CLI
```
