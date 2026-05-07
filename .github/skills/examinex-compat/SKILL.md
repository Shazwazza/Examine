---
name: examinex-compat
description: 'Validate Examine v4 backward compatibility with ExamineX by cloning the ExamineX.Source repo (support/9.x branch), swapping Examine NuGet package references to local project references, and verifying the build passes. Use when checking API compatibility, verifying breaking changes, or validating that Examine v4 works with ExamineX. Intended for the examine-compat-validator custom agent.'
compatibility: 'Requires git and dotnet SDK 10.0+. Requires network access to clone https://github.com/SDKits/ExamineX.Source.git'
metadata:
  author: Examine
  version: "1.0"
---

# ExamineX Compatibility Validator

Validates that the current Examine v4 codebase is backward compatible with ExamineX by replacing ExamineX's NuGet package references for Examine with project references pointing to this workspace, then building the ExamineX solution.

## When to Use This Skill

- After making changes to Examine public APIs
- Before releasing a new version of Examine
- When verifying that Examine v4 does not introduce breaking changes for ExamineX
- When the user asks to "check ExamineX compatibility", "validate ExamineX compat", or "test against ExamineX"

## Prerequisites

- `git` CLI available on PATH
- `dotnet` SDK 10.0+ installed (ExamineX targets `net10.0`)
- Network access to clone from GitHub (the SDKits/ExamineX.Source repo must be accessible with the current git credentials)

## Important Rules

- The `TEMP` folder at the workspace root is **disposable** and must **NEVER** be committed to the Examine repository.
- Ensure `TEMP/` is listed in the root `.gitignore` before proceeding.
- If `TEMP/ExamineX.Source` already exists, skip the clone step and use the existing clone (run `git pull` to update).
- Always use the **`support/9.x`** branch.
- Always build in **Debug** configuration — ExamineX runs obfuscation in Release which is unnecessary for compat testing.

## Repository Structure

The ExamineX.Source repository (`support/9.x` branch) has the following relevant layout:

```
ExamineX.Source/
├── src/
│   ├── ExamineX.sln                              ← Main solution file
│   ├── Directory.Packages.props                   ← Central Package Management (has Examine entries)
│   ├── Directory.Build.props                      ← Global properties (TFM: net10.0)
│   ├── global.json                                ← Requires SDK 10.0.100+
│   ├── ExamineX.Shared/                           ← Shared library (references Examine.Lucene)
│   ├── ExamineX.AzureSearch/                      ← Azure Search provider (references Examine.Lucene)
│   ├── ExamineX.ElasticSearch/                    ← Elastic Search provider (references Examine.Lucene)
│   ├── ExamineX.Solr/                             ← Solr provider (references Examine.Lucene)
│   ├── ExamineX.AzureSearch.Umbraco/              ← Azure + Umbraco integration
│   ├── ExamineX.ElasticSearch.Umbraco/            ← Elastic + Umbraco integration
│   ├── ExamineX.Shared.Umbraco/                   ← Shared Umbraco integration
│   ├── ExamineX.Common.Tests/                     ← Shared test utilities (references Examine)
│   ├── ExamineX.AzureSearch.Tests/                ← Azure Search tests (references Examine)
│   ├── ExamineX.ElasticSearch.Tests/              ← Elastic Search tests (references Examine)
│   ├── ExamineX.Solr.Tests/                       ← Solr tests
│   ├── ExamineX.AzureSearch.Umbraco.BlobMedia/    ← Blob media integration
│   ├── ExamineX.AzureSearch.Umbraco.ContentDelivery/ ← Content delivery integration
│   ├── ExamineX.AzureSearch.Umbraco.Forms/        ← Azure + Umbraco Forms
│   ├── ExamineX.AzureSearch.Umbraco.PDF/          ← Azure + Umbraco PDF
│   ├── ExamineX.ElasticSearch.Umbraco.Forms/      ← Elastic + Umbraco Forms
│   ├── ExamineX.AzureSearch.Umbraco.WebTest/      ← Web test app (legacy)
│   ├── ExamineX.AzureSearch.Umbraco.WebTest2/     ← Web test app (v13+)
│   └── ExamineX.Umbraco.WebTest/                  ← Combined web test app
```

## Package-to-Project Mapping

ExamineX uses Central Package Management (`Directory.Packages.props`). The following NuGet packages must be swapped to project references:

| NuGet Package | ExamineX Project(s) That Reference It | Examine Project Reference |
|---|---|---|
| `Examine.Lucene` (v3.7.1) | `ExamineX.Shared`, `ExamineX.AzureSearch`, `ExamineX.ElasticSearch`, `ExamineX.Solr` | `src/Examine.Lucene/Examine.Lucene.csproj` |
| `Examine` (v3.7.1) | `ExamineX.Common.Tests`, `ExamineX.ElasticSearch.Tests`, `ExamineX.AzureSearch.Tests` | `src/Examine.Host/Examine.csproj` |

> **Note:** The `Examine` meta-package transitively includes `Examine.Core` and `Examine.Lucene`. The test projects reference it for full Examine API access.

## Step-by-Step Workflow

### Step 1: Ensure TEMP Is Git-Ignored

Check that the workspace root `.gitignore` contains a `TEMP/` entry. If not, add it:

```
TEMP/
```

### Step 2: Clone ExamineX.Source (If Needed)

If `TEMP/ExamineX.Source` does not exist:

```bash
git clone --branch support/9.x https://github.com/SDKits/ExamineX.Source.git TEMP/ExamineX.Source
```

If it already exists, ensure it is on the correct branch and up to date:

```bash
cd TEMP/ExamineX.Source && git checkout support/9.x && git pull && cd ../..
```

### Step 3: Modify Directory.Packages.props

In `TEMP/ExamineX.Source/src/Directory.Packages.props`, **remove or comment out** the Examine package version entries:

```xml
<!-- Remove or comment out these lines: -->
<PackageVersion Include="Examine" Version="3.7.1" />
<PackageVersion Include="Examine.Lucene" Version="3.7.1" />
```

### Step 4: Swap Package References to Project References

All ExamineX projects live under `TEMP/ExamineX.Source/src/<ProjectName>/`, which is 4 directories deep from the Examine workspace root. The relative path prefix is `..\..\..\..\src\`.

#### 4a. Projects referencing `Examine.Lucene` (4 projects)

In each of these files, replace:
```xml
<PackageReference Include="Examine.Lucene" />
```
With:
```xml
<ProjectReference Include="..\..\..\..\src\Examine.Lucene\Examine.Lucene.csproj" />
```

Files to update:
- `TEMP/ExamineX.Source/src/ExamineX.Shared/ExamineX.Shared.csproj`
- `TEMP/ExamineX.Source/src/ExamineX.AzureSearch/ExamineX.AzureSearch.csproj`
- `TEMP/ExamineX.Source/src/ExamineX.ElasticSearch/ExamineX.ElasticSearch.csproj`
- `TEMP/ExamineX.Source/src/ExamineX.Solr/ExamineX.Solr.csproj`

#### 4b. Projects referencing `Examine` (3 test projects)

In each of these files, replace:
```xml
<PackageReference Include="Examine" />
```
With:
```xml
<ProjectReference Include="..\..\..\..\src\Examine.Host\Examine.csproj" />
```

Files to update:
- `TEMP/ExamineX.Source/src/ExamineX.Common.Tests/ExamineX.Common.Tests.csproj`
- `TEMP/ExamineX.Source/src/ExamineX.ElasticSearch.Tests/ExamineX.ElasticSearch.Tests.csproj`
- `TEMP/ExamineX.Source/src/ExamineX.AzureSearch.Tests/ExamineX.AzureSearch.Tests.csproj`

### Step 5: Restore and Build

From the workspace root, build the ExamineX solution in Debug configuration:

```bash
cd TEMP/ExamineX.Source/src
dotnet restore ExamineX.sln
dotnet build ExamineX.sln --no-restore --configuration Debug
```

**Expected outcome:** Build succeeds with zero errors. Warnings are acceptable.

If the build fails, analyze the error output:
- **Missing type/member errors** indicate a breaking API change in Examine that must be addressed.
- **Nullable annotation warnings promoted to errors** (CS8600, CS8601, CS8602, CS8604, CS8620, CS8765) may be caused by Examine v4 tightening NRT annotations. These can be suppressed in the affected `.csproj` files with `<NoWarn>` for compat testing purposes, while still detecting real API shape changes.
- **Target framework mismatches** may require aligning TFMs between Examine and ExamineX (ExamineX targets `net10.0`).
- **Transitive dependency conflicts** may require version alignment in `Directory.Packages.props`.

### Step 6: Run Tests

Run the Azure Search tests (these are designed to run locally):

```bash
cd TEMP/ExamineX.Source/src
dotnet test ExamineX.AzureSearch.Tests --no-build --configuration Debug
```

**Expected outcome:** All tests pass. Any test failures related to Examine APIs indicate compatibility issues.

> **Note:** The Elasticsearch and Solr test projects may require external service infrastructure. If they fail due to connection issues (not Examine API issues), those failures can be ignored — focus on the Azure Search tests and overall build success.

### Step 7: Report Results

Summarize the outcome:

1. **Build status:** Pass / Fail (with error details)
2. **Test status:** Pass / Fail / Skipped (with failure details if applicable)
3. **Breaking changes found:** List any Examine API changes that caused build failures
4. **Recommended fixes:** For each breaking change, suggest whether to fix in Examine (restore API) or note as an intentional breaking change

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Clone fails (403 Forbidden) | The SDKits org has OAuth restrictions — ensure your git credentials have access to SDKits/ExamineX.Source |
| Clone fails (branch not found) | Verify the `support/9.x` branch exists: `git ls-remote --heads https://github.com/SDKits/ExamineX.Source.git` |
| SDK version error | ExamineX requires .NET SDK 10.0.100+. Check `TEMP/ExamineX.Source/src/global.json` |
| Build fails with TFM errors | ExamineX targets `net10.0`. Ensure Examine supports this TFM or add it |
| NuGet restore fails after swap | Ensure the `Examine` and `Examine.Lucene` PackageVersion entries were fully removed from `Directory.Packages.props` |
| Relative paths are wrong | Verify workspace structure: Examine repo root must contain both `TEMP/ExamineX.Source/` and `src/` |
| Obfuscation errors | Ensure you are building with `--configuration Debug` (obfuscation only runs in Release) |
| Nullable warnings treated as errors | Add `<NoWarn>$(NoWarn);CS8600;CS8601;CS8602;CS8604;CS8620;CS8765</NoWarn>` to affected project PropertyGroups |
| Elasticsearch/Solr tests fail with connection errors | These test projects may need external services — focus on Azure Search tests and build success |

## Cleanup

The `TEMP` folder is disposable. To clean up:

```bash
Remove-Item -Recurse -Force TEMP/ExamineX.Source
```
