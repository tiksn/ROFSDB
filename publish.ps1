param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

Invoke-Build -Task Publish -Version $Version
