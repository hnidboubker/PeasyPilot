param(
    [int]$IntervalSeconds = 2
)

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

function Get-LastChangeUtc {
    Get-ChildItem $rootDirectory -File -Recurse |
        Where-Object {
            $_.FullName -notmatch '[\\/](\.git|artifacts|bin|obj)[\\/]' -and
            $extensions -contains $_.Extension
        } |
        Measure-Object -Property LastWriteTimeUtc -Maximum |
        Select-Object -ExpandProperty Maximum
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

Write-Host "Surveillance de $rootDirectory (Ctrl+C pour arrêter)"
Invoke-BuildAndPack
$lastChangeUtc = Get-LastChangeUtc

while ($true) {
    Start-Sleep -Seconds $IntervalSeconds
    $currentChangeUtc = Get-LastChangeUtc

    if ($currentChangeUtc -gt $lastChangeUtc) {
        Write-Host 'Modification détectée. Relance du build et du package...'
        Invoke-BuildAndPack
        $lastChangeUtc = Get-LastChangeUtc
    }
}