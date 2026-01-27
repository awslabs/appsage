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
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [string]$VSCodeExtensionVersion
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

# Function to build AppSage .NET application
function Build-AppSageDotNetApp {
    param(
        [string]$Configuration,
        [string]$PublishWorkspace,
        [string]$AppZipFile
    )
    
    # Derive .NET-related paths within the function
    $ScriptRoot = $PSScriptRoot
    $SolutionRoot = Split-Path $ScriptRoot -Parent
    $AppSageRunProject = Join-Path $SolutionRoot "AppSage.Run\AppSage.Run.csproj"
    $PublishOutput = Join-Path $SolutionRoot "AppSage.Run\bin\Publish\AppSageApp"
    
    # Step 1: Clean old files
    Write-Host "[1/6] Cleaning old .NET app files..." -ForegroundColor Green
    if (Test-Path $PublishOutput) {
        Remove-Item $PublishOutput -Recurse -Force
    }
    if (Test-Path $AppZipFile) {
        Remove-Item $AppZipFile -Force
    }
    Write-Host "  ✓ .NET app files cleaned" -ForegroundColor Green
    Write-Host ""

    # Step 2: Clean solution build artifacts
    Write-Host "[2/6] Cleaning solution build artifacts..." -ForegroundColor Green
    & dotnet clean "`"$SolutionRoot`"" --configuration $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Clean failed, continuing anyway..."
    }
    Write-Host "  ✓ Solution cleaned" -ForegroundColor Green
    Write-Host ""

    # Step 3: Ensure publish directory exists
    Write-Host "[3/6] Creating publish directories..." -ForegroundColor Green
    New-Item -Path $PublishWorkspace -ItemType Directory -Force | Out-Null
    Write-Host "  ✓ Publish directories created" -ForegroundColor Green
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

    # Step 5: Create AppSageApp.zip
    Write-Host "[5/6] Creating .NET app package..." -ForegroundColor Green

    # Create zip of the published app
    Compress-Archive -Path "$PublishOutput\*" -DestinationPath $AppZipFile -Force

    $appZipSize = (Get-Item $AppZipFile).Length / 1MB
    Write-Host "  ✓ .NET app package created ($([math]::Round($appZipSize, 2)) MB)" -ForegroundColor Green
    Write-Host ""
}

# Function to copy install script
function Copy-InstallScript {
    param(
        [string]$InstallScriptCopy
    )
    
    # Derive install script source path
    $ScriptRoot = $PSScriptRoot
    $InstallScript = Join-Path $ScriptRoot "Install-AppSage.ps1"
    
    # Clean old install script copy
    if (Test-Path $InstallScriptCopy) {
        Remove-Item $InstallScriptCopy -Force
    }

    # Copy the install script
    if (Test-Path $InstallScript) {
        Copy-Item $InstallScript -Destination $InstallScriptCopy
        Write-Host "  ✓ Install script copied" -ForegroundColor Green
    } else {
        Write-Warning "Install-AppSage.ps1 not found at $InstallScript"
    }
}

# Function to build and package VSCode extension
function Build-AppSageVSCodeExtension {
    param(
        [string]$Configuration,
        [string]$VSCodeExtensionCopy,
        [string]$Version
    )
    
    # Derive VSCode extension paths
    $ScriptRoot = $PSScriptRoot
    $SolutionRoot = Split-Path $ScriptRoot -Parent
    $VSCodeExtensionRoot = Join-Path $SolutionRoot "AppSage.VSCodeExtension"
    $BuildScript = Join-Path $VSCodeExtensionRoot "BuildVSCodeExtension.ps1"
    $ReleaseFolder = Join-Path $VSCodeExtensionRoot "release"
    
    # Clean old VSCode extension copy
    if (Test-Path $VSCodeExtensionCopy) {
        Remove-Item $VSCodeExtensionCopy -Force
    }

    # Check if build script exists
    if (-not (Test-Path $BuildScript)) {
        Write-Warning "VSCode extension build script not found at $BuildScript"
        return
    }

    # Run the VSCode extension build script
    Write-Host "[VSCode] Building and packaging VSCode extension..." -ForegroundColor Magenta
    
    try {
        # Save current location
        $currentLocation = Get-Location
        
        # Call the build script in the same PowerShell process
        if ($Version) {
            & $BuildScript -Version $Version
        } else {
            & $BuildScript
        }
        
        # Restore location in case the script changed it
        Set-Location $currentLocation
        
        if ($LASTEXITCODE -ne 0) {
            throw "VSCode extension build failed"
        }
        
        # Find the VSIX file in the release folder
        $vsixFiles = Get-ChildItem -Path $ReleaseFolder -Filter "*.vsix" -ErrorAction SilentlyContinue
        
        if ($vsixFiles -and $vsixFiles.Count -gt 0) {
            # Use the first (or only) VSIX file found
            $vsixPath = $vsixFiles[0].FullName
            
            # Copy the VSIX file to the workspace
            Copy-Item $vsixPath -Destination $VSCodeExtensionCopy
            $vsixSize = (Get-Item $VSCodeExtensionCopy).Length / 1KB
            Write-Host "  ✓ VSCode extension copied ($([math]::Round($vsixSize, 2)) KB)" -ForegroundColor Magenta
        } else {
            Write-Warning "No VSIX file found in $ReleaseFolder"
        }
    }
    catch {
        Write-Warning "Failed to build VSCode extension: $($_.Exception.Message)"
    }
}

# Paths (only for installer-specific files)
$ScriptRoot = $PSScriptRoot
$SolutionRoot = Split-Path $ScriptRoot -Parent
$FinalInstallerDir = Join-Path $ScriptRoot "Publish"
$InstallerZip = Join-Path $FinalInstallerDir "AppSageInstaller.zip"

# Temporary paths for installer creation
$PublishWorkspace = Join-Path $SolutionRoot "AppSage.Run\bin\Publish"
$AppZipFile = Join-Path $PublishWorkspace "AppSageApp.zip"
$InstallScriptCopy = Join-Path $PublishWorkspace "Install-AppSage.ps1"
$VSCodeExtensionCopy = Join-Path $PublishWorkspace "AppSageVSCodeExtension.vsix"

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

# Step 1-5: Build AppSage .NET Application
Build-AppSageDotNetApp -Configuration $Configuration -PublishWorkspace $PublishWorkspace -AppZipFile $AppZipFile

# Step 5.5: Build AppSage VSCode Extension
Build-AppSageVSCodeExtension -Configuration $Configuration -VSCodeExtensionCopy $VSCodeExtensionCopy -Version $VSCodeExtensionVersion

# Step 6: Copy install script and create final installer
Write-Host "[6/6] Creating final installer package..." -ForegroundColor Green

# Ensure final installer directory exists
New-Item -Path $FinalInstallerDir -ItemType Directory -Force | Out-Null

# Clean old installer zip
if (Test-Path $InstallerZip) {
    Remove-Item $InstallerZip -Force
}

# Copy install script to workspace
Copy-InstallScript -InstallScriptCopy $InstallScriptCopy

# Create final installer zip
$itemsToZip = @()
if (Test-Path $InstallScriptCopy) {
    $itemsToZip += $InstallScriptCopy
}
$itemsToZip += $AppZipFile
if (Test-Path $VSCodeExtensionCopy) {
    $itemsToZip += $VSCodeExtensionCopy
}

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
if (Test-Path $VSCodeExtensionCopy) {
    Write-Host "VSCode Extension: " -NoNewline
    Write-Host $VSCodeExtensionCopy -ForegroundColor Yellow
}
Write-Host "Final Installer: " -NoNewline
Write-Host $InstallerZip -ForegroundColor Yellow
Write-Host "Installer Size: " -NoNewline
Write-Host "$([math]::Round($installerZipSize, 2)) MB" -ForegroundColor Yellow
Write-Host ""