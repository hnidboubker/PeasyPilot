$ErrorActionPreference = 'Continue'
$rootDirectory = Split-Path -Parent $PSScriptRoot
$artifactsDirectory = Join-Path $rootDirectory 'artifacts'
$solutionPath = Join-Path $rootDirectory 'easy-peasy.slnx'
$extensions = @('.cs', '.csproj', '.props', '.targets', '.slnx', '.json', '.md', '.png', '.ps1', '.sh')

# Find dotnet executable
$dotnetPath = $null
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnetCommand) {
    $dotnetPath = $dotnetCommand.Source
} else {
    $candidates = @(
        (Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'),
        'dotnet'
    )
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate -ErrorAction SilentlyContinue) {
            $dotnetPath = $candidate
            break
        }
    }
}

if (-not $dotnetPath) {
    Write-Host '[ERROR] dotnet not found. Install .NET SDK or add to PATH.' -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] dotnet: $dotnetPath" -ForegroundColor Cyan
Write-Host "[INFO] solution: $solutionPath" -ForegroundColor Cyan
Write-Host "[INFO] artifacts: $artifactsDirectory" -ForegroundColor Cyan
Write-Host ""

function Test-RelevantChange {
    param([string]$Path)

    # Ignore system/build files
    if ($Path -match '[\\/](\.git|\.vs|artifacts|bin|obj|packages|\.idea)([\\/]|$)') {
        return $false
    }

    # Check extension
    if ($extensions -notcontains [System.IO.Path]::GetExtension($Path)) {
        return $false
    }

    return $true
}

function Invoke-BuildAndPack {
    Write-Host "`n[BUILD] Starting build and pack..." -ForegroundColor Green

    $startTime = Get-Date

    # Build
    Write-Host "[BUILD] Executing: $dotnetPath build `"$solutionPath`" -c Release" -ForegroundColor Cyan
    & $dotnetPath build "$solutionPath" -c Release

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Build failed (exit code: $LASTEXITCODE)" -ForegroundColor Red
        Write-Host "[INFO] Watching for changes..." -ForegroundColor Yellow
        return $false
    }

    # Prepare artifacts
    if (Test-Path $artifactsDirectory) {
        Write-Host "[CLEAN] Removing existing artifacts..." -ForegroundColor Cyan
        Remove-Item $artifactsDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }

    New-Item $artifactsDirectory -ItemType Directory -Force | Out-Null
    Write-Host "[PACK] Executing: $dotnetPath pack `"$solutionPath`" -c Release -o `"$artifactsDirectory`"" -ForegroundColor Cyan

    # Pack
    & $dotnetPath pack "$solutionPath" -c Release -o "$artifactsDirectory" --no-build

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Pack failed (exit code: $LASTEXITCODE)" -ForegroundColor Red
        Write-Host "[INFO] Watching for changes..." -ForegroundColor Yellow
        return $false
    }

    # List packages
    Write-Host "`n[SUCCESS] Packages created:" -ForegroundColor Green
    $packages = @(Get-ChildItem $artifactsDirectory -File -Include '*.nupkg', '*.snupkg' -ErrorAction SilentlyContinue)

    if ($packages.Count -eq 0) {
        Write-Host "[WARNING] No packages found in artifacts directory" -ForegroundColor Yellow
    } else {
        foreach ($pkg in $packages) {
            Write-Host "  ✓ $($pkg.Name) ($([Math]::Round($pkg.Length / 1KB, 2)) KB)" -ForegroundColor Green
        }
    }

    $duration = (Get-Date) - $startTime
    Write-Host "`n[SUCCESS] Build and pack completed in $($duration.TotalSeconds)s" -ForegroundColor Green
    Write-Host "[INFO] Watching for changes..." -ForegroundColor Yellow

    return $true
}

$watcher = [System.IO.FileSystemWatcher]::new($rootDirectory)
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]'FileName, LastWrite, DirectoryName'
$watcher.EnableRaisingEvents = $true
$sourceIdentifiers = 'PeasyPilot.VersionWatcher.Changed', 'PeasyPilot.VersionWatcher.Created', 'PeasyPilot.VersionWatcher.Deleted', 'PeasyPilot.VersionWatcher.Renamed'
$subscriptions = @(
    Register-ObjectEvent -InputObject $watcher -EventName Changed -SourceIdentifier $sourceIdentifiers[0]
    Register-ObjectEvent -InputObject $watcher -EventName Created -SourceIdentifier $sourceIdentifiers[1]
    Register-ObjectEvent -InputObject $watcher -EventName Deleted -SourceIdentifier $sourceIdentifiers[2]
    Register-ObjectEvent -InputObject $watcher -EventName Renamed -SourceIdentifier $sourceIdentifiers[3]
)

try {
Write-Host "Surveillance de $rootDirectory (Ctrl+C pour arrêter)"
Invoke-BuildAndPack

while ($true) {
    $changeEvent = Wait-Event -SubscriptionId $subscriptions.Id
    $changedPath = $changeEvent.SourceEventArgs.FullPath
    Remove-Event -EventIdentifier $changeEvent.EventIdentifier

    if (Test-RelevantChange $changedPath) {
        Write-Host 'Modification détectée. Relance du build et du package...'
        Invoke-BuildAndPack
        Get-Event | Where-Object { $sourceIdentifiers -contains $_.SourceIdentifier } | Remove-Event
    }
}
}
finally {
    $subscriptions | Unregister-Event
    $watcher.Dispose()
}