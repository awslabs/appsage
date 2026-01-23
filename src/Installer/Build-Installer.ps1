<#
.SYNOPSIS
    Builds the AppSage installer MSI package.

.DESCRIPTION
    This script performs the following steps:
    1. Publishes AppSage.Run project
    2. Zips the published files
    3. Builds the installer using Visual Studio

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.EXAMPLE
    .\BuildInstaller.ps1
    Builds Release installer

.EXAMPLE
    .\BuildInstaller.ps1 -Configuration Debug
    Builds Debug installer
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

# Script configuration
$ScriptRoot = $PSScriptRoot
$SolutionRoot = Split-Path $ScriptRoot -Parent
$AppSageRunProject = Join-Path $SolutionRoot "AppSage.Run\AppSage.Run.csproj"
$ApplicationFilesPath = Join-Path $ScriptRoot "AppSage.Installer\ApplicationFiles"
$ZipFilePath = Join-Path $ScriptRoot "AppSage.Installer\AppSageFiles.zip"
$InstallerProject = Join-Path $ScriptRoot "AppSage.Installer\AppSage.Installer.vdproj"

# Display header
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          AppSage Installer Build Script                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration: " -NoNewline
Write-Host $Configuration -ForegroundColor Yellow
Write-Host ""

# Step 1: Publish AppSage.Run
Write-Host "[1/4] Publishing AppSage.Run..." -ForegroundColor Green

$publishArgs = @(
    'publish',
    "`"$AppSageRunProject`"",
    '-c', $Configuration,
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-o', "`"$ApplicationFilesPath`""
)

if ($Configuration -eq 'Release') {
    $publishArgs += '-p:CopyOutputSymbolsToPublishDirectory=false'
    $publishArgs += '-p:DebugType=none'
} else {
    $publishArgs += '-p:CopyOutputSymbolsToPublishDirectory=true'
    $publishArgs += '-p:DebugType=portable'
}

Write-Host "  Command: dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw "Failed to publish AppSage.Run (exit code: $LASTEXITCODE)"
}

Write-Host "  ✓ Published successfully" -ForegroundColor Green
Write-Host ""

# Step 2: Zip ApplicationFiles
Write-Host "[2/4] Creating zip file..." -ForegroundColor Green

if (Test-Path $ZipFilePath) {
    Write-Host "  Removing existing zip file..." -ForegroundColor DarkGray
    Remove-Item $ZipFilePath -Force
}

Write-Host "  Compressing files from: $ApplicationFilesPath" -ForegroundColor DarkGray
Compress-Archive -Path "$ApplicationFilesPath\*" -DestinationPath $ZipFilePath -Force

$zipSize = (Get-Item $ZipFilePath).Length / 1MB
Write-Host "  ✓ Created: $ZipFilePath ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor Green
Write-Host ""

# Step 3: Find Visual Studio
Write-Host "[3/4] Locating Visual Studio..." -ForegroundColor Green

$vsWherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

if (-not (Test-Path $vsWherePath)) {
    throw "vswhere.exe not found at: $vsWherePath. Please ensure Visual Studio is installed."
}

# Find latest Visual Studio installation
$vsInstallPath = & $vsWherePath -latest -products * -property installationPath
$vsVersion = & $vsWherePath -latest -property catalog_productDisplayVersion
$vsDisplayName = & $vsWherePath -latest -property displayName

if (-not $vsInstallPath) {
    throw "Could not find Visual Studio installation using vswhere.exe"
}

$devenvPath = Join-Path $vsInstallPath "Common7\IDE\devenv.com"

if (-not (Test-Path $devenvPath)) {
    throw "devenv.com not found at: $devenvPath"
}

Write-Host "  Found: $vsDisplayName (v$vsVersion)" -ForegroundColor DarkGray
Write-Host "  Path: $vsInstallPath" -ForegroundColor DarkGray
Write-Host "  ✓ Visual Studio located" -ForegroundColor Green
Write-Host ""

# Step 4: Build Installer
Write-Host "[4/4] Building installer..." -ForegroundColor Green
Write-Host "  Project: $InstallerProject" -ForegroundColor DarkGray
Write-Host "  Configuration: $Configuration" -ForegroundColor DarkGray

& $devenvPath $InstallerProject /build $Configuration

if ($LASTEXITCODE -ne 0) {
    throw "Failed to build installer (exit code: $LASTEXITCODE)"
}

Write-Host "  ✓ Installer built successfully" -ForegroundColor Green
Write-Host ""

# Display completion message
$installerOutput = Join-Path $ScriptRoot "Installer\AppSage.Installer\$Configuration\AppSage.Installer.msi"

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "BUILD COMPLETE!" -ForegroundColor Green -NoNewline
Write-Host " 🎉" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $installerOutput) {
    $msiSize = (Get-Item $installerOutput).Length / 1MB
    Write-Host "Installer Location:" -ForegroundColor White
    Write-Host "  $installerOutput" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "File Size:" -ForegroundColor White
    Write-Host "  $([math]::Round($msiSize, 2)) MB" -ForegroundColor Yellow
} else {
    Write-Host "⚠ Warning: Expected installer not found at:" -ForegroundColor Red
    Write-Host "  $installerOutput" -ForegroundColor Red
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan