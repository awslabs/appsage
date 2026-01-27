# VS Code Extension Build Script
# This script builds and packages the AppSage VS Code Extension

param(
    [string]$Version
)

# Function to check if a command exists
function Test-Command {
    param([string]$CommandName)
    return $null -ne (Get-Command $CommandName -ErrorAction SilentlyContinue)
}

# Prerequisite checks
if (-not (Test-Command "node")) {
    Write-Host "Error: Node.js not found. Please install from https://nodejs.org/" -ForegroundColor Red
    exit 1
}

if (-not (Test-Command "npm")) {
    Write-Host "Error: npm not found" -ForegroundColor Red
    exit 1
}

if (-not (Test-Command "npx")) {
    Write-Host "Error: npx not found" -ForegroundColor Red
    exit 1
}

# Change to extension directory (script is now in the root of the extension)
$extensionPath = $PSScriptRoot
Set-Location $extensionPath

if (-not (Test-Path "package.json")) {
    Write-Host "Error: package.json not found" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "tsconfig.json")) {
    Write-Host "Error: tsconfig.json not found" -ForegroundColor Red
    exit 1
}

# Build process
Write-Host 'Building VS Code Extension...' -ForegroundColor Green

if (Test-Path 'release') { Remove-Item 'release' -Recurse -Force }
New-Item -ItemType Directory -Path 'release' -Force | Out-Null

npm install
if ($LASTEXITCODE -ne 0) { exit 1 }

npm run compile  
if ($LASTEXITCODE -ne 0) { exit 1 }

# Package with optional version parameter
if ($Version) {
    npx @vscode/vsce package --out release/ $Version
} else {
    npx @vscode/vsce package --out release/
}
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host 'Extension packaged successfully!' -ForegroundColor Green
Get-ChildItem release/*.vsix
