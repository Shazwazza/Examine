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

## Last Run Tasks (2026-05-29)
- Task 3: Implemented perf improvement (PR created) — is-pattern + avoid ToArray()
- Task 7: Monthly activity issue updated

## Optimization Backlog
| Priority | Area | Opportunity |
|----------|------|-------------|
| DONE | LuceneSearchExecutor | Redundant LastOrDefault() + new object[0] in search-after path (PR submitted 2026-05-25) |
| DONE | LuceneSearchExecutor | CheckQueryForExtractTerms reflection + Fields?.ToArray() copy (PR submitted 2026-05-29) |
| LOW | LuceneSearchExecutor | GetSearchAfterOptions: LastOrDefault() on ScoreDocs — could use direct index access ScoreDocs[ScoreDocs.Length-1] |
| LOW | CheckQueryForExtractTerms | Early-out for BooleanQuery with empty clauses |

## Completed Work
- 2026-05-25: PR "perf: eliminate redundant LastOrDefault() and empty array allocations in search-after path"
- 2026-05-29: PR "perf: use is-pattern matching and avoid ToArray() copy in LuceneSearchExecutor"

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~142 tests (net8.0 filter), takes ~2.5 min
- Efficiency Improver (separate bot) already made CreateSearchResult de-duplication (processedFields HashSet)
