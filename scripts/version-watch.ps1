# $ErrorActionPreference = 'Stop'

# $rootDirectory = Split-Path -Parent $PSScriptRoot
# $artifactsDirectory = Join-Path $rootDirectory 'artifacts'
# $extensions = @('.cs', '.csproj', '.props', '.targets', '.slnx', '.json', '.md', '.ps1')
# $slnxFile = Join-Path $rootDirectory 'justit-mapping.slnx'

# # ============================================================
# # VERIFY DOTNET
# # ============================================================

# $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
# $dotnetPath = if ($null -ne $dotnetCommand) {
#     $dotnetCommand.Source
# }
# else {
#     Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
# }

# if (-not (Test-Path $dotnetPath)) {
#     throw 'The .NET SDK was not found. Install it or add dotnet to PATH.'
# }

# if (-not (Test-Path $slnxFile)) {
#     throw "Solution file not found: $slnxFile"
# }

# Write-Host ""
# Write-Host "============================================================" -ForegroundColor DarkGray
# Write-Host " DOTNET BUILD / PACK - WATCH MODE" -ForegroundColor Green
# Write-Host "============================================================" -ForegroundColor DarkGray
# Write-Host "[INFO] dotnet: $dotnetPath" -ForegroundColor Cyan
# Write-Host "[INFO] root: $rootDirectory" -ForegroundColor Cyan
# Write-Host "[INFO] solution: $slnxFile" -ForegroundColor Cyan
# Write-Host ""

# # ============================================================
# # FUNCTIONS
# # ============================================================

# function Test-RelevantChange {
#     param([string]$Path)
    
#     # Ignore excluded directories
#     if ($Path -match '[\\/](\.git|artifacts|bin|obj|\.vs|packages|\.idea)([\\/]|$)') {
#         return $false
#     }
    
#     # Check if extension matches
#     $extension = [System.IO.Path]::GetExtension($Path)
#     return $extension -in $extensions
# }

# function Invoke-BuildAndPack {
#     $startTime = Get-Date
    
#     Write-Host ""
#     Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray
#     Write-Host " BUILD" -ForegroundColor Green
#     Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray
    
#     Write-Host "[BUILD] dotnet build" -ForegroundColor Cyan
#     & $dotnetPath build $slnxFile -c Release
    
#     if ($LASTEXITCODE -ne 0) {
#         Write-Host "[ERROR] Build failed" -ForegroundColor Red
#         return $false
#     }

#     Write-Host "[SUCCESS] Build OK" -ForegroundColor Green
    
#     # Clean artifacts
#     Write-Host "[CLEAN] Removing artifacts..." -ForegroundColor Cyan
#     Remove-Item $artifactsDirectory -Recurse -Force -ErrorAction SilentlyContinue
#     New-Item $artifactsDirectory -ItemType Directory -Force | Out-Null

#     Write-Host "[PACK] dotnet pack" -ForegroundColor Cyan
#     & $dotnetPath pack $slnxFile -c Release --no-build -o $artifactsDirectory
    
#     if ($LASTEXITCODE -ne 0) {
#         Write-Host "[ERROR] Pack failed" -ForegroundColor Red
#         return $false
#     }

#     Write-Host "[SUCCESS] Pack OK" -ForegroundColor Green
    
#     # Show packages
#     $packages = @(Get-ChildItem $artifactsDirectory -File -Recurse -Include '*.nupkg', '*.snupkg')
    
#     if ($packages.Count -gt 0) {
#         Write-Host "[SUCCESS] Packages created:" -ForegroundColor Green
#         foreach ($pkg in $packages) {
#             $size = [Math]::Round($pkg.Length / 1KB, 2)
#             Write-Host "  - $($pkg.Name) ($size KB)" -ForegroundColor Green
#         }
#     }

#     $duration = (Get-Date) - $startTime
#     Write-Host "[INFO] Duration: $([Math]::Round($duration.TotalSeconds, 2))s" -ForegroundColor Cyan
    
#     return $true
# }

# # ============================================================
# # INITIAL BUILD
# # ============================================================

# Invoke-BuildAndPack

# # ============================================================
# # WATCH MODE
# # ============================================================

# Write-Host ""
# Write-Host "============================================================" -ForegroundColor DarkGray
# Write-Host " MONITORING" -ForegroundColor Green
# Write-Host "============================================================" -ForegroundColor DarkGray
# Write-Host "[WATCH] Watching: $rootDirectory" -ForegroundColor Cyan
# Write-Host "[WATCH] Press Ctrl+C to stop" -ForegroundColor Yellow
# Write-Host ""

# $watcher = [System.IO.FileSystemWatcher]::new($rootDirectory)
# $watcher.IncludeSubdirectories = $true
# $watcher.NotifyFilter = [System.IO.NotifyFilters]'FileName, LastWrite, DirectoryName'
# $watcher.EnableRaisingEvents = $true

# $debounceDelay = 1000  # milliseconds
# $lastBuildTime = [DateTime]::MinValue
# $pendingBuild = $false
# $buildLock = New-Object System.Object

# $sourceIdentifiers = @('WatcherChanged', 'WatcherCreated', 'WatcherDeleted', 'WatcherRenamed')

# $subscriptions = @(
#     Register-ObjectEvent -InputObject $watcher -EventName Changed -SourceIdentifier $sourceIdentifiers[0] -Action {
#         if (Test-RelevantChange $EventArgs.FullPath) {
#             Set-Variable -Name pendingBuild -Value $true -Scope 1
#         }
#     }
#     Register-ObjectEvent -InputObject $watcher -EventName Created -SourceIdentifier $sourceIdentifiers[1] -Action {
#         if (Test-RelevantChange $EventArgs.FullPath) {
#             Set-Variable -Name pendingBuild -Value $true -Scope 1
#         }
#     }
#     Register-ObjectEvent -InputObject $watcher -EventName Deleted -SourceIdentifier $sourceIdentifiers[2] -Action {
#         if (Test-RelevantChange $EventArgs.FullPath) {
#             Set-Variable -Name pendingBuild -Value $true -Scope 1
#         }
#     }
#     Register-ObjectEvent -InputObject $watcher -EventName Renamed -SourceIdentifier $sourceIdentifiers[3] -Action {
#         if (Test-RelevantChange $EventArgs.FullPath) {
#             Set-Variable -Name pendingBuild -Value $true -Scope 1
#         }
#     }
# )

# try {
#     while ($true) {
#         if ($pendingBuild) {
#             $now = [DateTime]::UtcNow
            
#             # Debounce: wait at least 1 second since last build
#             if (($now - $lastBuildTime).TotalMilliseconds -ge $debounceDelay) {
#                 $pendingBuild = $false
                
#                 Write-Host "[WATCH] Change detected. Building..." -ForegroundColor Yellow
#                 lock ($buildLock) {
#                     Invoke-BuildAndPack
#                 }
                
#                 $lastBuildTime = [DateTime]::UtcNow
#                 Write-Host "[WATCH] Ready - waiting for changes..." -ForegroundColor Green
#             }
#         }

#         Start-Sleep -Milliseconds 300
#     }
# }
# finally {
#     $subscriptions | Unregister-Event
#     $watcher.Dispose()
#     Write-Host ""
#     Write-Host "[WATCH] Stopped." -ForegroundColor Yellow
# }

# Configuration
$solutionPath = "C:\chemin\vers\votre\solution"  # À modifier avec votre chemin
$csprojPath = Get-ChildItem -Path $solutionPath -Filter "*.csproj" -Recurse | Select-Object -First 1
$artifactFolder = Join-Path -Path $solutionPath -ChildPath "artifacts"

# Créer le dossier artifacts s'il n'existe pas
if (-not (Test-Path -Path $artifactFolder)) {
    New-Item -ItemType Directory -Path $artifactFolder | Out-Null
    Write-Host "✓ Dossier artifacts créé : $artifactFolder" -ForegroundColor Green
}

Write-Host "🔍 Surveillance du fichier: $($csprojPath.FullName)" -ForegroundColor Cyan
Write-Host "📦 Output: $artifactFolder" -ForegroundColor Cyan
Write-Host "⏳ En attente de changements... (Ctrl+C pour arrêter)`n" -ForegroundColor Yellow

# Initialiser le FileSystemWatcher
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $csprojPath.DirectoryName
$watcher.Filter = $csprojPath.Name
$watcher.IncludeSubdirectories = $false
$watcher.EnableRaisingEvents = $true

# Variable pour éviter les déclenchements multiples
$lastChange = $null
$debounceMs = 1000

# Action au changement de fichier
$action = {
    $currentTime = Get-Date
    
    # Éviter les changements en rafale (debouncing)
    if ($lastChange -and ($currentTime - $lastChange).TotalMilliseconds -lt $debounceMs) {
        return
    }
    
    $script:lastChange = $currentTime
    
    Write-Host "`n⚡ Changement détecté à $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Magenta
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
    
    $buildSuccess = $true
    $projectDir = $csprojPath.DirectoryName
    
    try {
        # 1. dotnet build
        Write-Host "📌 [1/4] Exécution: dotnet build" -ForegroundColor Cyan
        & dotnet build $csprojPath.FullName
        if ($LASTEXITCODE -ne 0) {
            throw "❌ ERREUR: dotnet build a échoué (code: $LASTEXITCODE)"
        }
        Write-Host "✓ dotnet build réussi`n" -ForegroundColor Green
        
        # 2. dotnet build -c Release
        Write-Host "📌 [2/4] Exécution: dotnet build -c Release" -ForegroundColor Cyan
        & dotnet build $csprojPath.FullName -c Release
        if ($LASTEXITCODE -ne 0) {
            throw "❌ ERREUR: dotnet build -c Release a échoué (code: $LASTEXITCODE)"
        }
        Write-Host "✓ dotnet build -c Release réussi`n" -ForegroundColor Green
        
        # 3. dotnet pack -c Release
        Write-Host "📌 [3/4] Exécution: dotnet pack -c Release" -ForegroundColor Cyan
        & dotnet pack $csprojPath.FullName -c Release -o $artifactFolder
        if ($LASTEXITCODE -ne 0) {
            throw "❌ ERREUR: dotnet pack a échoué (code: $LASTEXITCODE)"
        }
        Write-Host "✓ dotnet pack réussi`n" -ForegroundColor Green
        
        # 4. Vérifier les outputs et les déplacer si nécessaire
        Write-Host "📌 [4/4] Déplacement des packages vers artifacts" -ForegroundColor Cyan
        $binFolder = Join-Path -Path $projectDir -ChildPath "bin\Release"
        
        if (Test-Path -Path $binFolder) {
            $nupkgFiles = Get-ChildItem -Path $binFolder -Filter "*.nupkg" -Recurse
            if ($nupkgFiles) {
                foreach ($file in $nupkgFiles) {
                    Copy-Item -Path $file.FullName -Destination $artifactFolder -Force
                    Write-Host "  → Copié: $($file.Name)" -ForegroundColor Gray
                }
                Write-Host "✓ Packages déplacés avec succès`n" -ForegroundColor Green
            }
        }
        
    }
    catch {
        $buildSuccess = $false
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host "`n❌ ERREUR - Pipeline interrompu`n" -ForegroundColor Red
    }
    
    # Affichage final
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
    if ($buildSuccess) {
        Write-Host "✅ SUCCÈS - Tous les packages sont dans: $artifactFolder" -ForegroundColor Green
        Write-Host "🕐 Terminer à: $(Get-Date -Format 'HH:mm:ss')`n" -ForegroundColor Green
    } else {
        Write-Host "🔴 ÉCHEC - Veuillez vérifier les erreurs ci-dessus" -ForegroundColor Red
        Write-Host "🕐 Erreur à: $(Get-Date -Format 'HH:mm:ss')`n" -ForegroundColor Red
    }
    
    Write-Host "⏳ En attente de changements..." -ForegroundColor Yellow
}

# Enregistrer les événements
Register-ObjectEvent -InputObject $watcher -EventName "Changed" -Action $action | Out-Null
Register-ObjectEvent -InputObject $watcher -EventName "Created" -Action $action | Out-Null

# Boucle infinie
while ($true) {
    Start-Sleep -Milliseconds 100
}

