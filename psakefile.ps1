Task Publish -Depends Pack {
    Import-Module -Name Microsoft.PowerShell.SecretsManagement
    $apiKey = Get-Secret -Name 'TIKSN-ROFSDB-NuGet-ApiKey' -AsPlainText
    $nupkg = Join-Path -Path $script:trashFolder -ChildPath "ROFSDB.$script:NextVersion.nupkg"
    $nupkg = Resolve-Path -Path $nupkg
    $nupkg = $nupkg.Path
    
    Exec { nuget push $nupkg -ApiKey $apiKey -Source https://api.nuget.org/v3/index.json }
}

Task Pack -Depends Test, EstimateVersions {
    $project = "./src/ROFSDB/ROFSDB.csproj"
    $project = Resolve-Path $project
    $project = $project.Path

    Exec { dotnet pack --configuration Release --output $script:trashFolder -p:version=$script:NextVersion $project }
}

Task EstimateVersions {
    $Version = [Version]$Version

    Assert ($Version.Revision -eq -1) "Version should be formatted as Major.Minor.Patch like 1.2.3"
    Assert ($Version.Build -ne -1) "Version should be formatted as Major.Minor.Patch like 1.2.3"

    $Version = $Version.ToString()
    $script:NextVersion = $Version
}

Task Test -Depends Build {
    $project = Resolve-Path "./src/ROFSDB.Tests/ROFSDB.Tests.csproj"
    $project = $project.Path
    Exec { dotnet test $project }
}

Task Build -Depends Clean {
    $solution = Resolve-Path "./src/ROFSDB.sln"
    $solution = $solution.Path
    Exec { dotnet restore $solution }
    Exec { dotnet build $solution }
}

Task Clean -Depends Init {
}

Task Init {
    $date = Get-Date
    $ticks = $date.Ticks
    $script:trashFolder = Join-Path -Path . -ChildPath ".trash"
    $script:trashFolder = Join-Path -Path $script:trashFolder -ChildPath $ticks.ToString("D19")
    New-Item -Path $script:trashFolder -ItemType Directory | Out-Null
    $script:trashFolder = Resolve-Path -Path $script:trashFolder
}