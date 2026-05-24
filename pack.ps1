param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$Instance
)

Invoke-Build -Task Pack -File "$PSScriptRoot/.build.ps1" -Version $Version -Instance $Instance
