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

# Function to check if directory is empty or non-existent
function Test-DirectoryEmpty {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        return $true  # Non-existent directory is considered "empty"
    }
    
    $items = Get-ChildItem $Path -Force -ErrorAction SilentlyContinue
    return ($items.Count -eq 0)
}

# Function to prompt user for new installation path
function Get-NewInstallPath {
    param([string]$CurrentPath)
    
    Write-Host ""
    Write-Host "The directory '$CurrentPath' already exists and is not empty." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "For safety, AppSage requires an empty or non-existent directory." -ForegroundColor White
    Write-Host ""
    Write-Host "Please choose one of the following options:" -ForegroundColor Cyan
    Write-Host "  [1] Enter a different installation path" -ForegroundColor White
    Write-Host "  [2] Delete existing folder and install fresh" -ForegroundColor White
    Write-Host "  [3] Cancel installation" -ForegroundColor White
    Write-Host ""
    
    do {
        $choice = Read-Host "Enter your choice (1, 2, or 3)"
    
        switch ($choice) {
            "1" {
                Write-Host ""
                $newPath = Read-Host "Enter new installation path"
     
                if ([string]::IsNullOrWhiteSpace($newPath)) {
                    Write-Host "Invalid path. Please try again." -ForegroundColor Red
                    continue
                }
      
                # Expand environment variables and relative paths
                $newPath = [System.Environment]::ExpandEnvironmentVariables($newPath)
                $newPath = [System.IO.Path]::GetFullPath($newPath)
     
                Write-Host ""
                Write-Host "Checking new path: $newPath" -ForegroundColor Gray   
                if (Test-DirectoryEmpty $newPath) {
                    Write-Host "✓ Path is suitable for installation" -ForegroundColor Green
                    return $newPath
                }
                else {
                    Write-Host "✗ This directory also exists and is not empty." -ForegroundColor Red
                    Write-Host ""
        
                    # Recursively call function with the new path that's also non-empty
                    return Get-NewInstallPath $newPath
                }
            }
            "2" {
                Write-Host ""
                Write-Host "WARNING: This will permanently delete all contents of '$CurrentPath'" -ForegroundColor Red
                Write-Host ""
    
                # Show what will be deleted
                try {
                    $items = Get-ChildItem $CurrentPath -Force -ErrorAction SilentlyContinue
                    if ($items.Count -gt 0) {
                        Write-Host "Contents that will be deleted:" -ForegroundColor Yellow
                        $items | ForEach-Object { 
                            Write-Host "  $_" -ForegroundColor Gray 
                        }
                        Write-Host ""
                    }
                }
                catch {
                    Write-Host "Unable to list directory contents." -ForegroundColor Gray
                }
   
                do {
                    $confirm = Read-Host "Are you sure you want to delete this folder? Type 'YES' to confirm or 'NO' to go back"
                
                    if ($confirm -eq "YES") {
                        try {
                            Write-Host "Deleting existing folder..." -ForegroundColor Yellow
                            Remove-Item $CurrentPath -Recurse -Force
                            Write-Host "✓ Folder deleted successfully" -ForegroundColor Green
                            return $CurrentPath
                        }
                        catch {
                            Write-Host "ERROR: Failed to delete folder: $($_.Exception.Message)" -ForegroundColor Red
                            Write-Host "Please try a different option." -ForegroundColor Yellow
                            break
                        }
                    }
                    elseif ($confirm -eq "NO") {
                        break
                    }
                    else {
                        Write-Host "Please type 'YES' or 'NO'" -ForegroundColor Red
                    }
                } while ($true)
            }
            "3" {
                Write-Host ""
                Write-Host "Installation cancelled by user." -ForegroundColor Yellow
                exit 0
            }
            default {
                Write-Host "Invalid choice. Please enter 1, 2, or 3." -ForegroundColor Red
            }
        }
    } while ($true)
}

# Function to validate installation path safety
function Test-InstallationPathSafe {
    param([string]$Path)
    
    # Expand and normalize the path
    $fullPath = [System.Environment]::ExpandEnvironmentVariables($Path)
    $fullPath = [System.IO.Path]::GetFullPath($fullPath)
    
    # Check for dangerous system paths
    $systemPaths = @(
        $env:SystemRoot,
        $env:ProgramFiles,
        $env:windir,
        "C:\",
        "C:\Windows",
        "C:\Users"
    )
    
    foreach ($sysPath in $systemPaths) {
        if ($fullPath -eq $sysPath) {
            Write-Host ""
            Write-Host "ERROR: Cannot install directly to system directory '$fullPath'" -ForegroundColor Red
            Write-Host "Please choose a subdirectory like '$fullPath\AppSage'" -ForegroundColor Yellow
            return $false
        }
    }
    
    return $true
}

# Function to get previous installation path from registry
function Get-PreviousInstallationPath {
    param([string]$Scope)
    
    try {
        $registryPath = if ($Scope -eq 'Machine') {
            "HKLM:\SOFTWARE\AppSage"
        } else {
            "HKCU:\SOFTWARE\AppSage"
        }
        
        if (Test-Path $registryPath) {
            $installPath = Get-ItemProperty -Path $registryPath -Name "InstallPath" -ErrorAction SilentlyContinue
            if ($installPath -and $installPath.InstallPath) {
                return $installPath.InstallPath
            }
        }
        
        return $null
    }
    catch {
        return $null
    }
}

# Function to set installation path in registry
function Set-InstallationPathRegistry {
    param(
        [string]$InstallPath,
        [string]$Scope
    )
    
    try {
        $registryPath = if ($Scope -eq 'Machine') {
            "HKLM:\SOFTWARE\AppSage"
        } else {
            "HKCU:\SOFTWARE\AppSage"
        }
        
        # Create registry key if it doesn't exist
        if (-not (Test-Path $registryPath)) {
            New-Item -Path $registryPath -Force | Out-Null
        }
        
        # Set the installation path
        Set-ItemProperty -Path $registryPath -Name "InstallPath" -Value $InstallPath
        Set-ItemProperty -Path $registryPath -Name "InstallDate" -Value (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
        Set-ItemProperty -Path $registryPath -Name "Scope" -Value $Scope
        
        Write-Host "  ✓ Installation registered in Windows Registry" -ForegroundColor Green
    }
    catch {
        Write-Host "WARNING: Failed to register installation in Registry: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Function to handle previous installation
function Test-PreviousInstallation {
    param([string]$Scope)
    
    $previousPath = Get-PreviousInstallationPath -Scope $Scope
    
    if ($previousPath) {
        Write-Host "Previous AppSage installation detected:" -ForegroundColor Yellow
        Write-Host "  Location: $previousPath" -ForegroundColor Gray
        Write-Host ""
        
        # Check if the previous installation still exists
        $appExe = Join-Path $previousPath "AppSage.exe"
        if (Test-Path $appExe) {
            Write-Host "Previous installation appears to be active." -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Choose an option:" -ForegroundColor Cyan
            Write-Host "  [1] Upgrade existing installation (recommended)" -ForegroundColor White
            Write-Host "  [2] Install to a different location" -ForegroundColor White
            Write-Host "  [3] Cancel installation" -ForegroundColor White
            Write-Host ""
            
            do {
                $choice = Read-Host "Enter your choice (1, 2, or 3)"
                
                switch ($choice) {
                    "1" {
                        Write-Host ""
                        Write-Host "Upgrading existing installation at: $previousPath" -ForegroundColor Green
                        return $previousPath
                    }
                    "2" {
                        Write-Host ""
                        Write-Host "Proceeding with new installation location..." -ForegroundColor Green
                        return $null
                    }
                    "3" {
                        Write-Host ""
                        Write-Host "Installation cancelled by user." -ForegroundColor Yellow
                        exit 0
                    }
                    default {
                        Write-Host "Invalid choice. Please enter 1, 2, or 3." -ForegroundColor Red
                    }
                }
            } while ($true)
        }
        else {
            Write-Host "Previous installation directory no longer exists or is incomplete." -ForegroundColor Gray
            Write-Host "Proceeding with new installation..." -ForegroundColor Green
            Write-Host ""
            return $null
        }
    }
    
    return $null
}

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
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  AppSage Installation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Verify AppSageApp.zip exists
if (-not (Test-Path $AppZipFile)) {
    Write-Host "ERROR: AppSageApp.zip not found!" -ForegroundColor Red
    Write-Host "Expected location: $AppZipFile" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure AppSageApp.zip is in the same directory as this script." -ForegroundColor Yellow
    exit 1
}

# Check for previous installation
Write-Host "Checking for previous installations..." -ForegroundColor Green
$previousInstallPath = Test-PreviousInstallation -Scope $Scope

# Use previous installation path if user chose to upgrade
if ($previousInstallPath) {
    $InstallPath = $previousInstallPath
    Write-Host ""
}

# Validate and get final installation path
Write-Host "Validating installation path..." -ForegroundColor Green
Write-Host ""

# Check if the provided path is safe
if (-not (Test-InstallationPathSafe $InstallPath)) {
    exit 1
}

# Check if directory is empty, prompt for new path if not
$finalInstallPath = $InstallPath
while (-not (Test-DirectoryEmpty $finalInstallPath)) {
    $finalInstallPath = Get-NewInstallPath $finalInstallPath
    
    # Re-validate the new path
    if (-not (Test-InstallationPathSafe $finalInstallPath)) {
        Write-Host "Please choose a different path." -ForegroundColor Yellow
        continue
    }
}

Write-Host "Install Location: " -NoNewline
Write-Host $finalInstallPath -ForegroundColor Yellow
Write-Host "Scope: " -NoNewline
Write-Host $Scope -ForegroundColor Yellow
Write-Host ""

# Step 1: Create installation directory
Write-Host "[1/3] Creating installation directory..." -ForegroundColor Green

try {
    if (-not (Test-Path $finalInstallPath)) {
        New-Item -ItemType Directory -Path $finalInstallPath -Force | Out-Null
    }
    Write-Host "  ✓ Directory ready: $finalInstallPath" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to create installation directory." -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 2: Extract application files
Write-Host "[2/3] Extracting application files..." -ForegroundColor Green

try {
    Expand-Archive -Path $AppZipFile -DestinationPath $finalInstallPath -Force
    Write-Host "  ✓ Files extracted" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to extract application files." -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Add to PATH (optional)
if ($AddToPath) {
    Write-Host "[3/3] Adding to PATH..." -ForegroundColor Green
    
    try {
        $target = if ($Scope -eq 'Machine') { [EnvironmentVariableTarget]::Machine } else { [EnvironmentVariableTarget]::User }
        
        $currentPath = [Environment]::GetEnvironmentVariable('PATH', $target)
        
        # Check if already in PATH
        $pathDirs = $currentPath -split ';' | Where-Object { $_ -ne '' }
        $alreadyInPath = $pathDirs | Where-Object { $_ -eq $finalInstallPath }
        
        if ($alreadyInPath) {
            Write-Host "  Already in PATH" -ForegroundColor DarkGray
        }
        else {
            $newPath = $currentPath.TrimEnd(';') + ';' + $finalInstallPath
            [Environment]::SetEnvironmentVariable('PATH', $newPath, $target)
            Write-Host "  ✓ Added to $Scope PATH" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "WARNING: Failed to update PATH environment variable." -ForegroundColor Yellow
        Write-Host "You may need to add '$finalInstallPath' to your PATH manually." -ForegroundColor Yellow
    }
    
    Write-Host ""
}

# Verify installation
$appExe = Join-Path $finalInstallPath "AppSage.exe"
if (Test-Path $appExe) {
    Write-Host "✓ Installation verified successfully" -ForegroundColor Green
    
    # Register installation in Windows Registry
    Set-InstallationPathRegistry -InstallPath $finalInstallPath -Scope $Scope
}
else {
    Write-Host "WARNING: AppSage.exe not found in installation directory" -ForegroundColor Yellow
    Write-Host "Installation may be incomplete." -ForegroundColor Yellow
}
Write-Host ""

# Installation complete
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  INSTALLATION COMPLETE!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "AppSage has been installed to:" -ForegroundColor White
Write-Host "  $finalInstallPath" -ForegroundColor Yellow
Write-Host ""

if ($AddToPath -and (Test-Path $appExe)) {
    Write-Host "You can now run AppSage from any command prompt:" -ForegroundColor White
    Write-Host "  appsage --help" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Note: You may need to restart your terminal for PATH changes to take effect." -ForegroundColor Yellow
}
else {
    Write-Host "To run AppSage, use the full path:" -ForegroundColor White
    Write-Host "  `"$finalInstalledPath\appsage.exe`"" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
