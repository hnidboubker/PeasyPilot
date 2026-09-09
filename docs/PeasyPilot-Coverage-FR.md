# PeasyPilot.Coverage

Support des rapports et analyses de couverture.

```csharp
using PeasyPilot.Coverage;

var analyzer = new CoverageAnalyzer();
var report = await analyzer.AnalyzeAsync("bin/Release");

Console.WriteLine($"Couverture ligne: {report.LineCoverage:P}");
Console.WriteLine($"Couverture branche: {report.BranchCoverage:P}");
```

## Fonctionnalités
- ✅ Suivi couverture ligne
- ✅ Analyse couverture branche
- ✅ Couverture méthode
- ✅ Génération rapport HTML
- ✅ Intégration CI/CD

## Installation
```bash
dotnet add package PeasyPilot.Coverage
```

**Intégration :** Fonctionne avec OpenCover, Coverlet
