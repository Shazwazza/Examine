---
name: public-api-management
description: 'Manage Examine public API tracking using the PublicAPI Roslyn analyzer workflow. Generate change reports (Get-PublicApiReport.ps1) and merge unshipped APIs into shipped after a release (Merge-PublicApiFiles.ps1). Use when the user wants to review API changes, generate a compatibility report, prepare for a release, or merge unshipped APIs into shipped.'
compatibility: 'Requires PowerShell and dotnet SDK. Must be run from the Examine workspace root.'
metadata:
  author: Examine
  version: "1.0"
---

# Public API Management Skill

Manages the Examine public API tracking workflow powered by the Roslyn `RS0016`/`RS0017` analyzers and two companion PowerShell scripts.

## When to Use This Skill

- User asks to "generate an API report", "show API changes", "what APIs changed", or "review public API"
- User asks to "merge unshipped APIs", "ship the APIs", "finalize APIs for release", or "move unshipped to shipped"
- User asks "how do I use the PublicAPI scripts?" or "how does API tracking work?"
- Before a release, to audit and document all API surface changes
- After a release, to promote unshipped entries into shipped

## Background: How PublicAPI Tracking Works

Examine uses the Roslyn **PublicAPI analyzers** (`Microsoft.CodeAnalysis.PublicApiAnalyzers`). Each project that participates contains two files:

| File | Purpose |
|------|---------|
| `PublicAPI.Shipped.txt` | APIs that have been released in a prior version. **Do not edit by hand** — use the merge script. |
| `PublicAPI.Unshipped.txt` | APIs that are new or removed since the last release. The build populates these via RS0016 (missing entry) and RS0017 (removed entry) analyzer warnings. |

### Entry format

```
#nullable enable
Examine.SomeType
Examine.SomeType.SomeMethod(string! arg) -> void
*REMOVED*Examine.SomeType.OldMethod() -> void
```

- Lines without `*REMOVED*` are **additions** (new public API surface).
- Lines starting with `*REMOVED*` are **removals** (breaking changes).
- `#nullable enable` is a directive, not an API entry — both scripts ignore it.

## Available Scripts

Both scripts live under `build/` at the workspace root.

### 1. `Get-PublicApiReport.ps1` — Generate a Change Report

Scans all `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` files under `src/`, then produces a markdown report documenting every addition and removal, grouped by project and API kind.

#### Usage

```powershell
# From the workspace root (default paths)
.\build\Get-PublicApiReport.ps1

# Custom output location
.\build\Get-PublicApiReport.ps1 -OutputPath "C:\Reports\api-changes.md"

# Custom source path
.\build\Get-PublicApiReport.ps1 -SourcePath "D:\Other\src"
```

#### Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-SourcePath` | `<script dir>\..\src` | Root directory to search for PublicAPI files |
| `-OutputPath` | `<script dir>\..\PublicAPI-Changes.md` | Where to write the markdown report |

#### Output

- Writes a markdown file (default: `PublicAPI-Changes.md` at workspace root) containing:
  - Summary counts (safe additions, breaking additions, removed APIs)
  - Per-project tables broken down by API kind (Type, Constructor, Property, Abstract, etc.)
  - Detailed lists of:
    - **Removed APIs** (breaking — `*REMOVED*` entries)
    - **Breaking Additions** (abstract or interface members added to existing types — forces derived classes / implementors to update)
    - **Added APIs** (non-breaking — safe additions)
  - Next-steps checklist for release preparation

#### When to Run

- **Before a release** — to document and review all pending API changes.
- **During development** — to get a quick snapshot of the current API delta.
- **During code review** — to assess whether a PR introduces breaking changes.

---

### 2. `Merge-PublicApiFiles.ps1` — Merge Unshipped → Shipped

After a release, this script moves all entries from `PublicAPI.Unshipped.txt` into `PublicAPI.Shipped.txt` (sorted, deduplicated), then clears the unshipped file.

#### Usage

```powershell
# From the workspace root (default paths)
.\build\Merge-PublicApiFiles.ps1

# Custom source path
.\build\Merge-PublicApiFiles.ps1 -SourcePath "D:\Other\src"
```

#### Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-SourcePath` | `<script dir>\..\src` | Root directory to search for PublicAPI files |

#### What It Does

For each project:

1. Reads `PublicAPI.Unshipped.txt` (ignoring blank lines and `#nullable enable`)
2. Appends the additions to `PublicAPI.Shipped.txt`
3. Applies `*REMOVED*` entries by deleting the matching signature from `PublicAPI.Shipped.txt`. The `*REMOVED*` marker line is never written to shipped. If a `*REMOVED*` entry has no matching shipped API, the script warns and continues.
4. Sorts and deduplicates the combined shipped file
5. Clears `PublicAPI.Unshipped.txt` (preserving `#nullable enable` if present)

#### When to Run

- **After tagging a release** — to finalize the public API baseline for the newly shipped version.
- **Never before a release** — doing so would erase the record of what changed.

---

## Typical Release Workflow

```
1. Develop features / fix bugs (Unshipped files accumulate changes automatically via build)
2. Run  .\build\Get-PublicApiReport.ps1        ← Review the report
3. Address any unintended breaking changes
4. Tag the release
5. Run  .\build\Merge-PublicApiFiles.ps1        ← Promote unshipped → shipped
6. Commit the updated PublicAPI files
```

## Step-by-Step Instructions for the Agent

### Generating a Report

1. Ensure the working directory is the Examine workspace root.
2. Run:
   ```powershell
   .\build\Get-PublicApiReport.ps1
   ```
3. Read the generated `PublicAPI-Changes.md` and summarize key findings to the user:
   - Total additions and removals
   - Whether there are breaking changes
   - Which projects are affected
4. If the user wants the report at a different path, re-run with `-OutputPath`.

### Merging After a Release

1. **Confirm with the user** that they intend to merge (this is destructive — unshipped entries are cleared).
2. Run:
   ```powershell
   .\build\Merge-PublicApiFiles.ps1
   ```
3. Verify the output shows successful processing with no errors.
4. Remind the user to commit the updated `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` files.

### Fixing Analyzer Warnings

If the build shows RS0016 or RS0017 warnings:

- **RS0016** ("Symbol is not part of the declared API"): A new public member was added but not listed in `PublicAPI.Unshipped.txt`. Use the IDE code-fix (💡) or manually add the entry.
- **RS0017** ("Symbol is part of the declared API but is removed"): A previously shipped member was removed. Add a `*REMOVED*` entry to `PublicAPI.Unshipped.txt`.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Script says "No PublicAPI.Unshipped.txt files found" | Ensure `-SourcePath` points to the `src/` directory containing the projects |
| Report shows no changes but build has RS0016 warnings | The unshipped file may be missing entries — run the build and apply code-fixes first |
| Merge clears unshipped but shipped looks wrong | Check that the shipped file wasn't corrupted — entries should be sorted alphabetically |
| Emojis render as `?` in the report | The script writes UTF-8 with BOM; open the file in an editor that supports UTF-8 |
| PowerShell 5.1 garbles output | The script handles this via codepoint construction, but ensure the `.ps1` file is saved as UTF-8 with BOM |
