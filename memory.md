# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-05-25

## Build/Test Commands (Validated)
- Restore: `dotnet restore src/Examine.sln`
- Build: `dotnet build src/Examine.sln --configuration Release`
- Test: `dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --filter "TestCategory!~Benchmarks" -f net8.0`
- Benchmarks: `src/Examine.Benchmarks/` (BenchmarkDotNet, run as executable)

## Efficiency Notes
- `LuceneSearchExecutor.CreateSearchResult`: hot path for every search result materialisation
- `doc.Fields` lists same field name N times for multi-value fields; `doc.GetValues(name)` returns all values at once
- Tests run via NUnit, CI uses `dotnet test` with TRX logger

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| HIGH | Code-Level | `CreateSearchResult` redundant GetValues calls for multi-valued fields | Reduce allocs + CPU per search | ✅ PR created (2026-05-25) |
| MEDIUM | Code-Level | `ReplaceNonAlphanumericChars` in StringExtensions — unused dead code | N/A (no callers) | identified |
| MEDIUM | Code-Level | `GetSearchAfterOptions` calls `LastOrDefault()` twice on same sequence | Minor | identified |

## Completed Work
- 2026-05-25: PR created for CreateSearchResult dedup fix (branch: efficiency/create-search-result-dedup)

## Backlog Cursor
- Next scan: LuceneIndex.cs (1388 lines, hot indexing path)

## Last Run Tasks
- 2026-05-25: Task 1 (discover commands), Task 3 (implement fix), Task 7 (monthly summary)
