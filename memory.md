# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-06-26

## Build/Test Commands (Validated)
- Restore: `dotnet restore src/Examine.sln`
- Build: `dotnet build src/Examine.sln --configuration Release`
- Test: `dotnet test src/Examine.Test/Examine.Test.csproj -f net8.0`
- Benchmarks: `src/Examine.Benchmarks/` (BenchmarkDotNet, run as executable)

## Efficiency Notes
- Hot path: `CreateSearchResult` (now O(N) single-pass after PR #509)
- Hot path: `AddDocument` - system field value types now cached after PR #512
- `LuceneSearchQueryBase.GroupedAnd/Or/Not` string overloads - LINQ state machine allocs eliminated (PR #515 open)
- `MultiSearchContext`: LINQ state machine allocs eliminated in GetSearcher/GetFieldValueType/SearchableFields (PR #515 open)
- `CheckQueryForExtractTerms`: reflection replaced with pattern matching (PR #516/aw_pr_nrq open)
- `ManagedQueryInternal`: Select+Where LINQ replaced with inline loop (PR #516/aw_pr_nrq open)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- `StringExtensions.ReplaceNonAlphanumericChars` appears to be dead code (no callers found)

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | LINQ state-machine allocs in GroupedAnd/Or/Not + MultiSearchContext | Eliminate 1 heap obj per grouped-query call | PR #515 open |
| OPEN | Code-Level | Reflection in CheckQueryForExtractTerms + LINQ in ManagedQueryInternal | 3 reflection calls + 2 LINQ allocs eliminated | PR open (2026-06-26) |
| LOW | Code-Level | `StringExtensions.ReplaceNonAlphanumericChars` dead code with inefficient pattern | Cleanup if dead; or optimize if used | identified |
| LOW | Code-Level | `IsStandardAnalyzerStopWord` calls ToLower() on every stop-word check | 1 string alloc per word checked; CharArraySet may handle case internally | identified |

## Completed Work
- 2026-05-25: PR #438 merged — CreateSearchResult dedup fix
- 2026-05-29: PR #446 merged — ValueSet redundant copy fix
- 2026-06-01: PR #448 merged — Cast<IExamineValue>() elimination
- 2026-06-03/04: PRs #457, #462 merged — LINQ alloc + GroupedAnd/Or reductions
- 2026-06-17: PRs #475, #479, #481 merged — Cast+array copy in LuceneQuery, IndexItems fast-path, string[] copy
- 2026-06-19: PR #490 merged — ThreadStatic HashSet + single-pass SearchableFields
- 2026-06-25: PR #509 merged — single-pass field collection in CreateSearchResult (O(N²)→O(N))
- 2026-06-25: PR #512 merged (by perf-improver) — cached system field types in AddDocument + precompute TimeSpan constants
- 2026-06-25: PR #515 created — LINQ state-machine elimination in GroupedAnd/Or/Not + MultiSearchContext
- 2026-06-26: PR created (aw_pr_nrq) — reflection→pattern matching in CheckQueryForExtractTerms + LINQ inline loop in ManagedQueryInternal

## Backlog Cursor
- Next scan: explore LuceneIndex.cs for further I/O or allocation patterns; check StringExtensions dead code; investigate IsStandardAnalyzerStopWord ToLower() allocation

## Last Run Tasks
- 2026-06-25: Task 3 (LINQ state-machine elimination), Task 7 (update June 2026 issue #510)
- 2026-06-26: Task 2+3 (reflection elimination + LINQ inline loop in ManagedQueryInternal), Task 7 (update June 2026 issue #510)
