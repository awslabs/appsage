#Requires -Version 7.0

<#
.SYNOPSIS
    Builds and packages AppSage as a zip file.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

# Paths
$ScriptRoot = $PSScriptRoot
$SolutionRoot = Split-Path $ScriptRoot -Parent
$AppSageRunProject = Join-Path $SolutionRoot "src\AppSage.Run\AppSage.Run.csproj"
$PublishOutput = Join-Path $ScriptRoot "publish"
$ZipFile = Join-Path $ScriptRoot "AppSageApp.zip"
$InstallerZip = Join-Path $ScriptRoot "AppSageInstaller.zip"

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Building AppSage Package" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean old files
Write-Host "[1/3] Cleaning old files..." -ForegroundColor Green
if (Test-Path $PublishOutput) {
    Remove-Item $PublishOutput -Recurse -Force
}
if (Test-Path $ZipFile) {
    Remove-Item $ZipFile -Force
}
Write-Host "  ✓ Cleaned" -ForegroundColor Green
Write-Host ""

# Step 2: Publish AppSage.Run
Write-Host "[2/4] Publishing AppSage ($Configuration)..." -ForegroundColor Green

$publishArgs = @(
    'publish',
    "`"$AppSageRunProject`"",
    '-c', $Configuration,
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-o', "`"$PublishOutput`""
)

if ($Configuration -eq 'Release') {
    $publishArgs += '-p:DebugType=none'
}

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed"
}

Write-Host "  ✓ Published to: $PublishOutput" -ForegroundColor Green
Write-Host ""

# Step 3: Create zip
Write-Host "[3/3] Creating zip package..." -ForegroundColor Green

Compress-Archive -Path "$PublishOutput\*" -DestinationPath $ZipFile -Force

$zipSize = (Get-Item $ZipFile).Length / 1MB

Write-Host "  ✓ Package created" -ForegroundColor Green
Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  BUILD COMPLETE!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package: " -NoNewline
Write-Host $ZipFile -ForegroundColor Yellow
Write-Host "Size: " -NoNewline
Write-Host "$([math]::Round($zipSize, 2)) MB" -ForegroundColor Yellow
Write-Host ""