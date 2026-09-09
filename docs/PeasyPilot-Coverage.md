# PeasyPilot.Coverage

Coverage reporting and analysis support.

```csharp
using PeasyPilot.Coverage;

var analyzer = new CoverageAnalyzer();
var report = await analyzer.AnalyzeAsync("bin/Release");

Console.WriteLine($"Line coverage: {report.LineCoverage:P}");
Console.WriteLine($"Branch coverage: {report.BranchCoverage:P}");
```

## Features
- ✅ Line coverage tracking
- ✅ Branch coverage analysis
- ✅ Method coverage
- ✅ HTML report generation
- ✅ CI/CD integration

## Install
```bash
dotnet add package PeasyPilot.Coverage
```

**Integration:** Works with OpenCover, Coverlet
