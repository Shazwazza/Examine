---
name: umbraco-search-compat
description: 'Validate Examine v4 backward compatibility with Umbraco.Cms.Search by cloning the Umbraco.Cms.Search repo, swapping the Examine NuGet package reference to a local project reference, and verifying the build and tests pass. Use when checking API compatibility, verifying breaking changes, or validating that Examine v4 works with Umbraco.Cms.Search. Intended for the examine-compat-validator custom agent.'
compatibility: 'Requires git and dotnet SDK. Requires network access to clone https://github.com/umbraco/Umbraco.Cms.Search.git'
metadata:
  author: Examine
  version: "1.0"
---

# Umbraco.Cms.Search Compatibility Validator

Validates that the current Examine v4 codebase is backward compatible with Umbraco.Cms.Search by replacing its NuGet package reference for Examine with a project reference pointing to this workspace, then building and testing the solution.

## When to Use This Skill

- After making changes to Examine public APIs
- Before releasing a new version of Examine
- When verifying that Examine v4 does not introduce breaking changes for Umbraco.Cms.Search
- When the user asks to "check search compatibility", "validate search compat", or "test against Umbraco Search"

## Prerequisites

- `git` CLI available on PATH
- `dotnet` SDK installed (must support the target frameworks used by both Examine and Umbraco.Cms.Search)
- Network access to clone from GitHub

## Important Rules

- The `TEMP` folder at the workspace root is **disposable** and must **NEVER** be committed to the Examine repository.
- Ensure `TEMP/` is listed in the root `.gitignore` before proceeding.
- If `TEMP/Umbraco.Cms.Search` already exists, it means the repos skip the clone step and use the existing clone then ensure to run `git pull` to update.

## Repository Structure

The Umbraco.Cms.Search repository has the following relevant layout:

```
Umbraco.Cms.Search/
├── Directory.Packages.props          ← Central Package Management (has Examine entry)
├── src/
│   ├── Umbraco.Cms.Search.sln       ← Solution file
│   ├── Umbraco.Cms.Search.Core/     ← Core abstractions (no direct Examine ref)
│   ├── Umbraco.Cms.Search.Provider.Examine/  ← Examine provider (references Examine NuGet)
│   ├── Umbraco.Test.Search.Examine.Integration/ ← Integration tests
│   └── ...other projects
```

## Package-to-Project Mapping

Umbraco.Cms.Search uses Central Package Management (`Directory.Packages.props`). The following NuGet package must be swapped to a project reference:

| NuGet Package | Umbraco.Cms.Search Project That References It | Examine Project Reference |
|---|---|---|
| `Examine` (v4.0.0-beta.1) | `src/Umbraco.Cms.Search.Provider.Examine/Umbraco.Cms.Search.Provider.Examine.csproj` | `src/Examine.Host/Examine.csproj` |

> **Note:** `Umbraco.Cms.Search.Core` does NOT directly reference Examine — it references Umbraco CMS packages only.

## Step-by-Step Workflow

### Step 1: Ensure TEMP Is Git-Ignored

Check that the workspace root `.gitignore` contains a `TEMP/` entry. If not, add it:

```
TEMP/
```

### Step 2: Clone Umbraco.Cms.Search (If Needed)

If `TEMP/Umbraco.Cms.Search` does not exist:

```bash
git clone https://github.com/umbraco/Umbraco.Cms.Search.git TEMP/Umbraco.Cms.Search
```

If it already exists, optionally update it:

```bash
cd TEMP/Umbraco.Cms.Search && git pull && cd ../..
```

> **Note:** Do NOT use `--depth 1` as the repo uses Nerdbank.GitVersioning which requires full git history for version calculation. If you must shallow clone, be prepared to `git fetch --unshallow` before building.

### Step 3: Modify Directory.Packages.props

In `TEMP/Umbraco.Cms.Search/Directory.Packages.props`, **remove or comment out** the Examine package version entry:

```xml
<!-- Remove or comment out this line: -->
<PackageVersion Include="Examine" Version="4.0.0-beta.1" />
```

### Step 4: Swap Package Reference to Project Reference

#### Umbraco.Cms.Search.Provider.Examine.csproj

File: `TEMP/Umbraco.Cms.Search/src/Umbraco.Cms.Search.Provider.Examine/Umbraco.Cms.Search.Provider.Examine.csproj`

Replace:
```xml
<PackageReference Include="Examine"/>
```

With:
```xml
<ProjectReference Include="..\..\..\..\src\Examine.Host\Examine.csproj" />
```

### Step 5: Restore and Build

From the workspace root, build the Umbraco.Cms.Search solution:

```bash
cd TEMP/Umbraco.Cms.Search
dotnet restore
dotnet build --no-restore
```

**Expected outcome:** Build succeeds with zero errors. Warnings are acceptable.

If the build fails, analyze the error output:
- **Missing type/member errors** indicate a breaking API change in Examine that must be addressed.
- **Nullable annotation warnings promoted to errors** (CS8600, CS8601, CS8602, CS8604, CS8620, CS8765) may be caused by Examine v4 tightening NRT annotations. These can be suppressed in the affected `.csproj` files with `<NoWarn>` for compat testing purposes, while still detecting real API shape changes.
- **Target framework mismatches** may require aligning TFMs between Examine and Umbraco.Cms.Search.
- **Transitive dependency conflicts** may require version alignment in Directory.Packages.props.

### Step 6: Run Tests

Run the Examine-specific integration tests:

```bash
cd TEMP/Umbraco.Cms.Search
dotnet test src/Umbraco.Test.Search.Examine.Integration --no-build
```

If that filter doesn't work, run the full test suite:

```bash
dotnet test --no-build
```

**Expected outcome:** All tests pass. Any test failures related to Examine APIs indicate compatibility issues.

### Step 7: Report Results

Summarize the outcome:

1. **Build status:** Pass / Fail (with error details)
2. **Test status:** Pass / Fail (with failure details)
3. **Breaking changes found:** List any Examine API changes that caused build or test failures
4. **Recommended fixes:** For each breaking change, suggest whether to fix in Examine (restore API) or note as an intentional breaking change

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Clone fails | Check network access and git credentials |
| NBGV version height error | Run `git fetch --unshallow` in the clone directory |
| Build fails with TFM errors | Verify both repos target compatible frameworks (check `Directory.Build.props` in both repos) |
| NuGet restore fails after swap | Ensure the PackageVersion entry was fully removed from `Directory.Packages.props` |
| Relative paths are wrong | Verify the workspace structure: Examine repo root must contain both `TEMP/Umbraco.Cms.Search/` and `src/` |
| Tests fail on unrelated Umbraco issues | Focus only on failures that reference Examine namespaces or types |
| Nullable warnings treated as errors | Add `<NoWarn>$(NoWarn);CS8600;CS8601;CS8602;CS8604;CS8620;CS8765</NoWarn>` to affected project PropertyGroups |

## Cleanup

The `TEMP` folder is disposable. To clean up:

```bash
Remove-Item -Recurse -Force TEMP/Umbraco.Cms.Search
```
