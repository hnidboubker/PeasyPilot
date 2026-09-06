param(
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'

$excludedDirectories = @('.git', 'artifacts', 'bin', 'obj', '.vs', 'packages', '.idea')
$normalizedRoot = [System.IO.Path]::GetFullPath($RootPath)

Write-Host "[INFO] Searching for .csproj in: $normalizedRoot" -ForegroundColor Cyan
Write-Host ""

$projects = @(Get-ChildItem -Path $normalizedRoot -File -Recurse -Filter '*.csproj' -ErrorAction SilentlyContinue |
    Where-Object {
        $relativePath = $_.FullName.Substring($normalizedRoot.Length).TrimStart('\', '/')
        $segments = $relativePath -split '[\\/]'
        
        # Vérifier qu'aucun segment ne correspond aux répertoires exclus
        -not ($segments | Where-Object { $_ -in $excludedDirectories })
    })

if ($projects.Count -eq 0) {
    Write-Host "[ERROR] No .csproj files found in $normalizedRoot" -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Found $($projects.Count) project(s):" -ForegroundColor Cyan
$projects | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""

# ============================================================
# PROCESS VERSIONS
# ============================================================

$successCount = 0
$failureCount = 0

foreach ($project in $projects) {
    try {
        [xml]$csproj = Get-Content -Path $project.FullName -Raw
        $versionNode = $csproj.SelectSingleNode("//*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='Version']")

        if ($null -eq $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
            Write-Host "$($project.Name) -> No Version found, skipped" -ForegroundColor Yellow
            continue
        }

        $oldVersionText = $versionNode.InnerText.Trim()
        
        try {
            $oldVersion = [Version]::Parse($oldVersionText)
            $newVersion = [Version]::new($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build + 1)
        }
        catch {
            Write-Host "$($project.Name) -> Invalid version format '$oldVersionText', skipped" -ForegroundColor Yellow
            continue
        }

        $versionNode.InnerText = $newVersion.ToString()
        $csproj.Save($project.FullName)

        Write-Host "$($project.Name): $oldVersionText → $newVersion" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "$($project.Name): ERROR - $($_.Exception.Message)" -ForegroundColor Red
        $failureCount++
    }
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host "[SUCCESS] $successCount project(s) updated" -ForegroundColor Green
if ($failureCount -gt 0) {
    Write-Host "[ERROR] $failureCount project(s) failed" -ForegroundColor Red
}
