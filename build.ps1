param(
    [string[]]$Runtimes,
    [string]$Configuration
)

$ErrorActionPreference = "Stop"

if (-not $Runtimes -or $Runtimes.Count -eq 0) {
    $Runtimes = @("win-x64", "osx-x64", "osx-arm64", "linux-x64")
}

if ([string]::IsNullOrWhiteSpace($Configuration)) {
    $Configuration = "Release"
}

$root = $PSScriptRoot
$hostDir = Join-Path $root "Assets/UnityCopilot/Editor/CopilotSdkHost~"

if (-not (Test-Path $hostDir)) {
    throw "Copilot SDK host project not found at $hostDir"
}

Push-Location $hostDir
try {
    dotnet restore

    foreach ($rid in $Runtimes) {
        $outDir = Join-Path $hostDir ("publish/" + $rid)
        dotnet publish -c $Configuration -r $rid --self-contained true -o $outDir
    }
} finally {
    Pop-Location
}
