#Requires -Version 7.0

<#
.SYNOPSIS
    Installs AppSage from the packaged zip file.

.PARAMETER InstallPath
    Installation directory. Default: C:\Program Files\AppSage

.PARAMETER AddToPath
    Add AppSage to the system PATH environment variable. Default: true

.PARAMETER Scope
    Installation scope: 'Machine' (all users) or 'User' (current user only). Default: Machine

.EXAMPLE
    .\Install-AppSage.ps1
    Installs to C:\Program Files\AppSage for all users

.EXAMPLE
    .\Install-AppSage.ps1 -InstallPath "C:\Tools\AppSage" -Scope User
    Installs to custom path for current user only
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$InstallPath = "C:\Program Files\AppSage",
    
    [Parameter()]
    [bool]$AddToPath = $true,
    
    [Parameter()]
    [ValidateSet('Machine', 'User')]
    [string]$Scope = 'Machine'
)

$ErrorActionPreference = 'Stop'

# Check if running as administrator for Machine scope
if ($Scope -eq 'Machine') {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-Host ""
        Write-Host "ERROR: Administrator privileges required for system-wide installation." -ForegroundColor Red
        Write-Host ""
        Write-Host "Please run this script as Administrator, or use:" -ForegroundColor Yellow
        Write-Host "  .\Install-AppSage.ps1 -Scope User" -ForegroundColor Cyan
        Write-Host ""
        exit 1
    }
}

# Paths
$ScriptRoot = $PSScriptRoot
$AppZipFile = Join-Path $ScriptRoot "AppSageApp.zip"

Write-Host ""
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  AppSage Installation" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Install Location: " -NoNewline
Write-Host $InstallPath -ForegroundColor Yellow
Write-Host "Scope: " -NoNewline
Write-Host $Scope -ForegroundColor Yellow
Write-Host ""

# Verify AppSageApp.zip exists
if (-not (Test-Path $AppZipFile)) {
    Write-Host "ERROR: AppSageApp.zip not found!" -ForegroundColor Red
    Write-Host "Expected location: $AppZipFile" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure AppSageApp.zip is in the same directory as this script." -ForegroundColor Yellow
    exit 1
}

# Step 1: Create installation directory
Write-Host "[1/3] Creating installation directory..." -ForegroundColor Green

if (Test-Path $InstallPath) {
    Write-Host "  Directory already exists, cleaning..." -ForegroundColor DarkGray
    Remove-Item "$InstallPath\*" -Recurse -Force -ErrorAction SilentlyContinue
} else {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

Write-Host "  ? Directory ready: $InstallPath" -ForegroundColor Green
Write-Host ""

# Step 2: Extract application files
Write-Host "[2/3] Extracting application files..." -ForegroundColor Green

Expand-Archive -Path $AppZipFile -DestinationPath $InstallPath -Force

Write-Host "  ? Files extracted" -ForegroundColor Green
Write-Host ""

# Step 3: Add to PATH (optional)
if ($AddToPath) {
    Write-Host "[3/3] Adding to PATH..." -ForegroundColor Green
    
    $target = if ($Scope -eq 'Machine') { [EnvironmentVariableTarget]::Machine } else { [EnvironmentVariableTarget]::User }
    
    $currentPath = [Environment]::GetEnvironmentVariable('PATH', $target)
    
    # Check if already in PATH
    $pathDirs = $currentPath -split ';' | Where-Object { $_ -ne '' }
    $alreadyInPath = $pathDirs | Where-Object { $_ -eq $InstallPath }
    
    if ($alreadyInPath) {
        Write-Host "  Already in PATH" -ForegroundColor DarkGray
    } else {
        $newPath = $currentPath.TrimEnd(';') + ';' + $InstallPath
        [Environment]::SetEnvironmentVariable('PATH', $newPath, $target)
        Write-Host "  ? Added to $Scope PATH" -ForegroundColor Green
    }
    
    Write-Host ""
}

# Installation complete
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  INSTALLATION COMPLETE!" -ForegroundColor Green
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "AppSage has been installed to:" -ForegroundColor White
Write-Host "  $InstallPath" -ForegroundColor Yellow
Write-Host ""

if ($AddToPath) {
    Write-Host "You can now run AppSage from any command prompt:" -ForegroundColor White
    Write-Host "  appsage --help" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Note: You may need to restart your terminal for PATH changes to take effect." -ForegroundColor Yellow
} else {
    Write-Host "To run AppSage, use the full path:" -ForegroundColor White
    Write-Host "  $InstallPath\AppSage.exe" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
