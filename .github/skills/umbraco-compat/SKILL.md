---
name: umbraco-compat
description: 'Validate Examine v4 backward compatibility with Umbraco CMS by cloning the Umbraco-CMS repo, swapping NuGet package references to local project references, and verifying the build and tests pass. Use when checking API compatibility, verifying breaking changes, or validating that Examine v4 works with Umbraco CMS. Intended for the examine-compat-validator custom agent.'
compatibility: 'Requires git and dotnet SDK. Requires network access to clone https://github.com/umbraco/Umbraco-CMS.git'
metadata:
  author: Examine
  version: "1.0"
---

# Umbraco CMS Compatibility Validator

Validates that the current Examine v4 codebase is backward compatible with Umbraco CMS by replacing Umbraco's NuGet package references for Examine with project references pointing to this workspace, then building and testing the Umbraco solution.

## When to Use This Skill

- After making changes to Examine public APIs
- Before releasing a new version of Examine
- When verifying that Examine v4 does not introduce breaking changes for Umbraco CMS
- When the user asks to "check Umbraco compatibility", "validate compatibility", or "test against Umbraco"

## Prerequisites

- `git` CLI available on PATH
- `dotnet` SDK installed (must support the target frameworks used by both Examine and Umbraco CMS)
- Network access to clone from GitHub

## Important Rules

- The `TEMP` folder at the workspace root is **disposable** and must **NEVER** be committed to the Examine repository.
- Ensure `TEMP/` is listed in the root `.gitignore` before proceeding.
- If `TEMP/Umbraco-CMS` already exists, skip the clone step and use the existing clone (optionally run `git pull` to update).

## Package-to-Project Mapping

Umbraco CMS uses Central Package Management (`Directory.Packages.props`). The following NuGet packages must be swapped to project references:

| NuGet Package | Umbraco Project(s) That Reference It | Examine Project Reference |
|---|---|---|
| `Examine` | `src/Umbraco.Examine.Lucene/Umbraco.Examine.Lucene.csproj` | `src/Examine.Host/Examine.csproj` |
| `Examine.Core` | `src/Umbraco.Infrastructure/Umbraco.Infrastructure.csproj` | `src/Examine.Core/Examine.Core.csproj` |

## Step-by-Step Workflow

### Step 1: Ensure TEMP Is Git-Ignored

Check that the workspace root `.gitignore` contains a `TEMP/` entry. If not, add it:

```
TEMP/
```

### Step 2: Clone Umbraco CMS (If Needed)

If `TEMP/Umbraco-CMS` does not exist:

```bash
git clone --depth 1 https://github.com/umbraco/Umbraco-CMS.git TEMP/Umbraco-CMS
```

If it already exists, optionally update it:

```bash
cd TEMP/Umbraco-CMS && git pull && cd ../..
```

### Step 3: Modify Directory.Packages.props

In `TEMP/Umbraco-CMS/Directory.Packages.props`, **remove or comment out** the Examine package version entries:

```xml
<!-- Remove or comment out these lines: -->
<PackageVersion Include="Examine" Version="3.7.1" />
<PackageVersion Include="Examine.Core" Version="3.7.1" />
```

### Step 4: Swap Package References to Project References

#### 4a. Umbraco.Examine.Lucene.csproj

File: `TEMP/Umbraco-CMS/src/Umbraco.Examine.Lucene/Umbraco.Examine.Lucene.csproj`

Replace:
```xml
<PackageReference Include="Examine" />
```

With:
```xml
<ProjectReference Include="..\..\..\..\src\Examine.Host\Examine.csproj" />
```

#### 4b. Umbraco.Infrastructure.csproj

File: `TEMP/Umbraco-CMS/src/Umbraco.Infrastructure/Umbraco.Infrastructure.csproj`

Replace:
```xml
<PackageReference Include="Examine.Core" />
```

With:
```xml
<ProjectReference Include="..\..\..\..\src\Examine.Core\Examine.Core.csproj" />
```

### Step 5: Restore and Build

From the workspace root, build the Umbraco solution:

```bash
cd TEMP/Umbraco-CMS
dotnet restore
dotnet build --no-restore
```

**Expected outcome:** Build succeeds with zero errors. Warnings are acceptable.

If the build fails, analyze the error output:
- **Missing type/member errors** indicate a breaking API change in Examine that must be addressed.
- **Target framework mismatches** may require aligning TFMs between Examine and Umbraco.
- **Transitive dependency conflicts** may require version alignment in Directory.Packages.props.

### Step 6: Run Tests

```bash
cd TEMP/Umbraco-CMS
dotnet test --no-build --filter "FullyQualifiedName~Examine"
```

If no Examine-specific test filter exists, run the full test suite:

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
| Build fails with TFM errors | Verify both repos target compatible frameworks (check `Directory.Build.props` in both repos) |
| NuGet restore fails after swap | Ensure the PackageVersion entries were fully removed from `Directory.Packages.props` |
| Relative paths are wrong | Verify the workspace structure: Examine repo root must contain both `TEMP/Umbraco-CMS/` and `src/` |
| Tests fail on unrelated Umbraco issues | Focus only on failures that reference Examine namespaces or types |

## Cleanup

The `TEMP` folder is disposable. To clean up:

```bash
Remove-Item -Recurse -Force TEMP
```