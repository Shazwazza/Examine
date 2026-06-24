# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-06-24

## Build/Test Commands (Validated)
- Restore: `dotnet restore src/Examine.sln`
- Build: `dotnet build src/Examine.sln --configuration Release`
- Test: `dotnet test src/Examine.Test/Examine.Test.csproj -f net8.0`
- Benchmarks: `src/Examine.Benchmarks/` (BenchmarkDotNet, run as executable)

## Efficiency Notes
- `CreateSearchResult`: hot path for every search result — `doc.GetValues(fieldName)` did O(N²) total scans (PR open 2026-06-24, replaces with O(N) single-pass)
- Previous optimisations fully merged: PR #438 (GetValues dedup), #446 (ValueSet ctor), #448/#475 (Cast removal), #457/#462/#479/#481/#490 (various alloc reductions)
- Tests run via NUnit, CI uses `dotnet test`. Test count 148 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| HIGH | Code-Level | `CreateSearchResult` O(N²) `GetValues` inner scan | Reduce CPU+allocs per search result | PR open (2026-06-24) |
| LOW | Code-Level | `GroupedAnd/Or/Not` INestedQuery string→ExamineValue `Select().ToArray()` | Minor: 1 LINQ state machine per call | identified |
| LOW | Code-Level | `CheckQueryForExtractTerms` reflection for `NumericRangeQuery<>` | Minor: reflection per non-standard query | identified |

## Completed Work
- 2026-05-25: PR #438 merged — CreateSearchResult dedup fix
- 2026-05-29: PR #446 merged — ValueSet redundant copy fix
- 2026-06-01: PR #448 merged — Cast<IExamineValue>() elimination
- 2026-06-03/04: PRs #457, #462 merged — LINQ alloc + GroupedAnd/Or reductions
- 2026-06-17: PRs #475, #479, #481 merged — Cast+array copy in LuceneQuery, IndexItems fast-path, string[] copy
- 2026-06-19: PR #490 merged — ThreadStatic HashSet + single-pass SearchableFields
- 2026-06-24: PR created — single-pass field collection in CreateSearchResult (O(N²)→O(N))

## Backlog Cursor
- Next scan: LuceneSearchQueryBase INestedQuery string-to-ExamineValue helpers, or scan Directories/ for I/O patterns

## Last Run Tasks
- 2026-06-24: Task 2 (scan LuceneSearchExecutor for hot-path allocations), Task 3 (implement single-pass CreateSearchResult), Task 7 (create June 2026 issue #aw_june26)
