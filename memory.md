# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-06-30

## Build/Test Commands (Validated)
- Restore: `dotnet restore src/Examine.sln`
- Build: `dotnet build src/Examine.sln --configuration Release`
- Test: `dotnet test src/Examine.Test/Examine.Test.csproj -f net8.0`
- Benchmarks: `src/Examine.Benchmarks/` (BenchmarkDotNet, run as executable)

## Efficiency Notes
- Hot path: `CreateSearchResult` (now O(N) single-pass after PR #509)
- Hot path: `AddDocument` - system field value types now cached after PR #512
- Hot path: `AddDocument` - default field-type factories now cached (PR #522)
- Hot path: `CheckQueryForExtractTerms` - reflection replaced with pattern matching (PR #517)
- Hot path: `ManagedQueryInternal` - LINQ state-machine eliminated (PR #517)
- `SearchResult.GetValues` - dead Values fallback removed (PR open, branch efficiency/simplify-searchresult-getvalues)
- `LuceneSearchQueryBase.GroupedAnd/Or/Not` - LINQ state-machine allocs eliminated (PR #515, merged)
- `MultiSearchContext`: LINQ state-machine allocs eliminated (PR #515, merged)
- `LuceneQuery.GroupedAnd/Or/Not`: LINQ state-machine allocs eliminated (PR #518, open)
- `GenerateHash`: removed redundant .ToLower() after "x2" format (PR #519, merged)
- `RemoveStopWords`: Action delegate/innerBuilder/string-concat allocs eliminated (PR #519, merged)
- `AddDocument`: field.Key.StartsWith uses StringComparison.Ordinal (PR #521, merged)
- `IsStandardAnalyzerStopWord`: ToLowerInvariant() replaces ToLower() (PR #521, merged)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- `StringExtensions.ReplaceNonAlphanumericChars` is dead code (no callers found) with O(N²) pattern
- `StringExtensions.EnsureEndsWith` is dead code (no callers found)
- PRs from perf-improver: #516 (cache FieldValueType factories + early-return BooleanQuery + Array.Empty sort), #524 (ManagedQueryBenchmarks), #525 (benchmark results published)

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | LINQ state-machine allocs in LuceneQuery GroupedAnd/Or/Not | 6 SelectIterator allocs eliminated | PR #518 open |
| OPEN | Code-Level | Dead Values fallback in SearchResult.GetValues | Eliminates _fields lazy-init + array alloc on miss | PR open (efficiency/simplify-searchresult-getvalues) |
| LOW | Code-Level | `StringExtensions.ReplaceNonAlphanumericChars` dead code with inefficient pattern | Cleanup if dead; or optimize if used | identified |
| LOW | Code-Level | `StringExtensions.EnsureEndsWith` dead code with alloc-heavy EndsWith | Cleanup only | identified |

## Completed Work
- 2026-05-25: PR #438 merged — CreateSearchResult dedup fix
- 2026-05-29: PR #446 merged — ValueSet redundant copy fix
- 2026-06-01: PR #448 merged — Cast<IExamineValue>() elimination
- 2026-06-03/04: PRs #457, #462 merged — LINQ alloc + GroupedAnd/Or reductions
- 2026-06-17: PRs #475, #479, #481 merged — Cast+array copy in LuceneQuery, IndexItems fast-path, string[] copy
- 2026-06-19: PR #490 merged — ThreadStatic HashSet + single-pass SearchableFields
- 2026-06-25: PR #509 merged — single-pass field collection in CreateSearchResult (O(N²)→O(N))
- 2026-06-25: PR #512 merged (by perf-improver) — cached system field types in AddDocument + precompute TimeSpan constants
- 2026-06-29: PR #515 merged — LINQ state-machine elimination in GroupedAnd/Or/Not + MultiSearchContext
- 2026-06-29: PR #519 merged — redundant .ToLower() in GenerateHash + lambda/StringBuilder allocs in RemoveStopWords
- 2026-06-29: PR #521 merged — StringComparison.Ordinal in AddDocument StartsWith + ToLowerInvariant in IsStandardAnalyzerStopWord
- 2026-06-29: PR #522 merged — cache default field-type factories in AddDocument loop
- 2026-06-30: PR #517 merged — reflection→pattern matching in CheckQueryForExtractTerms + LINQ inline loop in ManagedQueryInternal
- 2026-06-30: PR created (branch: efficiency/simplify-searchresult-getvalues) — remove dead Values fallback in SearchResult.GetValues

## Backlog Cursor
- Next scan: check PR #518 status; explore remaining hot paths in SearchContext/LuceneSearchExecutor; investigate SearchableFields LINQ chain in SearchContext (still uses LINQ after PR #515); explore LuceneSearchQueryBase.GroupedAndInternal/GetMultiFieldQuery allocation patterns

## Last Run Tasks
- 2026-06-30: Task 3 (remove dead Values fallback in SearchResult.GetValues), Task 7 (update June 2026 issue #510)
