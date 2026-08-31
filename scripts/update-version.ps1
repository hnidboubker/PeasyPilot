param(
    [string]$RootPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'

$excludedDirectories = @('.git', 'artifacts', 'bin', 'obj')

$projects = Get-ChildItem -Path $RootPath -File -Recurse -Filter '*.csproj' |
    Where-Object {
        $relativePath = [System.IO.Path]::GetRelativePath($RootPath, $_.FullName)
        $segments = $relativePath -split '[\\/]'
        -not ($segments | Where-Object { $excludedDirectories -contains $_ })
    }

if (-not $projects) {
    Write-Host "Aucun fichier .csproj trouvé dans $RootPath"
    return
}

foreach ($project in $projects) {
    try {
        [xml]$csproj = Get-Content -Path $project.FullName -Raw

        $versionNode = $csproj.SelectSingleNode("//*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='Version']")

        if ($null -eq $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
            Write-Host "$($project.Name) -> pas de Version, ignoré"
            continue
        }

        $oldVersionText = $versionNode.InnerText.Trim()
        $oldVersion = [Version]::Parse($oldVersionText)
        $newVersion = [Version]::new($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build + 1)

        $versionNode.InnerText = $newVersion.ToString()
        $csproj.Save($project.FullName)

        Write-Host "$($project.Name): $oldVersionText -> $newVersion"
    }
    catch {
        Write-Warning "$($project.FullName): impossible de mettre à jour la version - $($_.Exception.Message)"
    }
}
