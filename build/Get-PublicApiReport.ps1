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

function Get-ApiKind {
    param([string]$Api)

    $cleanApi = $Api -replace '^\*REMOVED\*\s*', ''

    if ($cleanApi -match '^const\s+') { return 'Constant' }
    if ($cleanApi -match '^abstract\s+') { return 'Abstract' }
    if ($cleanApi -match '^virtual\s+') { return 'Virtual' }
    if ($cleanApi -match '^override\s+') { return 'Override' }
    if ($cleanApi -match '^static\s+') { return 'Static' }

    # Enum values look like: Namespace.Enum.Value = 0 -> Namespace.Enum
    if ($cleanApi -match '^.+?\s*=\s*\d+\s*->\s*.+$') { return 'Enum' }

    # Properties look like: Type.Member.get -> ReturnType
    if ($cleanApi -match '^.+?\.(get|set)\s*->\s*.+$') { return 'Property' }

    # Type declarations are lines with no arrow at all
    if ($cleanApi -notmatch '->') { return 'Type' }

    # Constructors look like: Namespace.Type.Type(...) -> void
    if ($cleanApi -match '^(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        if ($signature -match '(\w+)\.(\w+)\([^)]*\)' -and $matches[1] -eq $matches[2]) {
            return 'Constructor'
        }
    }

    return 'Member'
}

function Format-ApiSignature {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Api,

        # When output is already grouped by kind, omit the redundant "**Kind**:" label.
        [switch]$OmitKindLabel
    )

    $cleanApi = $Api -replace '^\*REMOVED\*\s*', ''
    $kind = Get-ApiKind -Api $Api

    # Constants
    if ($cleanApi -match '^const\s+(.+?)\s*=\s*(.+?)\s*->\s*(.+)$') {
        $name = $matches[1]
        $value = $matches[2]
        $type = $matches[3]
        if ($OmitKindLabel) { return "- ``$name`` = $value $Arrow *$type*" }
        return "- **$kind**: ``$name`` = $value $Arrow *$type*"
    }

    # Methods/members with explicit return types
    if ($cleanApi -match '^(abstract|virtual|override|static)\s+(.+?)\s*->\s*(.+)$') {
        $signature = $matches[2]
        $returnType = $matches[3]
        if ($OmitKindLabel) { return "- ``$signature`` $Arrow *$returnType*" }
        return "- **$kind**: ``$signature`` $Arrow *$returnType*"
    }

    # Properties
    if ($cleanApi -match '^(.+?\.(?:get|set))\s*->\s*(.+)$') {
        $propAccess = $matches[1]
        $type = $matches[2]
        if ($OmitKindLabel) { return "- ``$propAccess`` $Arrow *$type*" }
        return "- **$kind**: ``$propAccess`` $Arrow *$type*"
    }

    # Generic member (method/operator/etc)
    if ($cleanApi -match '^(.+?)\s*->\s*(.+)$') {
        $signature = $matches[1]
        $returnType = $matches[2]

        if ($kind -eq 'Constructor') {
            if ($OmitKindLabel) { return "- ``$signature``" }
            return "- **Constructor**: ``$signature``"
        }

        if ($OmitKindLabel) { return "- ``$signature`` $Arrow *$returnType*" }
        return "- **$kind**: ``$signature`` $Arrow *$returnType*"
    }

    # Enum values
    if ($cleanApi -match '^(.+?)\s*=\s*(\d+)\s*->\s*(.+)$') {
        $name = $matches[1]
        $value = $matches[2]
        $type = $matches[3]
        if ($OmitKindLabel) { return "- ``$name`` = $value $Arrow *$type*" }
        return "- **Enum**: ``$name`` = $value $Arrow *$type*"
    }

    # Type declarations
    if ($cleanApi -notmatch '->') {
        if ($OmitKindLabel) { return "- ``$cleanApi``" }
        return "- **Type**: ``$cleanApi``"
    }

    return "- ``$cleanApi``"
}

function Get-KindOrder {
    param([string]$Kind)
    switch ($Kind) {
        'Type' { return 0 }
        'Constant' { return 1 }
        'Enum' { return 2 }
        'Constructor' { return 3 }
        'Property' { return 4 }
        'Abstract' { return 5 }
        'Virtual' { return 6 }
        'Override' { return 7 }
        'Static' { return 8 }
        default { return 9 } # Member
    }
}

function Group-ApisByKind {
    param([string[]]$Apis)

    $groups = @{}
    foreach ($api in ($Apis | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })) {
        $k = Get-ApiKind -Api $api
        if (-not $groups.ContainsKey($k)) { $groups[$k] = New-Object System.Collections.Generic.List[string] }
        $groups[$k].Add($api)
    }

    return $groups
}

function Get-ParentTypeName {
    param([string]$Api)

    $cleanApi = $Api -replace '^\*REMOVED\*\s*', ''
    $cleanApi = $cleanApi -replace '^(abstract|virtual|override|static|const)\s+', ''

    # Properties: Type.Member.get/set -> ReturnType
    if ($cleanApi -match '^(.+?)\.(get|set)\s*(->\s*.+)?$') {
        $propPath = $matches[1]
        if ($propPath -match '^(.+)\.\w+$') { return $matches[1] }
        return $null
    }

    # Methods: Type.Method(args) -> ReturnType
    if ($cleanApi -match '^(.+?)\(') {
        $methodPath = $matches[1]
        if ($methodPath -match '^(.+)\.\w+$') { return $matches[1] }
        return $null
    }

    # Constants/Enums: Type.Name = value -> type
    if ($cleanApi -match '^(.+?)\s*=\s*') {
        $constPath = $matches[1]
        if ($constPath -match '^(.+)\.\w+$') { return $matches[1] }
        return $null
    }

    # Type declaration (no parent)
    return $null
}

function Test-IsInterfaceType {
    param([string]$TypeName)

    if ([string]::IsNullOrEmpty($TypeName)) { return $false }

    $parts = $TypeName.Split('.')
    $typePart = $parts[-1]

    # C# convention: interfaces start with 'I' followed by an uppercase letter
    return ($typePart.Length -ge 2 -and $typePart[0] -eq 'I' -and [char]::IsUpper($typePart[1]))
}

function Get-ApiIdentity {
    param([string]$Api)

    $cleanApi = $Api -replace '^\*REMOVED\*\s*', ''
    $cleanApi = $cleanApi -replace '^(abstract|virtual|override|static|const)\s+', ''

    # Methods/constructors: fully qualified name before '('
    if ($cleanApi -match '^([^(]+)\(') {
        return $matches[1].Trim()
    }

    # Properties/accessors or members with return type: everything before ' -> '
    if ($cleanApi -match '^(.+?)\s*->\s*') {
        return $matches[1].Trim()
    }

    # Type declarations or bare names
    return $cleanApi.Trim()
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
        
        # Build set of existing type names from shipped content
        $shippedTypes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        foreach ($line in $shippedContent) {
            $stripped = $line.Trim()
            if ((Get-ApiKind -Api $stripped) -eq 'Type') {
                [void]$shippedTypes.Add($stripped)
            }
        }

        # Categorize APIs into three buckets
        $newApis = @()
        $removedApis = @()
        $breakingAdditions = @()
        
        foreach ($api in $unshippedContent) {
            $apiTrimmed = $api.Trim()
            if ($apiTrimmed -eq "") { continue }
            
            # Check if this is a removed API (breaking change)
            if ($apiTrimmed.StartsWith("*REMOVED*")) {
                $removedApis += $api
            }
            # Check if this is truly new (not in shipped)
            elseif ($shippedContent -notcontains $api) {
                $kind = Get-ApiKind -Api $apiTrimmed
                $parentType = Get-ParentTypeName -Api $apiTrimmed

                # Abstract additions to existing types are breaking
                # (all derived classes must implement the new member)
                if ($kind -eq 'Abstract' -and $parentType -and $shippedTypes.Contains($parentType)) {
                    $breakingAdditions += $api
                }
                # New members on existing interfaces are breaking
                # (all implementors must add the new member)
                elseif ($kind -ne 'Type' -and $parentType -and $shippedTypes.Contains($parentType) -and (Test-IsInterfaceType -TypeName $parentType)) {
                    $breakingAdditions += $api
                }
                else {
                    $newApis += $api
                }
            }
        }
        
        if ($newApis.Count -gt 0 -or $removedApis.Count -gt 0 -or $breakingAdditions.Count -gt 0) {
            # Detect modified APIs: pair *REMOVED* entries with additions sharing the same identity
            $modifiedApis = [System.Collections.Generic.List[hashtable]]::new()

            $allAdditions = @($newApis) + @($breakingAdditions)
            $additionsByIdentity = @{}
            foreach ($api in $allAdditions) {
                $id = Get-ApiIdentity -Api $api
                if (-not $additionsByIdentity.ContainsKey($id)) {
                    $additionsByIdentity[$id] = [System.Collections.Generic.List[string]]::new()
                }
                $additionsByIdentity[$id].Add($api)
            }

            $matchedAdditions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
            $unmatchedRemoved = [System.Collections.Generic.List[string]]::new()

            foreach ($api in $removedApis) {
                $id = Get-ApiIdentity -Api $api
                if ($additionsByIdentity.ContainsKey($id) -and $additionsByIdentity[$id].Count -gt 0) {
                    $match = $additionsByIdentity[$id][0]
                    $additionsByIdentity[$id].RemoveAt(0)
                    [void]$matchedAdditions.Add($match)
                    $modifiedApis.Add(@{ Old = $api; New = $match })
                } else {
                    $unmatchedRemoved.Add($api)
                }
            }

            # Rebuild filtered lists without the matched pairs
            $newApis = @($newApis | Where-Object { -not $matchedAdditions.Contains($_) })
            $breakingAdditions = @($breakingAdditions | Where-Object { -not $matchedAdditions.Contains($_) })
            $removedApis = @($unmatchedRemoved)

            $projectInfo = @{
                Name = $projectName
                NewApis = $newApis
                RemovedApis = $removedApis
                BreakingAdditions = $breakingAdditions
                ModifiedApis = @($modifiedApis)
            }
            $allProjects += $projectInfo
            
            if ($newApis.Count -gt 0) {
                Write-Host "  Found $($newApis.Count) new API(s)" -ForegroundColor Green
            }
            if ($modifiedApis.Count -gt 0) {
                Write-Host "  Found $($modifiedApis.Count) modified API(s) [BREAKING]" -ForegroundColor Magenta
            }
            if ($breakingAdditions.Count -gt 0) {
                Write-Host "  Found $($breakingAdditions.Count) breaking addition(s) [BREAKING]" -ForegroundColor Red
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
    $totalBreakingAdditions = ($allProjects | ForEach-Object { $_.BreakingAdditions.Count } | Measure-Object -Sum).Sum
    $totalModifiedApis = ($allProjects | ForEach-Object { $_.ModifiedApis.Count } | Measure-Object -Sum).Sum
    $totalRemovedApis = ($allProjects | ForEach-Object { $_.RemovedApis.Count } | Measure-Object -Sum).Sum
    $hasBreakingChanges = ($totalRemovedApis -gt 0) -or ($totalBreakingAdditions -gt 0) -or ($totalModifiedApis -gt 0)

    $markdown += @"

- **Projects with changes:** $($allProjects.Count)
- **Total new APIs (safe):** $totalNewApis
- **Total modified APIs:** $totalModifiedApis $(if ($totalModifiedApis -gt 0) { "$EmojiWarning signature changes" } else { "" })
- **Total breaking additions:** $totalBreakingAdditions $(if ($totalBreakingAdditions -gt 0) { "$EmojiWarning abstract/interface members on existing types" } else { "" })
- **Total removed APIs:** $totalRemovedApis $(if ($totalRemovedApis -gt 0) { "$EmojiWarning **BREAKING**" } else { "" })

"@

    # Per-project hybrid output:
    # - Summary table (counts by kind)
    # - Categorized lists (Removed then Added)
    $markdown += @"

## Project Breakdown

"@

    foreach ($project in $allProjects) {
        $added = @($project.NewApis)
        $removed = @($project.RemovedApis)
        $breakingAdds = @($project.BreakingAdditions)
        $modified = @($project.ModifiedApis)

        $addedGroups = Group-ApisByKind -Apis $added
        $removedGroups = Group-ApisByKind -Apis $removed
        $breakingAddGroups = Group-ApisByKind -Apis $breakingAdds

        # Group modified APIs by the kind of their old (removed) entry
        $modifiedGroups = @{}
        foreach ($mod in $modified) {
            $k = Get-ApiKind -Api $mod.Old
            if (-not $modifiedGroups.ContainsKey($k)) { $modifiedGroups[$k] = New-Object System.Collections.Generic.List[hashtable] }
            $modifiedGroups[$k].Add($mod)
        }

        $allKinds = @($addedGroups.Keys + $removedGroups.Keys + $breakingAddGroups.Keys + $modifiedGroups.Keys | Sort-Object -Unique | Sort-Object { Get-KindOrder $_ }, { $_ })

        $markdown += "`n### $($project.Name)`n`n"

        # Summary table
        $markdown += "| Kind | Added | Modified | Breaking Additions | Removed |`n"
        $markdown += "|---|---:|---:|---:|---:|`n"

        foreach ($k in $allKinds) {
            $aCount = if ($addedGroups.ContainsKey($k)) { $addedGroups[$k].Count } else { 0 }
            $mCount = if ($modifiedGroups.ContainsKey($k)) { $modifiedGroups[$k].Count } else { 0 }
            $bCount = if ($breakingAddGroups.ContainsKey($k)) { $breakingAddGroups[$k].Count } else { 0 }
            $rCount = if ($removedGroups.ContainsKey($k)) { $removedGroups[$k].Count } else { 0 }
            $markdown += "| $k | $aCount | $mCount | $bCount | $rCount |`n"
        }

        $markdown += "| **Total** | **$($added.Count)** | **$($modified.Count)** | **$($breakingAdds.Count)** | **$($removed.Count)** |`n"

        # Breaking changes - Removed APIs (pure removals, no replacement)
        if ($removed.Count -gt 0) {
            $markdown += "`n#### $EmojiWarning Removed APIs (BREAKING) ($($removed.Count))`n`n"

            foreach ($k in $allKinds) {
                if (-not $removedGroups.ContainsKey($k)) { continue }
                $markdown += "##### $k ($($removedGroups[$k].Count))`n`n"

                foreach ($api in ($removedGroups[$k] | Sort-Object)) {
                    $markdown += "$(Format-ApiSignature -Api $api -OmitKindLabel)`n"
                }

                $markdown += "`n"
            }
        }

        # Breaking changes - Modified APIs (signature changes: old removed + new added)
        if ($modified.Count -gt 0) {
            $markdown += "`n#### $EmojiWarning Modified APIs (BREAKING) ($($modified.Count))`n`n"
            $markdown += "_Signature changes $Arrow callers and/or derived classes must be updated._`n`n"

            foreach ($k in $allKinds) {
                if (-not $modifiedGroups.ContainsKey($k)) { continue }
                $markdown += "##### $k ($($modifiedGroups[$k].Count))`n`n"

                foreach ($mod in ($modifiedGroups[$k] | Sort-Object { $_.Old })) {
                    $oldFormatted = Format-ApiSignature -Api $mod.Old -OmitKindLabel
                    $newFormatted = Format-ApiSignature -Api $mod.New -OmitKindLabel
                    $newFormatted = $newFormatted -replace '^- ', '  **Changed to:** '
                    $markdown += "$oldFormatted`n$newFormatted`n"
                }

                $markdown += "`n"
            }
        }

        # Breaking changes - Abstract/interface member additions to existing types
        if ($breakingAdds.Count -gt 0) {
            $markdown += "`n#### $EmojiWarning Breaking Additions ($($breakingAdds.Count))`n`n"
            $markdown += "_New abstract or interface members on existing types $Arrow all derived classes / implementors must be updated._`n`n"

            foreach ($k in $allKinds) {
                if (-not $breakingAddGroups.ContainsKey($k)) { continue }
                $markdown += "##### $k ($($breakingAddGroups[$k].Count))`n`n"

                foreach ($api in ($breakingAddGroups[$k] | Sort-Object)) {
                    $markdown += "$(Format-ApiSignature -Api $api -OmitKindLabel)`n"
                }

                $markdown += "`n"
            }
        }

        # Non-breaking additions
        if ($added.Count -gt 0) {
            $markdown += "`n#### $EmojiCheck Added APIs (Non-Breaking) ($($added.Count))`n`n"

            foreach ($k in $allKinds) {
                if (-not $addedGroups.ContainsKey($k)) { continue }
                $markdown += "##### $k ($($addedGroups[$k].Count))`n`n"

                foreach ($api in ($addedGroups[$k] | Sort-Object)) {
                    $markdown += "$(Format-ApiSignature -Api $api -OmitKindLabel)`n"
                }

                $markdown += "`n"
            }
        }
    }

    $markdown += @"

## Summary

### $EmojiCheck Additions (Non-Breaking)
$totalNewApis new API(s) have been added. These are **safe changes** that do not break existing code.

### $EmojiWarning Breaking Changes

"@

    if ($hasBreakingChanges) {
        $breakingDetails = @()
        if ($totalModifiedApis -gt 0) {
            $breakingDetails += "$totalModifiedApis API(s) have **changed signatures**."
        }
        if ($totalRemovedApis -gt 0) {
            $breakingDetails += "$totalRemovedApis API(s) have been **removed**."
        }
        if ($totalBreakingAdditions -gt 0) {
            $breakingDetails += "$totalBreakingAdditions **abstract or interface member(s)** have been added to existing types (all derived classes / implementors must be updated)."
        }
        $markdown += @"
$($breakingDetails -join "`n`n")

These are **BREAKING CHANGES** that will require:
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