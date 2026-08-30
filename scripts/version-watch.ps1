$ErrorActionPreference = 'Stop'
$rootDirectory = Split-Path -Parent $PSScriptRoot
$artifactsDirectory = Join-Path $rootDirectory 'artifacts'
$extensions = @('.cs', '.csproj', '.props', '.targets', '.slnx', '.json', '.md', '.png', '.ps1', '.sh')
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

function Test-RelevantChange {
    param([string]$Path)

    return $Path -notmatch '[\\/](\.git|artifacts|bin|obj)([\\/]|$)' -and
        $extensions -contains [System.IO.Path]::GetExtension($Path)
}

function Invoke-BuildAndPack {
    Write-Host 'Build de la solution...'
    & $dotnetPath build (Join-Path $rootDirectory 'easy-peasy.slnx') -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'Le build a échoué. Surveillance maintenue.' -ForegroundColor Red
        return
    }

    Remove-Item $artifactsDirectory -Recurse -Force -ErrorAction SilentlyContinue
    New-Item $artifactsDirectory -ItemType Directory -Force | Out-Null

    Write-Host 'Package de la solution...'
    & $dotnetPath pack (Join-Path $rootDirectory 'easy-peasy.slnx') -c Release --no-build -o $artifactsDirectory
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'Le packaging a échoué. Surveillance maintenue.' -ForegroundColor Red
        return
    }

    Get-ChildItem $artifactsDirectory -File -Recurse -Include '*.nupkg', '*.snupkg' |
        Select-Object -ExpandProperty FullName
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