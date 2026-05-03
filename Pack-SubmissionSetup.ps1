# Builds a SMALL framework-dependent setup under output\setup (typ. under 100 MB zipped).
# Usage (from repo root):
#   .\Pack-SubmissionSetup.ps1
#   .\Pack-SubmissionSetup.ps1 -Zip
# Requires: .NET 8 SDK

param(
    [switch] $Zip
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$SetupRoot = Join-Path $Root "output\setup"

$apps = @(
    @{ Project = "src\NetSupport.Tutor\NetSupport.Tutor.csproj"; Out = "Tutor" },
    @{ Project = "src\NetSupport.Student\NetSupport.Student.csproj"; Out = "Student" },
    @{ Project = "src\NetSupport.Designer\NetSupport.Designer.csproj"; Out = "Designer" }
)

Write-Host "Publishing framework-dependent (win-x64) to $SetupRoot ..."

foreach ($a in $apps) {
    $dest = Join-Path $SetupRoot $a.Out
    if (Test-Path $dest) {
        Remove-Item -Recurse -Force $dest
    }
    $proj = Join-Path $Root $a.Project
    dotnet publish $proj `
        -c Release `
        -r win-x64 `
        --self-contained false `
        -p:DebugType=none `
        -p:DebugSymbols=false `
        -o $dest
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Get-ChildItem -Path $dest -Filter *.pdb -ErrorAction SilentlyContinue | Remove-Item -Force
}

$samplesDest = Join-Path $SetupRoot "samples\exams"
if (Test-Path $samplesDest) {
    Remove-Item -Recurse -Force $samplesDest
}
New-Item -ItemType Directory -Force -Path $samplesDest | Out-Null
Copy-Item -Path (Join-Path $Root "samples\exams\*") -Destination $samplesDest -Recurse -Force

foreach ($f in @("README.md", "INSTALL.txt")) {
    Copy-Item -Path (Join-Path $Root $f) -Destination (Join-Path $SetupRoot $f) -Force
}

Write-Host "Done. Setup folder: $SetupRoot"

if ($Zip) {
    $zipPath = Join-Path $Root "output\NetSupportSchool-Setup.zip"
    if (Test-Path $zipPath) {
        Remove-Item -Force $zipPath
    }
    Compress-Archive -Path (Join-Path $SetupRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
    $sizeMb = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Host "Created: $zipPath ($sizeMb MB)"
}
