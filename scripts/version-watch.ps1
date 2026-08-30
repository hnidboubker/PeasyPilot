param(
    [int]$IntervalSeconds = 2
)

$ErrorActionPreference = 'Stop'
$rootDirectory = Split-Path -Parent $PSScriptRoot
$artifactsDirectory = Join-Path $rootDirectory 'artifacts'
$extensions = @('.cs', '.csproj', '.props', '.targets', '.slnx', '.json', '.md', '.png', '.ps1', '.sh')

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
    & dotnet build (Join-Path $rootDirectory 'easy-peasy.slnx') -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'Le build a échoué. Surveillance maintenue.' -ForegroundColor Red
        return
    }

    Remove-Item $artifactsDirectory -Recurse -Force -ErrorAction SilentlyContinue
    New-Item $artifactsDirectory -ItemType Directory -Force | Out-Null

    Write-Host 'Package de la solution...'
    & dotnet pack (Join-Path $rootDirectory 'easy-peasy.slnx') -c Release --no-build -o $artifactsDirectory
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