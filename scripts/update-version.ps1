param(
    [string]$RootPath = "."
)

$projects = Get-ChildItem `
    -Path $RootPath `
    -Filter "*.csproj" `
    -Recurse

foreach ($project in $projects) {

    [xml]$csproj = Get-Content $project.FullName

    $versionNode = $csproj.Project.PropertyGroup |
        Where-Object { $_.Version } |
        Select-Object -First 1

    if ($null -eq $versionNode) {
        Write-Host "$($project.Name) -> pas de version, ignoré"
        continue
    }

    $oldVersion = [Version]$versionNode.Version

    $newVersion = [Version]::new(
        $oldVersion.Major,
        $oldVersion.Minor,
        $oldVersion.Build + 1
    )

    $versionNode.Version = $newVersion.ToString()

    $csproj.Save($project.FullName)

    Write-Host "$($project.Name) : $oldVersion -> $newVersion"
}
