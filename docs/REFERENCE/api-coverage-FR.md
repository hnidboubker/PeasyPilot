# Référence API PeasyPilot.Coverage

## Aperçu

`PeasyPilot.Coverage` fournit des abstractions de reporting de couverture de code et des outils d'analyse pour suivre les métriques de couverture de test. Il offre une interface indépendante du framework pour collecter, signaler et analyser les données de couverture de code à partir des exécutions de test, permettant une visibilité sur quelles parties de votre base de code sont exercées par vos tests.

**Responsabilités principales :**
- Abstraction de reporting de couverture via ICoverageProvider
- Suivi de la couverture de ligne et de la couverture de branche
- Calcul du pourcentage de couverture
- Intégration de couverture indépendante du framework
- Intégration transparente du conteneur DI
- Métriques de couverture pour l'orchestration et le reporting
- Support pour l'évaluation de la qualité des tests basée sur la couverture

**Cibles :** .NET 8.0, 9.0, 10.0

**Dépendances Optionnelles :** OpenCover, CodeCoverage (outils externes)

---

## Abstractions Principales

### ICoverageProvider

Interface pour collecter les informations de couverture de code à partir des exécutions de test.

```csharp
namespace PeasyPilot.Coverage;

/// <summary>
/// Fournisseur pour collecter les informations de couverture de code.
/// </summary>
public interface ICoverageProvider
{
    /// <summary>
    /// Obtient le rapport de couverture de manière asynchrone.
    /// </summary>
    /// <returns>Le rapport de couverture.</returns>
    Task<CoverageReport> GetCoverageAsync();
}
```

**Objectif :** Fournit une abstraction indépendante du framework pour collecter les métriques de couverture, permettant les implémentations enfichables qui fonctionnent avec différents outils de couverture (OpenCover, CodeCoverage, etc.).

---

## Modèles Principaux

### CoverageReport

Métriques complètes de couverture de code pour une exécution de test.

```csharp
namespace PeasyPilot.Coverage;

/// <summary>
/// Informations de couverture de code pour une exécution de test.
/// </summary>
public class CoverageReport
{
    /// <summary>
    /// Obtient ou définit le nombre total de lignes couvertes.
    /// </summary>
    public int LinesCovered { get; set; }

    /// <summary>
    /// Obtient ou définit le nombre total de lignes de code.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Obtient ou définit le nombre total de branches couvertes.
    /// </summary>
    public int BranchesCovered { get; set; }

    /// <summary>
    /// Obtient ou définit le nombre total de branches.
    /// </summary>
    public int TotalBranches { get; set; }

    /// <summary>
    /// Obtient le pourcentage de couverture de ligne (0-100).
    /// </summary>
    public double LineCoveragePercentage
    {
        get => TotalLines > 0 ? (LinesCovered * 100.0) / TotalLines : 0;
    }

    /// <summary>
    /// Obtient le pourcentage de couverture de branche (0-100).
    /// </summary>
    public double BranchCoveragePercentage
    {
        get => TotalBranches > 0 ? (BranchesCovered * 100.0) / TotalBranches : 0;
    }

    /// <summary>
    /// Obtient une représentation en chaîne du rapport de couverture.
    /// </summary>
    public override string ToString()
    {
        return $"Coverage Report\n" +
               $"  Line Coverage: {LineCoveragePercentage:F2}% ({LinesCovered}/{TotalLines})\n" +
               $"  Branch Coverage: {BranchCoveragePercentage:F2}% ({BranchesCovered}/{TotalBranches})";
    }
}
```

**Objectif :** Encapsule les métriques de couverture avec les propriétés de pourcentage calculées pour un reporting et des assertions faciles.

---

## Métriques de Couverture

### Couverture de Ligne

Mesure le pourcentage de lignes exécutables qui ont été exécutées pendant les exécutions de test :

```
Couverture de Ligne % = (Lignes Couvertes / Lignes Totales) × 100
```

**Interprétation :**
- **90-100% :** Couverture excellente (la plupart du code est exercée)
- **70-89% :** Bonne couverture (la plupart des chemins sont testés)
- **50-69% :** Couverture acceptable (les chemins basiques sont testés)
- **<50% :** Couverture faible (lacunes significatives)

### Couverture de Branche

Mesure le pourcentage de branches de code (conditionnelles) qui ont été exécutées :

```
Couverture de Branche % = (Branches Couvertes / Branches Totales) × 100
```

**Interprétation :**
- **90-100% :** Tous les chemins de code sont testés
- **70-89% :** La plupart des chemins conditionnels sont couverts
- **50-69% :** Certains chemins ne sont pas couverts
- **<50% :** Des lacunes de chemins significatives existent

---

## Exemples Concrets

### Exemple 1 : Création Basique de Rapport de Couverture

```csharp
using PeasyPilot.Coverage;

public class BasicCoverageReportTest
{
    [Fact]
    public void TestCreateCoverageReport()
    {
        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.Equal(450, report.LinesCovered);
        Assert.Equal(500, report.TotalLines);
        Assert.Equal(90.0, report.LineCoveragePercentage);
        Assert.Equal(87.5, report.BranchCoveragePercentage);
    }

    [Fact]
    public void TestCoveragePercentageCalculation()
    {
        var report = new CoverageReport
        {
            LinesCovered = 75,
            TotalLines = 100,
            BranchesCovered = 15,
            TotalBranches = 20
        };

        Assert.Equal(75.0, report.LineCoveragePercentage);
        Assert.Equal(75.0, report.BranchCoveragePercentage);
    }

    [Fact]
    public void TestZeroCoverageEdgeCase()
    {
        var report = new CoverageReport
        {
            LinesCovered = 0,
            TotalLines = 0,
            BranchesCovered = 0,
            TotalBranches = 0
        };

        Assert.Equal(0.0, report.LineCoveragePercentage);
        Assert.Equal(0.0, report.BranchCoveragePercentage);
    }
}
```

### Exemple 2 : Formatage du Rapport de Couverture

```csharp
using PeasyPilot.Coverage;

public class CoverageReportFormattingTest
{
    [Fact]
    public void TestCoverageReportToString()
    {
        var report = new CoverageReport
        {
            LinesCovered = 420,
            TotalLines = 500,
            BranchesCovered = 32,
            TotalBranches = 40
        };

        var reportString = report.ToString();

        Assert.Contains("Coverage Report", reportString);
        Assert.Contains("Line Coverage: 84.00%", reportString);
        Assert.Contains("420/500", reportString);
        Assert.Contains("Branch Coverage: 80.00%", reportString);
        Assert.Contains("32/40", reportString);
    }

    [Fact]
    public void TestMultipleCoverageReports()
    {
        var reports = new[]
        {
            new CoverageReport
            {
                LinesCovered = 450,
                TotalLines = 500,
                BranchesCovered = 35,
                TotalBranches = 40
            },
            new CoverageReport
            {
                LinesCovered = 300,
                TotalLines = 400,
                BranchesCovered = 28,
                TotalBranches = 32
            }
        };

        foreach (var report in reports)
        {
            Console.WriteLine(report.ToString());
            Assert.True(report.LineCoveragePercentage >= 0);
            Assert.True(report.LineCoveragePercentage <= 100);
        }
    }
}
```

### Exemple 3 : Validation de Seuil de Couverture

```csharp
using PeasyPilot.Coverage;

public class CoverageThresholdTest
{
    [Fact]
    public void TestValidateCoverageThreshold()
    {
        var minimumLinesCoverageThreshold = 80.0;
        var minimumBranchCoverageThreshold = 75.0;

        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.True(report.LineCoveragePercentage >= minimumLinesCoverageThreshold,
            $"Line coverage {report.LineCoveragePercentage}% is below threshold {minimumLinesCoverageThreshold}%");

        Assert.True(report.BranchCoveragePercentage >= minimumBranchCoverageThreshold,
            $"Branch coverage {report.BranchCoveragePercentage}% is below threshold {minimumBranchCoverageThreshold}%");
    }

    [Fact]
    public void TestFailsCoverageThreshold()
    {
        var minimumCoverageThreshold = 90.0;

        var report = new CoverageReport
        {
            LinesCovered = 75,
            TotalLines = 100,
            BranchesCovered = 10,
            TotalBranches = 20
        };

        Assert.False(report.LineCoveragePercentage >= minimumCoverageThreshold,
            "Coverage should fail the threshold");
    }
}
```

### Exemple 4 : Suivi de la Progression de Couverture

```csharp
using PeasyPilot.Coverage;

public class CoverageProgressTrackingTest
{
    [Fact]
    public void TestTrackCoverageImprovement()
    {
        var reports = new[]
        {
            new CoverageReport
            {
                LinesCovered = 50,
                TotalLines = 200,
                BranchesCovered = 10,
                TotalBranches = 40
            },
            new CoverageReport
            {
                LinesCovered = 120,
                TotalLines = 200,
                BranchesCovered = 28,
                TotalBranches = 40
            },
            new CoverageReport
            {
                LinesCovered = 180,
                TotalLines = 200,
                BranchesCovered = 38,
                TotalBranches = 40
            }
        };

        var lineCoverageProgress = reports
            .Select(r => r.LineCoveragePercentage)
            .ToList();

        Assert.Equal(25.0, lineCoverageProgress[0]);
        Assert.Equal(60.0, lineCoverageProgress[1]);
        Assert.Equal(90.0, lineCoverageProgress[2]);

        // Verify improvement trend
        for (int i = 1; i < lineCoverageProgress.Count; i++)
        {
            Assert.True(lineCoverageProgress[i] >= lineCoverageProgress[i - 1],
                "Coverage should not decrease");
        }
    }
}
```

### Exemple 5 : Intégration avec le Conteneur DI

```csharp
using PeasyPilot.Coverage;
using Microsoft.Extensions.DependencyInjection;

public class CoverageProviderDiTest
{
    [Fact]
    public async Task TestCoverageProviderWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICoverageProvider, MockCoverageProvider>();
        var serviceProvider = services.BuildServiceProvider();

        var coverageProvider = serviceProvider.GetRequiredService<ICoverageProvider>();
        var report = await coverageProvider.GetCoverageAsync();

        Assert.NotNull(report);
        Assert.True(report.TotalLines > 0);
    }
}

public class MockCoverageProvider : ICoverageProvider
{
    public Task<CoverageReport> GetCoverageAsync()
    {
        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        return Task.FromResult(report);
    }
}
```

### Exemple 6 : Comparaison de Rapports de Couverture

```csharp
using PeasyPilot.Coverage;

public class CoverageComparisonTest
{
    [Fact]
    public void TestCompareCoverageReports()
    {
        var reportBefore = new CoverageReport
        {
            LinesCovered = 300,
            TotalLines = 500,
            BranchesCovered = 20,
            TotalBranches = 40
        };

        var reportAfter = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        var lineCoverageImprovement = 
            reportAfter.LineCoveragePercentage - reportBefore.LineCoveragePercentage;
        var branchCoverageImprovement = 
            reportAfter.BranchCoveragePercentage - reportBefore.BranchCoveragePercentage;

        Assert.Equal(30.0, lineCoverageImprovement);
        Assert.Equal(37.5, branchCoverageImprovement);
        Assert.True(lineCoverageImprovement > 0, "Coverage should improve");
    }
}
```

### Exemple 7 : Agrégation de Rapports de Couverture

```csharp
using PeasyPilot.Coverage;
using System.Collections.Generic;

public class CoverageAggregationTest
{
    [Fact]
    public void TestAggregateCoverageReports()
    {
        var reports = new[]
        {
            new CoverageReport
            {
                LinesCovered = 100,
                TotalLines = 200,
                BranchesCovered = 10,
                TotalBranches = 20
            },
            new CoverageReport
            {
                LinesCovered = 150,
                TotalLines = 200,
                BranchesCovered = 15,
                TotalBranches = 20
            },
            new CoverageReport
            {
                LinesCovered = 200,
                TotalLines = 200,
                BranchesCovered = 20,
                TotalBranches = 20
            }
        };

        var aggregatedReport = new CoverageReport
        {
            LinesCovered = reports.Sum(r => r.LinesCovered),
            TotalLines = reports.Sum(r => r.TotalLines),
            BranchesCovered = reports.Sum(r => r.BranchesCovered),
            TotalBranches = reports.Sum(r => r.TotalBranches)
        };

        Assert.Equal(450, aggregatedReport.LinesCovered);
        Assert.Equal(600, aggregatedReport.TotalLines);
        Assert.Equal(75.0, aggregatedReport.LineCoveragePercentage);
    }
}
```

### Exemple 8 : Notation de Qualité des Tests Basée sur la Couverture

```csharp
using PeasyPilot.Coverage;

public class CoverageQualityScoringTest
{
    private double CalculateQualityScore(CoverageReport report)
    {
        // Score based on both line and branch coverage
        const double lineWeightage = 0.6;
        const double branchWeightage = 0.4;

        var score = (report.LineCoveragePercentage * lineWeightage) +
                    (report.BranchCoveragePercentage * branchWeightage);

        return Math.Round(score, 2);
    }

    [Fact]
    public void TestQualityScoring()
    {
        var excellentReport = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        var goodReport = new CoverageReport
        {
            LinesCovered = 350,
            TotalLines = 500,
            BranchesCovered = 28,
            TotalBranches = 40
        };

        var poorReport = new CoverageReport
        {
            LinesCovered = 150,
            TotalLines = 500,
            BranchesCovered = 10,
            TotalBranches = 40
        };

        Assert.True(CalculateQualityScore(excellentReport) > 
                   CalculateQualityScore(goodReport));
        Assert.True(CalculateQualityScore(goodReport) > 
                   CalculateQualityScore(poorReport));
    }
}
```

### Exemple 9 : Validation de Rapport de Couverture

```csharp
using PeasyPilot.Coverage;

public class CoverageValidationTest
{
    private bool IsValidCoverageReport(CoverageReport report)
    {
        // Validate logical constraints
        if (report.LinesCovered < 0 || report.TotalLines < 0)
            return false;

        if (report.BranchesCovered < 0 || report.TotalBranches < 0)
            return false;

        if (report.LinesCovered > report.TotalLines)
            return false;

        if (report.BranchesCovered > report.TotalBranches)
            return false;

        return true;
    }

    [Fact]
    public void TestValidCoverageReport()
    {
        var validReport = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.True(IsValidCoverageReport(validReport));
    }

    [Fact]
    public void TestInvalidCoverageReport()
    {
        var invalidReport = new CoverageReport
        {
            LinesCovered = 600,  // More than total
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.False(IsValidCoverageReport(invalidReport));
    }
}
```

### Exemple 10 : Sérialisation JSON de Rapport de Couverture

```csharp
using PeasyPilot.Coverage;
using System.Text.Json;

public class CoverageJsonSerializationTest
{
    [Fact]
    public void TestSerializeCoverageReport()
    {
        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Assert.Contains("\"linesCovered\"", json);
        Assert.Contains("\"totalLines\"", json);
        Assert.Contains("450", json);
        Assert.Contains("500", json);
    }

    [Fact]
    public void TestDeserializeCoverageReport()
    {
        var json = @"{
            ""linesCovered"": 450,
            ""totalLines"": 500,
            ""branchesCovered"": 35,
            ""totalBranches"": 40
        }";

        var report = JsonSerializer.Deserialize<CoverageReport>(json);

        Assert.NotNull(report);
        Assert.Equal(450, report.LinesCovered);
        Assert.Equal(500, report.TotalLines);
        Assert.Equal(90.0, report.LineCoveragePercentage);
    }
}
```

---

## Interprétation des Métriques de Couverture

### Plages de Couverture et Recommandations

| Couverture % | Évaluation | Recommandation |
|-------------|-----------|-----------------|
| 90-100% | Excellente | Prête pour la production, maintenir la qualité |
| 80-89% | Très Bien | Lacunes mineures, acceptable pour la publication |
| 70-79% | Bien | Lacunes notables, améliorer avant la publication |
| 60-69% | Acceptable | Lacunes significatives, tests nécessaires |
| <60% | Faible | Lacunes majeures, travail substantiel nécessaire |

### Analyse des Lacunes de Couverture

Quand la couverture est en dessous du objectif :
1. Identifier les lignes et branches non couvertes
2. Déterminer s'ils sont des chemins critiques ou des cas limite
3. Ajouter des tests pour les chemins critiques
4. Documenter pourquoi les cas limite ne sont pas couverts
5. Suivre les tendances de couverture au fil du temps

---

## Motifs d'Intégration

### Constructions Gated par Couverture

Échouer les constructions si la couverture tombe en dessous du seuil :

```csharp
public async Task ValidateCoverageTresholdAsync(
    ICoverageProvider provider,
    double minimumCoverage)
{
    var report = await provider.GetCoverageAsync();
    
    if (report.LineCoveragePercentage < minimumCoverage)
    {
        throw new Exception(
            $"Coverage {report.LineCoveragePercentage}% is below " +
            $"minimum {minimumCoverage}%");
    }
}
```

### Reporting de Couverture

Exporter les métriques de couverture pour les tableaux de bord :

```csharp
public string GenerateCoverageReport(CoverageReport report)
{
    return $@"
    # Coverage Report
    
    - **Line Coverage:** {report.LineCoveragePercentage:F2}% ({report.LinesCovered}/{report.TotalLines})
    - **Branch Coverage:** {report.BranchCoveragePercentage:F2}% ({report.BranchesCovered}/{report.TotalBranches})
    - **Status:** {GetCoverageStatus(report)}
    ";
}
```

---

## Considérations de Performances

- **Calcul des Métriques :** O(1) - calculs de pourcentage instantanés
- **Création de Rapport :** Surcharge minimale pour l'agrégation
- **Mémoire :** Structure de données légère (4 entiers seulement)
- **Évolutivité :** Gère les projets de toute taille sans dégradation de performance

---

## Voir Aussi

- [API PeasyPilot.Core](api-core-FR.md)
- [Guide de Génération de Test](../GUIDES/test-generation-guide-FR.md)
- [Guide de Test Unitaire](../GUIDES/unit-testing-guide-FR.md)
- [Documentation OpenCover](https://github.com/OpenCover/opencover)

