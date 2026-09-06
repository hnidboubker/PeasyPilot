param(
[string]$ProjectPath = ".",
[ValidateSet("Debug", "Release")]
[string]$Configuration = "Release",
[switch]$Watch
)

$ErrorActionPreference = "Stop"

============================================================
Configuration
============================================================

$extensions = @(
'.cs',
'.csproj',
'.props',
'.targets',
'.sln',
'.slnx',
'.json',
'.md',
'.ps1',
'.sh'
)

Résout le dossier du projet

$rootDirectory = (Resolve-Path $ProjectPath).Path

============================================================
Helpers
============================================================

function Write-Info {
param([string]$Message)
Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Success {
param([string]$Message)
Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-ErrorMessage {
param([string]$Message)
Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-WarningMessage {
param([string]$Message)
Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

============================================================
Find dotnet
============================================================

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue

if (-not $dotnetCommand) {
Write-ErrorMessage "dotnet not found."
Write-Host "Install the .NET SDK or add dotnet to PATH." -ForegroundColor Yellow
exit 1
}

$dotnetPath = $dotnetCommand.Source

Write-Info "dotnet: $dotnetPath"
Write-Info "project directory: $rootDirectory"
Write-Info "configuration: $Configuration"
Write-Host ""

============================================================
Find solution / project automatically
============================================================

$solutionFiles = @(
Get-ChildItem -Path $rootDirectory -Filter ".slnx" -File -ErrorAction SilentlyContinue
Get-ChildItem -Path $rootDirectory -Filter ".sln" -File -ErrorAction SilentlyContinue
)

$projectFiles = @(
Get-ChildItem -Path $rootDirectory -Filter "*.csproj" -File -ErrorAction SilentlyContinue
)

$buildTarget = $null

if ($solutionFiles.Count -gt 0) {

# Priorité au .slnx
$slnx = $solutionFiles | Where-Object { $_.Extension -eq ".slnx" } | Select-Object -First 1

if ($slnx) {
    $buildTarget = $slnx.FullName
}
else {
    $buildTarget = ($solutionFiles | Select-Object -First 1).FullName
}


}
elseif ($projectFiles.Count -eq 1) {

$buildTarget = $projectFiles[0].FullName


}
elseif ($projectFiles.Count -gt 1) {

Write-WarningMessage "Multiple .csproj files found."

Write-Host ""
Write-Host "Projects found:" -ForegroundColor Yellow

for ($i = 0; $i -lt $projectFiles.Count; $i++) {
    Write-Host "  [$i] $($projectFiles[$i].Name)"
}

Write-Host ""

$selection = Read-Host "Select project number"

if ($selection -notmatch '^\d+$' -or
    [int]$selection -ge $projectFiles.Count) {

    Write-ErrorMessage "Invalid project selection."
    exit 1
}

$buildTarget = $projectFiles[[int]$selection].FullName


}
else {

Write-ErrorMessage "No .sln, .slnx or .csproj found in:"
Write-Host "  $rootDirectory" -ForegroundColor Yellow
exit 1


}

Write-Info "Build target: $buildTarget"
Write-Host ""

============================================================
Artifacts
============================================================

$artifactsDirectory = Join-Path $rootDirectory "artifacts"

============================================================
Test relevant changes
============================================================

function Test-RelevantChange {
param(
[string]$Path
)

if (-not $Path) {
    return $false
}

# Ignore build/system folders
if ($Path -match '[\\/](\.git|\.vs|artifacts|bin|obj|packages|\.idea)([\\/]|$)') {
    return $false
}

$extension = [System.IO.Path]::GetExtension($Path)

return $extensions -contains $extension


}

============================================================
Build + Pack
============================================================

function Invoke-BuildAndPack {

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " BUILD" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray

$startTime = Get-Date

try {

    # --------------------------------------------------------
    # Build
    # --------------------------------------------------------

    Write-Info "Running dotnet build..."

    & $dotnetPath build `
        "$buildTarget" `
        "-c" `
        "$Configuration"

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE."
    }

    Write-Success "Build completed successfully."

    # --------------------------------------------------------
    # Clean artifacts
    # --------------------------------------------------------

    if (Test-Path $artifactsDirectory) {

        Write-Info "Cleaning artifacts..."

        Remove-Item `
            $artifactsDirectory `
            -Recurse `
            -Force
    }

    New-Item `
        -Path $artifactsDirectory `
        -ItemType Directory `
        -Force |
        Out-Null

    # --------------------------------------------------------
    # Pack
    # --------------------------------------------------------

    Write-Info "Running dotnet pack..."

    & $dotnetPath pack `
        "$buildTarget" `
        "-c" `
        "$Configuration" `
        "-o" `
        "$artifactsDirectory" `
        "--no-build"

    if ($LASTEXITCODE -ne 0) {
        throw "Pack failed with exit code $LASTEXITCODE."
    }

    Write-Success "Pack completed successfully."

    # --------------------------------------------------------
    # Packages
    # --------------------------------------------------------

    $packages = @(
        Get-ChildItem `
            -Path $artifactsDirectory `
            -File `
            -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Extension -in @(".nupkg", ".snupkg")
        }
    )

    if ($packages.Count -eq 0) {

        Write-WarningMessage "No NuGet packages were created."

    }
    else {

        Write-Host ""
        Write-Host "Packages:" -ForegroundColor Green

        foreach ($package in $packages) {

            $size = [Math]::Round(
                $package.Length / 1KB,
                2
            )

            Write-Host "  ✓ $($package.Name) ($size KB)" `
                -ForegroundColor Green
        }
    }

    $duration = (Get-Date) - $startTime

    Write-Host ""
    Write-Success "BUILD + PACK SUCCESS"
    Write-Info "Duration: $([Math]::Round($duration.TotalSeconds, 2)) seconds"
    Write-Info "Artifacts: $artifactsDirectory"

    return $true
}
catch {

    Write-Host ""
    Write-ErrorMessage $_.Exception.Message
    Write-ErrorMessage "BUILD + PACK FAILED"

    return $false
}


}

============================================================
Initial build
============================================================

$success = Invoke-BuildAndPack

============================================================
Stop immediately if Watch is not enabled
============================================================

if (-not $Watch) {

Write-Host ""

if ($success) {
    Write-Success "Process completed successfully."
    exit 0
}
else {
    Write-ErrorMessage "Process stopped because of an error."
    exit 1
}


}

============================================================
WATCH MODE
============================================================

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " WATCH MODE" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray

if (-not $success) {
Write-WarningMessage "Initial build failed. Waiting for changes..."
}

Write-Info "Watching: $rootDirectory"
Write-Info "Press Ctrl+C to stop."

$watcher = [System.IO.FileSystemWatcher]::new($rootDirectory)

$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]'FileName, LastWrite, DirectoryName'
$watcher.EnableRaisingEvents = $true

$changeMarker = Join-Path $env:TEMP "dotnet-generic-build-change"

$sourceIdentifiers = @(
"GenericDotnetWatcher.Changed",
"GenericDotnetWatcher.Created",
"GenericDotnetWatcher.Deleted",
"GenericDotnetWatcher.Renamed"
)

try {

$subscriptions = @(
    Register-ObjectEvent `
        -InputObject $watcher `
        -EventName Changed `
        -SourceIdentifier $sourceIdentifiers[0] `
        -Action {
            if (Test-RelevantChange $EventArgs.FullPath) {
                Set-Content `
                    -Path $changeMarker `
                    -Value (Get-Date)
            }
        }

    Register-ObjectEvent `
        -InputObject $watcher `
        -EventName Created `
        -SourceIdentifier $sourceIdentifiers[1] `
        -Action {
            if (Test-RelevantChange $EventArgs.FullPath) {
                Set-Content `
                    -Path $changeMarker `
                    -Value (Get-Date)
            }
        }

    Register-ObjectEvent `
        -InputObject $watcher `
        -EventName Deleted `
        -SourceIdentifier $sourceIdentifiers[2] `
        -Action {
            if (Test-RelevantChange $EventArgs.FullPath) {
                Set-Content `
                    -Path $changeMarker `
                    -Value (Get-Date)
            }
        }

    Register-ObjectEvent `
        -InputObject $watcher `
        -EventName Renamed `
        -SourceIdentifier $sourceIdentifiers[3] `
        -Action {
            if (Test-RelevantChange $EventArgs.FullPath) {
                Set-Content `
                    -Path $changeMarker `
                    -Value (Get-Date)
            }
        }
)

while ($true) {

    if (Test-Path $changeMarker) {

        Remove-Item `
            $changeMarker `
            -Force `
            -ErrorAction SilentlyContinue

        # Debounce
        Start-Sleep -Milliseconds 1000

        Write-Host ""
        Write-Info "Change detected. Rebuilding..."

        $success = Invoke-BuildAndPack

        if ($success) {

            Write-Host ""
            Write-Success "READY - waiting for next change..."

        }
        else {

            Write-Host ""
            Write-ErrorMessage "BUILD FAILED - waiting for next change..."
        }

        Get-Event `
            -SourceIdentifier $sourceIdentifiers `
            -ErrorAction SilentlyContinue |
            Remove-Event `
            -ErrorAction SilentlyContinue
    }

    Start-Sleep -Milliseconds 300
}


}
catch {

Write-ErrorMessage "Watcher stopped: $($_.Exception.Message)"


}
finally {

Write-Info "Cleaning watcher..."

foreach ($subscription in $subscriptions) {
    Unregister-Event `
        -SourceIdentifier $subscription.Name `
        -ErrorAction SilentlyContinue
}

$watcher.Dispose()

Write-Info "Watcher stopped."


}