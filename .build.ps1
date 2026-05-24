<#
.Synopsis
	Build script invoked by Invoke-Build.

.Description

#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$Instance,

    [Parameter(Mandatory = $false)]
    [switch]$Check
)

function Resolve-RepositoryPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $resolvedPath = Resolve-Path $Path
    return $resolvedPath.Path
}

function Get-NuGetApiKey {
    if (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {
        return $env:NUGET_API_KEY
    }

    if (Get-Module -Name Microsoft.PowerShell.SecretManagement -ListAvailable) {
        Import-Module -Name Microsoft.PowerShell.SecretManagement
        $apiKey = Get-Secret -Name 'TIKSN-ROFSDB-NuGet-ApiKey' -AsPlainText -ErrorAction SilentlyContinue

        if (-not [string]::IsNullOrWhiteSpace($apiKey)) {
            return $apiKey
        }
    }

    return $null
}

task Init {
    $script:solution = Resolve-RepositoryPath './src/ROFSDB.slnx'
    $script:NextVersion = $null
    $script:trashFolder = Join-Path -Path . -ChildPath '.trash'

    if ([string]::IsNullOrWhiteSpace($Instance)) {
        $Instance = Get-Date -Format 'yyyyMMddHHmmss'
    }

    $script:trashFolder = Join-Path -Path $script:trashFolder -ChildPath $Instance
    New-Item -Path $script:trashFolder -ItemType Directory -Force | Out-Null
    $script:trashFolder = Resolve-RepositoryPath $script:trashFolder
}

task Clean Init, {
    Exec { dotnet clean $script:solution }
}

task Restore Clean, {
    Exec { dotnet restore $script:solution }
}

task Format Restore, {
    if ($Check -or $env:CI) {
        Exec { dotnet format whitespace $script:solution --verify-no-changes }
        Exec { dotnet format style $script:solution --severity info --verify-no-changes }
        Exec { dotnet format analyzers $script:solution --severity info --verify-no-changes }
    }
    else {
        Exec { dotnet format whitespace $script:solution }
        Exec { dotnet format style $script:solution --severity info }
        Exec { dotnet format analyzers $script:solution --severity info }
    }
}

task Version Init, {
    if ([string]::IsNullOrWhiteSpace($Version)) {
        return
    }

    $script:NextVersion = $Version
    Write-Build Green "Package version: $script:NextVersion"
}

task Build Format, Version, {
    if ([string]::IsNullOrWhiteSpace($script:NextVersion)) {
        Exec { dotnet build $script:solution --configuration Release --no-restore }
    }
    else {
        Exec { dotnet build $script:solution --configuration Release --no-restore -p:Version=$script:NextVersion }
    }
}

task Test Build, {
    $project = Resolve-RepositoryPath './src/ROFSDB.Tests/ROFSDB.Tests.csproj'

    Exec { dotnet test $project --configuration Release --no-build }
}

task Pack Test, Version, {
    Assert (-not [string]::IsNullOrWhiteSpace($script:NextVersion)) 'Version is required for package creation.'

    $project = Resolve-RepositoryPath './src/ROFSDB/ROFSDB.csproj'

    Exec { dotnet pack $project --configuration Release --no-build --output $script:trashFolder -p:Version=$script:NextVersion }
}

task Publish Pack, {
    $apiKey = Get-NuGetApiKey
    Assert (-not [string]::IsNullOrWhiteSpace($apiKey)) 'NUGET_API_KEY environment variable or TIKSN-ROFSDB-NuGet-ApiKey SecretManagement secret is required to publish.'

    $nupkg = Join-Path -Path $script:trashFolder -ChildPath "ROFSDB.$script:NextVersion.nupkg"
    $nupkg = Resolve-RepositoryPath $nupkg

    Exec { dotnet nuget push $nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json }
}
