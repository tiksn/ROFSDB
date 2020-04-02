param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

Invoke-psake -taskList Publish -parameters @{"Version" = $Version }
