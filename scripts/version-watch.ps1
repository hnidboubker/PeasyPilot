param(
[string]$ProjectPath = ".",
[ValidateSet("Debug", "Release")]
[string]$Configuration = "Release",
[switch]$Watch
)

$ErrorActionPreference = "Continue"

============================================================
DOTNET
============================================================

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue

if (-not $dotnet) {
Write-Host "[ERROR] dotnet not found." -ForegroundColor Red
Write-Host "Install the .NET SDK or add dotnet to PATH." -ForegroundColor Yellow
exit 1
}

$dotnetPath = $dotnet.Source

============================================================
ROOT DIRECTORY
============================================================

try {
$rootDirectory = (Resolve-Path $ProjectPath -ErrorAction Stop).Path
}
catch {
Write-Host "[ERROR] Project path not found: $ProjectPath" -ForegroundColor Red
exit 1
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " DOTNET BUILD / PACK" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray

Write-Host "[INFO] dotnet : $dotnetPath" -ForegroundColor Cyan
Write-Host "[INFO] project path : $rootDirectory" -ForegroundColor Cyan
Write-Host "[INFO] configuration: $Configuration" -ForegroundColor Cyan
Write-Host ""

============================================================
FIND SOLUTION / PROJECT
============================================================

$solution = Get-ChildItem -Path $rootDirectory
-File `
-ErrorAction SilentlyContinue |
Where-Object {
$.Extension -eq ".slnx" -or
$.Extension -eq ".sln"
} |
Select-Object -First 1

$projects = @(Get-ChildItem -Path $rootDirectory
-Filter "*.csproj" -File
-ErrorAction SilentlyContinue)

$buildTarget = $null

if ($solution) {

$buildTarget = $solution.FullName

Write-Host "[INFO] Solution found: $($solution.Name)" -ForegroundColor Cyan


}
elseif ($projects.Count -eq 1) {

$buildTarget = $projects[0].FullName

Write-Host "[INFO] Project found: $($projects[0].Name)" -ForegroundColor Cyan


}
elseif ($projects.Count -gt 1) {

Write-Host "[INFO] Multiple projects found:" -ForegroundColor Yellow
Write-Host ""

for ($i = 0; $i -lt $projects.Count; $i++) {
    Write-Host "  [$i] $($projects[$i].Name)"
}

Write-Host ""

$choice = Read-Host "Choose project number"

if ($choice -notmatch '^\d+$') {
    Write-Host "[ERROR] Invalid project number." -ForegroundColor Red
    exit 1
}

$index = [int]$choice

if ($index -lt 0 -or $index -ge $projects.Count) {
    Write-Host "[ERROR] Invalid project number." -ForegroundColor Red
    exit 1
}

$buildTarget = $projects[$index].FullName


}
else {

Write-Host "[ERROR] No .sln, .slnx or .csproj found." -ForegroundColor Red
Write-Host "[ERROR] Directory: $rootDirectory" -ForegroundColor Red
exit 1


}

Write-Host "[INFO] Target: $buildTarget" -ForegroundColor Cyan
Write-Host ""

============================================================
ARTIFACTS
============================================================

$artifactsDirectory = Join-Path $rootDirectory "artifacts"

============================================================
BUILD + PACK
============================================================

function Invoke-BuildAndPack {

Write-Host ""
Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray
Write-Host " BUILD" -ForegroundColor Green
Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray

$startTime = Get-Date

# --------------------------------------------------------
# BUILD
# --------------------------------------------------------

Write-Host "[BUILD] dotnet build" -ForegroundColor Cyan
Write-Host ""

& $dotnetPath build $buildTarget -c $Configuration

$buildExitCode = $LASTEXITCODE

Write-Host ""

if ($buildExitCode -ne 0) {

    Write-Host "[ERROR] BUILD FAILED" -ForegroundColor Red
    Write-Host "[ERROR] Exit code: $buildExitCode" -ForegroundColor Red

    return $false
}

Write-Host "[SUCCESS] BUILD OK" -ForegroundColor Green

# --------------------------------------------------------
# CLEAN ARTIFACTS
# --------------------------------------------------------

if (Test-Path $artifactsDirectory) {

    Write-Host "[CLEAN] Removing artifacts..." -ForegroundColor Cyan

    Remove-Item `
        $artifactsDirectory `
        -Recurse `
        -Force `
        -ErrorAction SilentlyContinue
}

New-Item `
    -Path $artifactsDirectory `
    -ItemType Directory `
    -Force |
    Out-Null

# --------------------------------------------------------
# PACK
# --------------------------------------------------------

Write-Host ""
Write-Host "[PACK] dotnet pack" -ForegroundColor Cyan
Write-Host ""

& $dotnetPath pack `
    $buildTarget `
    -c $Configuration `
    -o $artifactsDirectory `
    --no-build

$packExitCode = $LASTEXITCODE

Write-Host ""

if ($packExitCode -ne 0) {

    Write-Host "[ERROR] PACK FAILED" -ForegroundColor Red
    Write-Host "[ERROR] Exit code: $packExitCode" -ForegroundColor Red

    return $false
}

Write-Host "[SUCCESS] PACK OK" -ForegroundColor Green

# --------------------------------------------------------
# SHOW PACKAGES
# --------------------------------------------------------

$packages = @(Get-ChildItem `
    -Path $artifactsDirectory `
    -File `
    -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Extension -eq ".nupkg" -or
        $_.Extension -eq ".snupkg"
    })

Write-Host ""

if ($packages.Count -eq 0) {

    Write-Host "[WARNING] No NuGet package found." -ForegroundColor Yellow
}
else {

    Write-Host "[SUCCESS] Packages created:" -ForegroundColor Green

    foreach ($package in $packages) {

        $size = [Math]::Round(
            $package.Length / 1KB,
            2
        )

        Write-Host "  - $($package.Name) ($size KB)" -ForegroundColor Green
    }
}

# --------------------------------------------------------
# RESULT
# --------------------------------------------------------

$duration = (Get-Date) - $startTime

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " SUCCESS" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host "[SUCCESS] Build + Pack completed." -ForegroundColor Green
Write-Host "[INFO] Duration: $([Math]::Round($duration.TotalSeconds, 2)) seconds" -ForegroundColor Cyan
Write-Host "[INFO] Artifacts: $artifactsDirectory" -ForegroundColor Cyan
Write-Host ""

return $true


}

============================================================
INITIAL BUILD
============================================================

$success = Invoke-BuildAndPack

============================================================
NORMAL MODE
Stop after success or error
============================================================

if (-not $Watch) {

if ($success) {
    exit 0
}
else {
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
Write-Host "[WATCH] Watching: $rootDirectory" -ForegroundColor Cyan
Write-Host "[WATCH] Press Ctrl+C to stop." -ForegroundColor Yellow
Write-Host ""

$watcher = New-Object System.IO.FileSystemWatcher

$watcher.Path = $rootDirectory
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::FileName -bor
[System.IO.NotifyFilters]::LastWrite -bor `
[System.IO.NotifyFilters]::DirectoryName

$watcher.EnableRaisingEvents = $true

$eventName = "GenericDotnetWatcher"

Register-ObjectEvent -InputObject $watcher
-EventName Changed -SourceIdentifier $eventName
-Action {

    $path = $EventArgs.FullPath

    if (
        $path -notmatch '[\\/](bin|obj|artifacts|\.git|\.vs|packages|\.idea)([\\/]|$)'
    ) {

        $extension = [System.IO.Path]::GetExtension($path)

        if (
            $extension -in @(
                ".cs",
                ".csproj",
                ".props",
                ".targets",
                ".sln",
                ".slnx",
                ".json",
                ".md",
                ".ps1"
            )
        ) {

            Set-Content `
                -Path (Join-Path $env:TEMP "generic-dotnet-build") `
                -Value (Get-Date)
        }
    }
} | Out-Null


try {

while ($true) {

    $marker = Join-Path $env:TEMP "generic-dotnet-build"

    if (Test-Path $marker) {

        Remove-Item `
            $marker `
            -Force `
            -ErrorAction SilentlyContinue

        Start-Sleep -Milliseconds 1000

        Write-Host ""
        Write-Host "[WATCH] Change detected." -ForegroundColor Yellow

        $success = Invoke-BuildAndPack

        if ($success) {
            Write-Host "[WATCH] READY - waiting for changes..." -ForegroundColor Green
        }
        else {
            Write-Host "[WATCH] ERROR - waiting for changes..." -ForegroundColor Red
        }
    }

    Start-Sleep -Milliseconds 300
}


}
finally {

Unregister-Event `
    -SourceIdentifier $eventName `
    -ErrorAction SilentlyContinue

$watcher.Dispose()

Write-Host ""
Write-Host "[WATCH] Stopped." -ForegroundColor Yellow


}