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

## Last Run Tasks (2026-06-04)
- Task 3: Implemented perf improvement (PR created) — LINQ LastOrDefault + minor array allocs
- Task 7: Monthly activity issue created (new month)

## Optimization Backlog
| Priority | Area | Opportunity |
|----------|------|-------------|
| DONE | LuceneSearchExecutor | Redundant LastOrDefault() + new object[0] in search-after path (PR #441, merged) |
| DONE | LuceneSearchExecutor | CheckQueryForExtractTerms reflection + Fields?.ToArray() copy (PR #445, merged) |
| DONE | LuceneSearchExecutor | GetSearchAfterOptions ScoreDocs.LastOrDefault() LINQ + minor array allocs (PR submitted 2026-06-04) |
| LOW | CheckQueryForExtractTerms | Early-out for BooleanQuery with empty clauses (very minor) |
| MEDIUM | LuceneSearchQueryBase | fields.ToArray() on IEnumerable<string> in GroupedAnd/Or/Not — could accept IReadOnlyList |

## Completed Work
- 2026-05-25: PR #441 "perf: eliminate redundant LastOrDefault() and empty array allocations in search-after path" — merged
- 2026-05-29: PR #445 "perf: use is-pattern matching and avoid ToArray() copy in LuceneSearchExecutor" — merged
- 2026-06-04: PR submitted "perf: eliminate LINQ LastOrDefault() and minor array allocations"

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~142 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
