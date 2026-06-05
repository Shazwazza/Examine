# Copilot Instructions for Examine

## Build & Test

Solution: `src/Examine.sln`

```powershell
# Build
dotnet build src/Examine.sln --configuration Release

# Run all tests (excluding benchmarks)
dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --filter "TestCategory!=Benchmarks"

# Run a single test by name
dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --filter "FullyQualifiedName~YourTestName"

# Run tests for a specific target framework
dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --framework net10.0

# Pack NuGet packages
dotnet pack src/Examine.sln --configuration Release -p:ContinuousIntegrationBuild=true
```

Tests use **NUnit**. Target frameworks: `net8.0`, `net9.0`, `net10.0`.

## Architecture

Examine is a .NET indexing/search abstraction over Lucene.NET, split into layered packages:

- **Examine.Core** — Framework-agnostic contracts and base classes. Key types:
  - `IExamineManager` — Singleton registry for indexes and searchers (the main entry point)
  - `IIndex` — Write-side abstraction (`IndexItems`, `DeleteFromIndex`, `CreateIndex`)
  - `ISearcher` — Read-side abstraction (`CreateQuery`, `Search`)
  - `BaseIndexProvider` — Abstract base for index implementations; validates `ValueSet`s and raises indexing events
  - `ValueSet` — The document/payload indexed into an index (ID + category + field dictionary)
  - `FieldDefinition` / `FieldDefinitionCollection` — Schema for index fields and their types

- **Examine.Lucene** — Lucene.NET implementation layer:
  - `LuceneIndex` — Primary index implementation with NRT (near-real-time) support, async indexing, taxonomy/facets
  - `BaseLuceneSearcher` — Builds Lucene queries using configured analyzers
  - `LuceneQuery` — Fluent query builder implementing `IQuery`/`INestedQuery`
  - `SearchContext` — Manages `SearcherManager` acquire/release lifecycle
  - `MultiIndexSearcher` — Composes multiple searchers for cross-index queries

- **Examine.Host** (`Examine` NuGet package) — DI registration via `AddExamine()`, `AddExamineLuceneIndex()`, `AddExamineLuceneMultiSearcher()`

Data flows: configure indexes via DI → populate with `ValueSet` documents → query via fluent `IQuery` API → results as `ISearchResults`.

## Public API Tracking

Every public API change is tracked via the Roslyn `Microsoft.CodeAnalysis.PublicApiAnalyzers` package. Each library project has `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` files.

- **RS0016** warning = new public member not listed in Unshipped. Use the IDE code-fix to add it.
- **RS0017** warning = shipped member was removed. Add a `*REMOVED*` prefix entry to Unshipped.
- Generate a change report: `.\build\Get-PublicApiReport.ps1`
- After a release, merge unshipped → shipped: `.\build\Merge-PublicApiFiles.ps1`

When adding or modifying public APIs, update the corresponding `PublicAPI.Unshipped.txt` file. The Copilot skill `public-api-management` can automate report generation and merging.

## v3/v4 Backward Compatibility

This codebase (v4) must maintain backward compatibility with v3 APIs. The custom agent `examine-compat-validator` and its companion skills (`examinex-compat`, `umbraco-compat`, `umbraco-search-compat`) validate compatibility by building downstream consumers (ExamineX, Umbraco CMS, Umbraco.Cms.Search) against local Examine project references.

Key constraints:
- Do not change signatures of public/protected methods without considering downstream impact
- Adding required parameters to existing methods is a breaking change
- Adding abstract/interface members forces implementors to update (breaking)
- `Examine.Lucene` grants `InternalsVisibleTo` to `Examine.Test` and `Examine.Benchmarks`

## Conventions

- **Nullable reference types** are enabled project-wide; `TreatWarningsAsErrors` is on
- **C# latest** language version
- 4-space indentation for C#; 2-space for XML/YAML/JSON (see `src/.editorconfig`)
- Test naming follows behavior-driven style (Given/When/Then or descriptive phrases)
- Tests use `[TestFixture]`, `[Test]`, `[TestCase]` attributes with NUnit
- Integration tests use real Lucene directories with `using` blocks and explicit cleanup
- The `TEMP/` folder at workspace root is disposable scratch space for compatibility testing — never commit it
