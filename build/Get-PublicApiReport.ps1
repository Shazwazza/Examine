<#
.SYNOPSIS
    Generates a breaking changes report by analyzing PublicAPI files.

.DESCRIPTION
    This script analyzes PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt files to generate
    a human-readable markdown report of API changes. It identifies:
    - New APIs added (in Unshipped)
    - Removed APIs (breaking changes - marked with *REMOVED*)
    - Modified APIs (breaking changes - signature changes)
    
    The report is saved as a markdown file for documentation purposes.

.PARAMETER SourcePath
    The root path to search for PublicAPI files. Defaults to "../src" relative to script location.

.PARAMETER OutputPath
    The path where the markdown report will be saved. Defaults to "../PublicAPI-Changes.md"

.EXAMPLE
    .\Get-PublicApiReport.ps1
    Generates report with default paths.

.EXAMPLE
    .\Get-PublicApiReport.ps1 -OutputPath "C:\Reports\api-changes.md"
    Generates report at specified location.

.NOTES
    Run this script to document API changes before a release.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$SourcePath,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath
)

# Ensure UTF-8 encoding for emojis/Unicode
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

# IMPORTANT: Avoid emoji literals directly in the script file.
# Windows PowerShell 5.1 can read .ps1 files as ANSI when there's no BOM, which corrupts emojis.
# Build emojis/arrows from codepoints so they are always correct at runtime.
$EmojiWarning = ([string][char]0x26A0) + ([string][char]0xFE0F) # ⚠️ (warning + variation selector)
$EmojiCheck   = ([string][char]0x2705)                          # ✅
$Arrow        = ([string][char]0x2192)                          # →

# Set default paths if not provided
if ([string]::IsNullOrEmpty($SourcePath)) {
    if ([string]::IsNullOrEmpty($PSScriptRoot)) {
        $SourcePath = Join-Path (Get-Location) "src"
    } else {
        $SourcePath = Join-Path $PSScriptRoot "..\src"
    }
}

if ([string]::IsNullOrEmpty($OutputPath)) {
    if ([string]::IsNullOrEmpty($PSScriptRoot)) {
        $OutputPath = Join-Path (Get-Location) "PublicAPI-Changes.md"
    } else {
        $OutputPath = Join-Path $PSScriptRoot "..\PublicAPI-Changes.md"
    }
}

# Function to format API signatures in a human-readable way
function Format-ApiSignature {
    param([string]$Api)
    
    # Remove *REMOVED* prefix if present
    $cleanApi = $Api -replace '^\*REMOVED\*\s*', ''
    
    # Parse different API types for better formatting
    
    # Constants
    if ($cleanApi -match '^const\s+(.+?)\s*=\s*(.+?)\s*->\s*(.+)$') {
        $name = $matches[1]
        $value = $matches[2]
        $type = $matches[3]
        return "- **Constant**: ``$name`` = $value $Arrow *$type*"
    }
    
    # Abstract methods
    if ($cleanApi -match '^abstract\s+(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        $returnType = $matches[2]
        return "- **Abstract**: ``$signature`` $Arrow *$returnType*"
    }
    
    # Virtual methods
    if ($cleanApi -match '^virtual\s+(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        $returnType = $matches[2]
        return "- **Virtual**: ``$signature`` $Arrow *$returnType*"
    }
    
    # Override methods
    if ($cleanApi -match '^override\s+(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        $returnType = $matches[2]
        return "- **Override**: ``$signature`` $Arrow *$returnType*"
    }
    
    # Static members
    if ($cleanApi -match '^static\s+(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        $returnType = $matches[2]
        return "- **Static**: ``$signature`` $Arrow *$returnType*"
    }
    
    # Properties (with .get or .set)
    if ($cleanApi -match '^(.+?\.(?:get|set))\s*->\s*(.+)$') {
        $propAccess = $matches[1]
        $type = $matches[2]
        return "- **Property**: ``$propAccess`` $Arrow *$type*"
    }
    
    # Regular members/methods
    if ($cleanApi -match '^(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        $returnType = $matches[2]
        
        # Check if it's a constructor (type name matches method name)
        if ($signature -match '(\w+)\.(\w+)\([^)]*\)' -and $matches[1] -eq $matches[2]) {
            return "- **Constructor**: ``$signature``"
        }
        
        return "- **Member**: ``$signature`` $Arrow *$returnType*"
    }
    
    # Enum values
    if ($cleanApi -match '^(.+?)\s*=\s*(\d+)\s*->\s*(.+)$') {
        $name = $matches[1]
        $value = $matches[2]
        $type = $matches[3]
        return "- **Enum**: ``$name`` = $value $Arrow *$type*"
    }
    
    # Type declarations (just the type name)
    if ($cleanApi -notmatch '->') {
        return "- **Type**: ``$cleanApi``"
    }
    
    # Fallback - just wrap in code
    return "- ``$cleanApi``"
}

# Ensure the source path exists
if (-not (Test-Path $SourcePath)) {
    Write-Error "Source path does not exist: $SourcePath"
    exit 1
}

Write-Host "Analyzing PublicAPI files in: $SourcePath" -ForegroundColor Cyan
Write-Host ""

# Initialize collections for tracking changes
$allNewApis = @()
$allProjects = @()

# Find all PublicAPI.Unshipped.txt files
$unshippedFiles = Get-ChildItem -Path $SourcePath -Recurse -Filter "PublicAPI.Unshipped.txt"

if ($unshippedFiles.Count -eq 0) {
    Write-Warning "No PublicAPI.Unshipped.txt files found in $SourcePath"
    exit 0
}

Write-Host "Found $($unshippedFiles.Count) project(s) with PublicAPI tracking" -ForegroundColor Green
Write-Host ""

foreach ($unshippedFile in $unshippedFiles) {
    $unshippedPath = $unshippedFile.FullName
    $shippedPath = $unshippedPath.Replace("Unshipped", "Shipped")
    $projectName = $unshippedFile.Directory.Name
    
    Write-Host "Analyzing: $projectName" -ForegroundColor Yellow
    
    if (-not (Test-Path $shippedPath)) {
        Write-Warning "  Corresponding Shipped file not found, skipping."
        continue
    }
    
    try {
        # Read files
        $unshippedContent = Get-Content $unshippedPath | Where-Object { 
            $_.Trim() -ne "" -and $_.Trim() -ne "#nullable enable" 
        }
        
        $shippedContent = Get-Content $shippedPath | Where-Object { 
            $_.Trim() -ne "" -and $_.Trim() -ne "#nullable enable" 
        }
        
        # Categorize APIs
        $newApis = @()
        $removedApis = @()
        
        foreach ($api in $unshippedContent) {
            $apiTrimmed = $api.Trim()
            if ($apiTrimmed -eq "") { continue }
            
            # Check if this is a removed API (breaking change)
            if ($apiTrimmed.StartsWith("*REMOVED*")) {
                $removedApis += $api
            }
            # Check if this is truly new (not in shipped)
            elseif ($shippedContent -notcontains $api) {
                $newApis += $api
            }
        }
        
        if ($newApis.Count -gt 0 -or $removedApis.Count -gt 0) {
            $projectInfo = @{
                Name = $projectName
                NewApis = $newApis
                RemovedApis = $removedApis
            }
            $allProjects += $projectInfo
            
            if ($newApis.Count -gt 0) {
                Write-Host "  Found $($newApis.Count) new API(s)" -ForegroundColor Green
            }
            if ($removedApis.Count -gt 0) {
                Write-Host "  Found $($removedApis.Count) removed API(s) [BREAKING]" -ForegroundColor Red
            }
        } else {
            Write-Host "  No API changes" -ForegroundColor Gray
        }
        
    }
    catch {
        Write-Error "  Error analyzing files: $_"
    }
    
    Write-Host ""
}

# Generate markdown report
Write-Host "Generating markdown report..." -ForegroundColor Cyan

$markdown = @"
# Public API Changes Report

Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Summary

"@

if ($allProjects.Count -eq 0) {
    $markdown += @"

**No API changes detected** - All Unshipped files are empty or only contain items already in Shipped files.

"@
} else {
    $totalNewApis = ($allProjects | ForEach-Object { $_.NewApis.Count } | Measure-Object -Sum).Sum
    $totalRemovedApis = ($allProjects | ForEach-Object { $_.RemovedApis.Count } | Measure-Object -Sum).Sum
    $hasBreakingChanges = $totalRemovedApis -gt 0
    
    $markdown += @"

- **Projects with changes:** $($allProjects.Count)
- **Total new APIs:** $totalNewApis
- **Total removed APIs:** $totalRemovedApis $(if ($hasBreakingChanges) { "$EmojiWarning **BREAKING CHANGES**" } else { "" })

"@

    # Group projects by whether they have breaking changes
    $projectsWithBreaking = $allProjects | Where-Object { $_.RemovedApis.Count -gt 0 }
    $projectsWithAdditions = $allProjects | Where-Object { $_.NewApis.Count -gt 0 }
    
    # Show breaking changes first if any exist
    if ($projectsWithBreaking.Count -gt 0) {
        $markdown += @"

## $EmojiWarning Breaking Changes

The following projects have **removed APIs** which constitute breaking changes:

"@
        foreach ($project in $projectsWithBreaking) {
           $markdown += "`n### $($project.Name)`n`n"
           $markdown += "#### Removed APIs ($($project.RemovedApis.Count)) - BREAKING`n`n"
           $markdown += "The following public APIs have been **removed**:`n`n"
           
           foreach ($api in $project.RemovedApis) {
               $formattedApi = Format-ApiSignature $api
               $markdown += "$formattedApi`n"
           }
           
           $markdown += "`n"
       }
    }
    
    # Show additions
    if ($projectsWithAdditions.Count -gt 0) {
        $markdown += @"

## $EmojiCheck New APIs (Non-Breaking)

The following projects have **new APIs** added:

"@
        foreach ($project in $projectsWithAdditions) {
           if ($project.NewApis.Count -eq 0) { continue }
           
           $markdown += "`n### $($project.Name)`n`n"
           $markdown += "#### New APIs ($($project.NewApis.Count))`n`n"
           $markdown += "The following public APIs have been added:`n`n"
           
           foreach ($api in $project.NewApis) {
               $formattedApi = Format-ApiSignature $api
               $markdown += "$formattedApi`n"
           }
           
           $markdown += "`n"
       }
    }
    
    $markdown += @"

## Summary

### $EmojiCheck Additions (Non-Breaking)
$totalNewApis new API(s) have been added. These are **safe changes** that do not break existing code.

### $EmojiWarning Breaking Changes
"@

    if ($hasBreakingChanges) {
        $markdown += @"
$totalRemovedApis API(s) have been **removed**. These are **BREAKING CHANGES** that will require:
- Major version bump (e.g., 3.x $Arrow 4.0)
- Migration guide for consumers
- Release notes highlighting the breaking changes

"@
    } else {
        $markdown += @"
No breaking changes detected. All existing shipped APIs remain unchanged.

"@
    }
}

$markdown += @"

## Next Steps

Before releasing:

1. Review all new APIs for:
   - Naming consistency
   - XML documentation completeness
   - Design patterns alignment
   - Backward compatibility

2. Update release notes with these API changes

3. Run the build to ensure no analyzer warnings (RS0016, RS0017)

4. After release, run ``.\build\Merge-PublicApiFiles.ps1`` to move Unshipped $Arrow Shipped

---

*Generated by Get-PublicApiReport.ps1*
"@

# Write report to file as UTF-8 with BOM (best compatibility for Windows + emojis)
$utf8Bom = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($OutputPath, $markdown, $utf8Bom)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Report saved to: $OutputPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

exit 0