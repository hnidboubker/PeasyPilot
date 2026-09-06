$ErrorActionPreference = 'Stop'

$rootDirectory = Split-Path -Parent $PSScriptRoot
$artifactsDirectory = Join-Path $rootDirectory 'artifacts'
$extensions = @('.cs', '.csproj', '.props', '.targets', '.slnx', '.json', '.md', '.ps1')
$slnxFile = Join-Path $rootDirectory 'justit-mapping.slnx'

# ============================================================
# VERIFY DOTNET
# ============================================================

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnetPath = if ($null -ne $dotnetCommand) {
    $dotnetCommand.Source
}
else {
    Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
}

if (-not (Test-Path $dotnetPath)) {
    throw 'The .NET SDK was not found. Install it or add dotnet to PATH.'
}

if (-not (Test-Path $slnxFile)) {
    throw "Solution file not found: $slnxFile"
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " DOTNET BUILD / PACK - WATCH MODE" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host "[INFO] dotnet: $dotnetPath" -ForegroundColor Cyan
Write-Host "[INFO] root: $rootDirectory" -ForegroundColor Cyan
Write-Host "[INFO] solution: $slnxFile" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# FUNCTIONS
# ============================================================

function Test-RelevantChange {
    param([string]$Path)
    
    # Ignore excluded directories
    if ($Path -match '[\\/](\.git|artifacts|bin|obj|\.vs|packages|\.idea)([\\/]|$)') {
        return $false
    }
    
    # Check if extension matches
    $extension = [System.IO.Path]::GetExtension($Path)
    return $extension -in $extensions
}

function Invoke-BuildAndPack {
    $startTime = Get-Date
    
    Write-Host ""
    Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host " BUILD" -ForegroundColor Green
    Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray
    
    Write-Host "[BUILD] dotnet build" -ForegroundColor Cyan
    & $dotnetPath build $slnxFile -c Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Build failed" -ForegroundColor Red
        return $false
    }

    Write-Host "[SUCCESS] Build OK" -ForegroundColor Green
    
    # Clean artifacts
    Write-Host "[CLEAN] Removing artifacts..." -ForegroundColor Cyan
    Remove-Item $artifactsDirectory -Recurse -Force -ErrorAction SilentlyContinue
    New-Item $artifactsDirectory -ItemType Directory -Force | Out-Null

    Write-Host "[PACK] dotnet pack" -ForegroundColor Cyan
    & $dotnetPath pack $slnxFile -c Release --no-build -o $artifactsDirectory
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Pack failed" -ForegroundColor Red
        return $false
    }

    Write-Host "[SUCCESS] Pack OK" -ForegroundColor Green
    
    # Show packages
    $packages = @(Get-ChildItem $artifactsDirectory -File -Recurse -Include '*.nupkg', '*.snupkg')
    
    if ($packages.Count -gt 0) {
        Write-Host "[SUCCESS] Packages created:" -ForegroundColor Green
        foreach ($pkg in $packages) {
            $size = [Math]::Round($pkg.Length / 1KB, 2)
            Write-Host "  - $($pkg.Name) ($size KB)" -ForegroundColor Green
        }
    }

    $duration = (Get-Date) - $startTime
    Write-Host "[INFO] Duration: $([Math]::Round($duration.TotalSeconds, 2))s" -ForegroundColor Cyan
    
    return $true
}

# ============================================================
# INITIAL BUILD
# ============================================================

Invoke-BuildAndPack

# ============================================================
# WATCH MODE
# ============================================================

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " MONITORING" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host "[WATCH] Watching: $rootDirectory" -ForegroundColor Cyan
Write-Host "[WATCH] Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

$watcher = [System.IO.FileSystemWatcher]::new($rootDirectory)
$watcher.IncludeSubdirectories = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]'FileName, LastWrite, DirectoryName'
$watcher.EnableRaisingEvents = $true

$debounceDelay = 1000  # milliseconds
$lastBuildTime = [DateTime]::MinValue
$pendingBuild = $false
$buildLock = New-Object System.Object

$sourceIdentifiers = @('WatcherChanged', 'WatcherCreated', 'WatcherDeleted', 'WatcherRenamed')

$subscriptions = @(
    Register-ObjectEvent -InputObject $watcher -EventName Changed -SourceIdentifier $sourceIdentifiers[0] -Action {
        if (Test-RelevantChange $EventArgs.FullPath) {
            Set-Variable -Name pendingBuild -Value $true -Scope 1
        }
    }
    Register-ObjectEvent -InputObject $watcher -EventName Created -SourceIdentifier $sourceIdentifiers[1] -Action {
        if (Test-RelevantChange $EventArgs.FullPath) {
            Set-Variable -Name pendingBuild -Value $true -Scope 1
        }
    }
    Register-ObjectEvent -InputObject $watcher -EventName Deleted -SourceIdentifier $sourceIdentifiers[2] -Action {
        if (Test-RelevantChange $EventArgs.FullPath) {
            Set-Variable -Name pendingBuild -Value $true -Scope 1
        }
    }
    Register-ObjectEvent -InputObject $watcher -EventName Renamed -SourceIdentifier $sourceIdentifiers[3] -Action {
        if (Test-RelevantChange $EventArgs.FullPath) {
            Set-Variable -Name pendingBuild -Value $true -Scope 1
        }
    }
)

try {
    while ($true) {
        if ($pendingBuild) {
            $now = [DateTime]::UtcNow
            
            # Debounce: wait at least 1 second since last build
            if (($now - $lastBuildTime).TotalMilliseconds -ge $debounceDelay) {
                $pendingBuild = $false
                
                Write-Host "[WATCH] Change detected. Building..." -ForegroundColor Yellow
                lock ($buildLock) {
                    Invoke-BuildAndPack
                }
                
                $lastBuildTime = [DateTime]::UtcNow
                Write-Host "[WATCH] Ready - waiting for changes..." -ForegroundColor Green
            }
        }

        Start-Sleep -Milliseconds 300
    }
}
finally {
    $subscriptions | Unregister-Event
    $watcher.Dispose()
    Write-Host ""
    Write-Host "[WATCH] Stopped." -ForegroundColor Yellow
}
