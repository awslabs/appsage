#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Cleans .NET build artifacts and Node.js dependencies from the workspace.

.DESCRIPTION
    This script removes the following directories:
    - bin/ folders (containing compiled .NET artifacts)
    - obj/ folders (containing intermediate build files)
    - node_modules/ folders (containing Node.js dependencies)

.PARAMETER Path
    The root path to clean. Defaults to the current directory.

.PARAMETER WhatIf
    Shows what would be deleted without actually deleting anything.

.PARAMETER Verbose
    Shows detailed information about what is being deleted.

.EXAMPLE
    .\clean-build-artifacts.ps1
    Cleans the current directory and all subdirectories.

.EXAMPLE
    .\clean-build-artifacts.ps1 -WhatIf
    Shows what would be deleted without actually deleting.

.EXAMPLE
    .\clean-build-artifacts.ps1 -Path "C:\MyProject" -Verbose
    Cleans the specified path with verbose output.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory = $false)]
    [string]$Path = (Get-Location).Path
)

# Set verbose preference based on common parameter
# The -Verbose switch is automatically available due to [CmdletBinding()]

# Ensure the path exists
if (-not (Test-Path $Path)) {
    Write-Error "Path '$Path' does not exist."
    exit 1
}

Write-Host "Cleaning build artifacts in: $Path" -ForegroundColor Cyan
Write-Host ""

# Define patterns to search for
$foldersToClean = @("bin", "obj", "node_modules")

# Initialize counters
$totalFoldersFound = 0
$totalFoldersDeleted = 0
$totalSizeFreed = 0

foreach ($folderPattern in $foldersToClean) {
    Write-Host "Looking for '$folderPattern' folders..." -ForegroundColor Yellow
    
    # Find all directories matching the pattern
    $foldersFound = Get-ChildItem -Path $Path -Recurse -Directory -Name $folderPattern -ErrorAction SilentlyContinue
    
    if ($foldersFound) {
        $folderCount = $foldersFound.Count
        $totalFoldersFound += $folderCount
        
        Write-Host "   Found $folderCount '$folderPattern' folder(s)" -ForegroundColor Green
        
        foreach ($folder in $foldersFound) {
            $fullPath = Join-Path $Path $folder
            
            if (Test-Path $fullPath) {
                # Calculate folder size before deletion
                try {
                    $folderSize = (Get-ChildItem -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue | 
                                  Measure-Object -Property Length -Sum -ErrorAction SilentlyContinue).Sum
                    
                    if ($null -eq $folderSize) { $folderSize = 0 }
                    
                    $sizeInMB = [math]::Round($folderSize / 1MB, 2)
                    
                    if ($WhatIfPreference) {
                        Write-Host "   [WHAT-IF] Would delete: $fullPath ($sizeInMB MB)" -ForegroundColor Magenta
                    } else {
                        Write-Verbose "   Deleting: $fullPath ($sizeInMB MB)"
                        
                        # Remove the directory and all its contents
                        if ($PSCmdlet.ShouldProcess($fullPath, "Delete directory")) {
                            Remove-Item -Path $fullPath -Recurse -Force -ErrorAction Continue
                            
                            if (-not (Test-Path $fullPath)) {
                                $totalFoldersDeleted++
                                $totalSizeFreed += $folderSize
                                Write-Host "   Success: Deleted $fullPath ($sizeInMB MB)" -ForegroundColor Green
                            } else {
                                Write-Warning "   Failed to delete: $fullPath"
                            }
                        }
                    }
                } catch {
                    Write-Warning "   Error processing $fullPath : $($_.Exception.Message)"
                }
            }
        }
    } else {
        Write-Host "   No '$folderPattern' folders found" -ForegroundColor Gray
    }
    
    Write-Host ""
}

# Summary
$totalSizeFreedMB = [math]::Round($totalSizeFreed / 1MB, 2)
$totalSizeFreedGB = [math]::Round($totalSizeFreed / 1GB, 2)

Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "   Total folders found: $totalFoldersFound" -ForegroundColor White

if ($WhatIfPreference) {
    Write-Host "   [WHAT-IF] Would delete $totalFoldersFound folder(s)" -ForegroundColor Magenta
    Write-Host "   [WHAT-IF] Would free approximately $totalSizeFreedMB MB ($totalSizeFreedGB GB)" -ForegroundColor Magenta
} else {
    Write-Host "   Folders successfully deleted: $totalFoldersDeleted" -ForegroundColor Green
    Write-Host "   Disk space freed: $totalSizeFreedMB MB ($totalSizeFreedGB GB)" -ForegroundColor Green
    
    if ($totalFoldersDeleted -lt $totalFoldersFound) {
        $failedCount = $totalFoldersFound - $totalFoldersDeleted
        Write-Host "   Failed to delete: $failedCount folder(s)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Cleanup completed!" -ForegroundColor Cyan
