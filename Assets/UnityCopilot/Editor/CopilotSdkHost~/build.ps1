$ErrorActionPreference = "Stop"

param(
    [ValidateSet("win-x64", "osx-x64", "osx-arm64", "linux-x64")]
    [string[]]$Runtimes = @("win-x64"),
    [switch]$AllRuntimes
)

$projectPath = Join-Path $PSScriptRoot "CopilotSdkHost.csproj"
if (!(Test-Path $projectPath)) {
    throw "Project file not found: $projectPath"
}

if ($AllRuntimes) {
    $Runtimes = @("win-x64", "osx-x64", "osx-arm64", "linux-x64")
}

$Runtimes = $Runtimes | Select-Object -Unique
if ($Runtimes.Count -eq 0) {
    throw "No runtimes selected."
}

dotnet restore "$projectPath"

foreach ($runtime in $Runtimes) {
    $outputPath = Join-Path $PSScriptRoot ("publish/" + $runtime)
    Write-Host "Publishing runtime '$runtime' to '$outputPath'..."
    dotnet publish "$projectPath" -c Release -r $runtime --self-contained true -o $outputPath
}

Write-Host "Publish complete."
