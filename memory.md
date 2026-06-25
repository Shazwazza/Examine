# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-06-25

## Build/Test Commands (Validated)
- Restore: `dotnet restore src/Examine.sln`
- Build: `dotnet build src/Examine.sln --configuration Release`
- Test: `dotnet test src/Examine.Test/Examine.Test.csproj -f net8.0`
- Benchmarks: `src/Examine.Benchmarks/` (BenchmarkDotNet, run as executable)

## Efficiency Notes
- Hot path: `CreateSearchResult` (now O(N) single-pass after PR #509)
- Hot path: `AddDocument` - system field value types now cached after PR #512
- `LuceneSearchQueryBase.GroupedAnd/Or/Not` string overloads - LINQ state machine allocs eliminated (PR open 2026-06-25)
- `MultiSearchContext`: LINQ state machine allocs eliminated in GetSearcher/GetFieldValueType/SearchableFields (PR open 2026-06-25)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | LINQ state-machine allocs in GroupedAnd/Or/Not + MultiSearchContext | Eliminate 1 heap obj per grouped-query call | PR open (2026-06-25) |
| LOW | Code-Level | `CheckQueryForExtractTerms` reflection for `NumericRangeQuery<>` | Minor: reflection per non-standard query | identified |

## Completed Work
- 2026-05-25: PR #438 merged — CreateSearchResult dedup fix
- 2026-05-29: PR #446 merged — ValueSet redundant copy fix
- 2026-06-01: PR #448 merged — Cast<IExamineValue>() elimination
- 2026-06-03/04: PRs #457, #462 merged — LINQ alloc + GroupedAnd/Or reductions
- 2026-06-17: PRs #475, #479, #481 merged — Cast+array copy in LuceneQuery, IndexItems fast-path, string[] copy
- 2026-06-19: PR #490 merged — ThreadStatic HashSet + single-pass SearchableFields
- 2026-06-25: PR #509 merged — single-pass field collection in CreateSearchResult (O(N²)→O(N))
- 2026-06-25: PR #512 merged (by perf-improver) — cached system field types in AddDocument + precompute TimeSpan constants
- 2026-06-25: PR created — LINQ state-machine elimination in GroupedAnd/Or/Not + MultiSearchContext

## Backlog Cursor
- Next scan: `CheckQueryForExtractTerms` reflection path, or explore Directories/ for I/O patterns

## Last Run Tasks
- 2026-06-25: Task 3 (implement LINQ state-machine elimination), Task 7 (update June 2026 issue #510)
