[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, HelpMessage="Migration name")]
    [Alias("n")][string]$Name,
    [Parameter(Mandatory=$false, HelpMessage="Startup Project")]
    [Alias("s")][string]$StartupProject = "src/MemoAna.Backend",
    [Parameter(Mandatory=$false, HelpMessage="Migration Project")]
    [Alias("p")][string]$Project = "src/MemoAna.Backend",
    [Parameter(Mandatory=$false, HelpMessage="Migration Output Folder")]
    [Alias("d")][string]$MigrationsFolder = "Infrastructure/Persistence/Migrations"
)
$RepoRoot = (Get-Item "$PSScriptRoot\..").FullName
$ProjectRoot = (Get-Item "$RepoRoot\$StartupProject").FullName
Push-Location $ProjectRoot
try {
    Write-Host $PWD
    Write-Host "Adding migration '$Name'" -ForegroundColor Cyan
    #dotnet ef migrations add $Name --startup-project $StartupProject --project $Project --output-dir $MigrationsFolder
    dotnet ef migrations add $Name --output-dir $MigrationsFolder --verbose
}
finally {
    Pop-Location
}