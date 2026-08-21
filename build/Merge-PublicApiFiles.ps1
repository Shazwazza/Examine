<#
.SYNOPSIS
    Merges PublicAPI.Unshipped.txt files into PublicAPI.Shipped.txt files.

.DESCRIPTION
    This script automates the process of moving API entries from PublicAPI.Unshipped.txt 
    files to PublicAPI.Shipped.txt files. This should be done before releasing a new version 
    to mark all unshipped APIs as shipped.
    
    The script:
    1. Finds all PublicAPI.Unshipped.txt files in the src directory
    2. Appends their additions (excluding #nullable enable) to corresponding Shipped files
    3. Applies *REMOVED* entries by deleting the matching signature from the Shipped file
       (the *REMOVED* marker line itself is never written to Shipped)
    4. Sorts the Shipped files alphabetically
    5. Clears Unshipped files (leaving only #nullable enable if it was present)

.PARAMETER SourcePath
    The root path to search for PublicAPI files. Defaults to "../src" relative to script location.

.EXAMPLE
    .\Merge-PublicApiFiles.ps1
    Merges all PublicAPI files in the default src directory.

.EXAMPLE
    .\Merge-PublicApiFiles.ps1 -SourcePath "C:\MyProject\src"
    Merges all PublicAPI files in the specified directory.

.NOTES
    This script should be run after tagging a release to finalize the public API tracking.
    Running it earlier would erase the record of what changed in the release.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$SourcePath = (Join-Path $PSScriptRoot "..\src")
)

# Ensure the source path exists
if (-not (Test-Path $SourcePath)) {
    Write-Error "Source path does not exist: $SourcePath"
    exit 1
}

Write-Host "Searching for PublicAPI.Unshipped.txt files in: $SourcePath" -ForegroundColor Cyan
Write-Host ""

# Find all PublicAPI.Unshipped.txt files
$unshippedFiles = Get-ChildItem -Path $SourcePath -Recurse -Filter "PublicAPI.Unshipped.txt"

if ($unshippedFiles.Count -eq 0) {
    Write-Warning "No PublicAPI.Unshipped.txt files found in $SourcePath"
    exit 0
}

Write-Host "Found $($unshippedFiles.Count) PublicAPI.Unshipped.txt file(s)" -ForegroundColor Green
Write-Host ""

$processedCount = 0
$errorCount = 0

foreach ($unshippedFile in $unshippedFiles) {
    $unshippedPath = $unshippedFile.FullName
    $shippedPath = $unshippedPath.Replace("Unshipped", "Shipped")
    
    Write-Host "Processing: $($unshippedFile.Directory.Name)/$($unshippedFile.Name)" -ForegroundColor Yellow
    
    # Verify shipped file exists
    if (-not (Test-Path $shippedPath)) {
        Write-Warning "  Corresponding Shipped file not found: $shippedPath"
        Write-Warning "  Skipping this file."
        $errorCount++
        continue
    }
    
    try {
        # Read unshipped file
        $unshippedContent = Get-Content $unshippedPath -Raw
        $unshippedLines = $unshippedContent -split "`r?`n" | Where-Object { 
            $_.Trim() -ne "" -and $_.Trim() -ne "#nullable enable" 
        }
        
        # Check if unshipped has any actual API entries
        if ($unshippedLines.Count -eq 0) {
            Write-Host "  No API entries to merge (file is empty or contains only #nullable enable)" -ForegroundColor Gray
            continue
        }
        
        Write-Host "  Found $($unshippedLines.Count) API entries to merge" -ForegroundColor Cyan

        # Split into removals and additions. A "*REMOVED*<signature>" entry means the
        # signature must be deleted from Shipped - the marker itself is never shipped.
        $removedPrefix = "*REMOVED*"
        $removedSignatures = @($unshippedLines |
            Where-Object { $_.TrimStart().StartsWith($removedPrefix) } |
            ForEach-Object { $_.TrimStart().Substring($removedPrefix.Length).Trim() })
        $addedLines = @($unshippedLines | Where-Object { -not $_.TrimStart().StartsWith($removedPrefix) })

        if ($removedSignatures.Count -gt 0) {
            Write-Host "    $($removedSignatures.Count) removal(s), $($addedLines.Count) addition(s)" -ForegroundColor Cyan
        }

        # Read shipped file
        $shippedContent = Get-Content $shippedPath -Raw
        $shippedLines = $shippedContent -split "`r?`n" | Where-Object { $_.Trim() -ne "" }
        
        # Check if shipped file has #nullable enable
        $hasNullableDirective = $shippedLines[0] -eq "#nullable enable"
        
        # Combine and sort (excluding #nullable enable for sorting)
        $apiLines = $shippedLines | Where-Object { $_ -ne "#nullable enable" }
        $combined = ($apiLines + $addedLines) | Sort-Object -Unique

        # Apply removals, warning about any that didn't match a shipped entry
        if ($removedSignatures.Count -gt 0) {
            $notFound = @($removedSignatures | Where-Object { $combined -notcontains $_ })
            foreach ($missing in $notFound) {
                Write-Warning "  *REMOVED* entry has no matching Shipped API: $missing"
            }
            $combined = @($combined | Where-Object { $removedSignatures -notcontains $_ })
        }
        
        # Rebuild shipped file with #nullable enable at the top if it was present
        if ($hasNullableDirective) {
            $finalContent = @("#nullable enable") + $combined
        } else {
            $finalContent = $combined
        }
        
        # Write to shipped file
        $finalContent -join "`r`n" | Set-Content $shippedPath -NoNewline
        Write-Host "  Updated Shipped file with $($combined.Count) total API entries" -ForegroundColor Green
        
        # Clear unshipped file (keep #nullable enable if it was present)
        $unshippedHasNullable = $unshippedContent.TrimStart().StartsWith("#nullable enable")
        if ($unshippedHasNullable) {
            "#nullable enable" | Set-Content $unshippedPath -NoNewline
            Write-Host "  Cleared Unshipped file (kept #nullable enable)" -ForegroundColor Green
        } else {
            "" | Set-Content $unshippedPath -NoNewline
            Write-Host "  Cleared Unshipped file" -ForegroundColor Green
        }
        
        $processedCount++
    }
    catch {
        Write-Error "  Error processing file: $_"
        $errorCount++
    }
    
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Successfully processed: $processedCount file(s)" -ForegroundColor Green
if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount file(s)" -ForegroundColor Red
}
Write-Host "========================================" -ForegroundColor Cyan

if ($errorCount -gt 0) {
    exit 1
}

exit 0