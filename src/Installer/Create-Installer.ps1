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

# Function to check .NET version (minimum version or higher)
function Test-DotNetVersion {
    param(
        [string]$MinimumVersion = "8.0",
        [switch]$ExactMatch = $false
    )
    
    try {
        $sdks = & dotnet --list-sdks 2>$null
        if ($LASTEXITCODE -ne 0) {
    Write-Warning ".NET CLI not found"
          return $false
        }
        
        if ($sdks -eq $null -or $sdks.Count -eq 0) {
     Write-Warning "No .NET SDKs found"
          return $false
 }
        
        # Parse the minimum version
        $minVersion = [System.Version]::Parse($MinimumVersion)
 
        # Find compatible versions
        $compatibleVersions = @()
        
    foreach ($sdk in $sdks) {
            # Extract version from SDK string (format: "8.0.100 [path]")
  $versionMatch = $sdk -match '^(\d+\.\d+\.\d+)'
    if ($versionMatch) {
       $sdkVersion = [System.Version]::Parse($matches[1])
                
      if ($ExactMatch) {
       # For exact match, compare major.minor only
 if ($sdkVersion.Major -eq $minVersion.Major -and $sdkVersion.Minor -eq $minVersion.Minor) {
  $compatibleVersions += $sdk
              }
                } else {
       # For minimum version, check if SDK version >= minimum version
      if ($sdkVersion -ge $minVersion) {
             $compatibleVersions += $sdk
          }
          }
            }
        }
        
        if ($compatibleVersions.Count -gt 0) {
    if ($ExactMatch) {
            Write-Host "  ✓ .NET $MinimumVersion SDK found: $($compatibleVersions[0])" -ForegroundColor Green
            } else {
   Write-Host "  ✓ .NET $MinimumVersion+ SDK found: $($compatibleVersions[0])" -ForegroundColor Green
  if ($compatibleVersions.Count -gt 1) {
         Write-Host "    Additional compatible versions: $($compatibleVersions[1..($compatibleVersions.Count-1)] -join ', ')" -ForegroundColor Gray
         }
  }
   return $true
    } else {
          if ($ExactMatch) {
          Write-Warning ".NET $MinimumVersion SDK not found"
            } else {
 Write-Warning ".NET $MinimumVersion or higher SDK not found"
      }
    Write-Host "Available SDKs:" -ForegroundColor Yellow
 $sdks | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
            return $false
        }
    }
    catch {
        Write-Error "Error checking .NET version: $($_.Exception.Message)"
        return $false
    }
}

# Paths
$ScriptRoot = $PSScriptRoot
$SolutionRoot = Split-Path $ScriptRoot -Parent
$AppSageRunProject = Join-Path $SolutionRoot "AppSage.Run\AppSage.Run.csproj"
$PublishOutput = Join-Path $SolutionRoot "AppSage.Run\bin\Publish\AppSageApp"
$PublishBaseDir = Join-Path $SolutionRoot "AppSage.Run\bin\Publish"
$AppZipFile = Join-Path $PublishBaseDir "AppSageApp.zip"
$InstallScript = Join-Path $ScriptRoot "Install-AppSage.ps1"
$InstallScriptCopy = Join-Path $PublishBaseDir "Install-AppSage.ps1"
$FinalInstallerDir = Join-Path $ScriptRoot "Publish"
$InstallerZip = Join-Path $FinalInstallerDir "AppSageInstaller.zip"

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Building AppSage Package" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Step 0: Check .NET prerequisites
Write-Host "[0/6] Checking .NET prerequisites..." -ForegroundColor Green
$dotNetVersionRequired= "10.0"
if (-not (Test-DotNetVersion -MinimumVersion $dotNetVersionRequired)) {
    throw ".NET $dotNetVersionRequired or higher SDK is required but not found. Please install .NET $dotNetVersionRequired+ x64 SDK from https://dotnet.microsoft.com/download"
}
Write-Host ""

# Step 1: Clean old files
Write-Host "[1/6] Cleaning old files..." -ForegroundColor Green
if (Test-Path $PublishOutput) {
 Remove-Item $PublishOutput -Recurse -Force
}
if (Test-Path $AppZipFile) {
    Remove-Item $AppZipFile -Force
}
if (Test-Path $InstallScriptCopy) {
    Remove-Item $InstallScriptCopy -Force
}
if (Test-Path $FinalInstallerDir) {
    Remove-Item $FinalInstallerDir -Recurse -Force
}
Write-Host "  ✓ Cleaned" -ForegroundColor Green
Write-Host ""

# Step 2: Clean solution build artifacts
Write-Host "[2/6] Cleaning solution build artifacts..." -ForegroundColor Green
& dotnet clean "`"$SolutionRoot`"" --configuration $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Clean failed, continuing anyway..."
}
Write-Host "  ✓ Solution cleaned" -ForegroundColor Green
Write-Host ""

# Step 3: Ensure directories exist
Write-Host "[3/6] Creating directories..." -ForegroundColor Green
New-Item -Path $PublishBaseDir -ItemType Directory -Force | Out-Null
New-Item -Path $FinalInstallerDir -ItemType Directory -Force | Out-Null
Write-Host "  ✓ Directories created" -ForegroundColor Green
Write-Host ""

# Step 4: Publish AppSage.Run
Write-Host "[4/6] Publishing AppSage ($Configuration)..." -ForegroundColor Green

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

# Step 5: Create AppSageApp.zip and copy Install-AppSage.ps1
Write-Host "[5/6] Creating app package and copying install script..." -ForegroundColor Green

# Create zip of the published app
Compress-Archive -Path "$PublishOutput\*" -DestinationPath $AppZipFile -Force

# Copy the install script
if (Test-Path $InstallScript) {
    Copy-Item $InstallScript -Destination $InstallScriptCopy
    Write-Host "  ✓ Install script copied" -ForegroundColor Green
} else {
    Write-Warning "Install-AppSage.ps1 not found at $InstallScript"
}

$appZipSize = (Get-Item $AppZipFile).Length / 1MB
Write-Host "  ✓ App package created ($([math]::Round($appZipSize, 2)) MB)" -ForegroundColor Green
Write-Host ""

# Step 6: Create final installer zip
Write-Host "[6/6] Creating final installer package..." -ForegroundColor Green

# Compress both Install-AppSage.ps1 and AppSageApp.zip into the final installer
$itemsToZip = @()
if (Test-Path $InstallScriptCopy) {
    $itemsToZip += $InstallScriptCopy
}
$itemsToZip += $AppZipFile

Compress-Archive -Path $itemsToZip -DestinationPath $InstallerZip -Force

$installerZipSize = (Get-Item $InstallerZip).Length / 1MB

Write-Host "  ✓ Final installer created" -ForegroundColor Green
Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  BUILD COMPLETE!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "App Package: " -NoNewline
Write-Host $AppZipFile -ForegroundColor Yellow
Write-Host "Final Installer: " -NoNewline
Write-Host $InstallerZip -ForegroundColor Yellow
Write-Host "Installer Size: " -NoNewline
Write-Host "$([math]::Round($installerZipSize, 2)) MB" -ForegroundColor Yellow
Write-Host ""