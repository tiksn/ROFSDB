param(
    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$Instance,

    [Parameter(Mandatory = $false)]
    [switch]$Check
)

Invoke-Build -Task Format -File "$PSScriptRoot/.build.ps1" -Version $Version -Instance $Instance -Check:$Check
