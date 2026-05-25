# Perf Improver Memory - Shazwazza/Examine

## Validated Commands
```bash
# Build
dotnet build src/Examine.sln --configuration Release

# Test
dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --filter "TestCategory!=Benchmarks" -f net8.0

# Benchmarks (BenchmarkDotNet)
dotnet run --project src/Examine.Benchmarks --configuration Release
```

## Last Run Tasks (2026-05-25)
- Task 1: Commands validated ✅
- Task 3: Implemented perf improvement (PR created)
- Task 7: Monthly activity issue created

## Optimization Backlog
| Priority | Area | Opportunity |
|----------|------|-------------|
| DONE | LuceneSearchExecutor | Redundant LastOrDefault() + new object[0] in search-after path (PR submitted) |
| MEDIUM | LuceneSearchExecutor | _facetFields.Any() evaluated twice per Execute() call (lines 126, 248) |
| MEDIUM | LuceneSearchExecutor | LuceneFacetExtractionContext created per field in ExtractFacets loop |
| LOW | LuceneSearchExecutor | GetSearchAfterOptions: lastFieldDoc.Fields?.ToArray() allocates on every paginated result |
| LOW | CheckQueryForExtractTerms | Uses reflection GetType() + IsAssignableFrom per query — consider type-switch pattern |

## Completed Work
- 2026-05-25: PR "perf: eliminate redundant LastOrDefault() and empty array allocations in search-after path" on branch perf-assist/fix-search-after-allocations

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~297 tests, takes ~2.5 min on net8.0
- Efficiency Improver (separate bot) already made CreateSearchResult de-duplication (processedFields HashSet)
