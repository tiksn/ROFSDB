param(
    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$Instance
)

Invoke-Build -Task Build -File "$PSScriptRoot/.build.ps1" -Version $Version -Instance $Instance
