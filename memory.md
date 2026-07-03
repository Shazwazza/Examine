# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-07-03

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
- `SearchResult.GetValues` - dead Values fallback removed (PR #526 open)
- `LuceneSearchQueryBase.GroupedAnd/Or/Not` - LINQ state-machine allocs eliminated (PR #515, merged)
- `MultiSearchContext`: LINQ state-machine allocs eliminated (PR #515, merged)
- `LuceneQuery.GroupedAnd/Or/Not`: LINQ state-machine allocs eliminated (PR #518, open)
- `GenerateHash`: removed redundant .ToLower() after "x2" format (PR #519, merged)
- `RemoveStopWords`: Action delegate/innerBuilder/string-concat allocs eliminated (PR #519, merged)
- `AddDocument`: field.Key.StartsWith uses StringComparison.Ordinal (PR #521, merged)
- `IsStandardAnalyzerStopWord`: ToLowerInvariant() replaces ToLower() (PR #521, merged)
- `MultiIndexSearcher.GetSearchContext()`: Lazy<LuceneSearcher[]> caches array; for-loop eliminates SelectIterator (PR #529 open)
- `Field<T>` / `FieldNested<T>`: new single-field RangeQueryInternal<T> overload eliminates string[1] alloc per call (PR #531 open)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- `StringExtensions.ReplaceNonAlphanumericChars` is dead code (no callers found) with O(N²) pattern
- `StringExtensions.EnsureEndsWith` is dead code (no callers found)
- PRs from perf-improver: #516 (cache FieldValueType factories + early-return BooleanQuery + Array.Empty sort), #524 (ManagedQueryBenchmarks), #525 (benchmark results published), #527 (FieldValueTypeCollection.GetValueType GetOrAdd TArg overload)

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | LINQ state-machine allocs in LuceneQuery GroupedAnd/Or/Not | 6 SelectIterator allocs eliminated | PR #518 open |
| OPEN | Code-Level | Dead Values fallback in SearchResult.GetValues | Eliminates _fields lazy-init + array alloc on miss | PR #526 open |
| OPEN | Code-Level | MultiIndexSearcher LINQ allocs per search | 2 LINQ iterator allocs eliminated per search | PR #529 open |
| OPEN | Code-Level | string[1] alloc in Field<T> across 4 call sites | 1 array alloc eliminated per Field<T> call | PR #531 open |
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
- 2026-06-30: PR #526 created (branch: efficiency/simplify-searchresult-getvalues) — remove dead Values fallback in SearchResult.GetValues
- 2026-07-01: PR #529 created (branch: efficiency/cache-multiindexsearcher-array) — cache LuceneSearcher[] in MultiIndexSearcher; eliminate 2 LINQ iterator allocs per search
- 2026-07-02: PR #531 created (branch: efficiency/eliminate-single-field-range-array-alloc-29b5fcb5659a9be2) — single-field RangeQueryInternal<T> overload; eliminate string[1] alloc per Field<T> call
- 2026-07-03: Maintenance run — verified CI on all 4 open PRs, scanned remaining hot paths

## Monthly Issues
- June 2026: #510 (closed 2026-07-01)
- July 2026: #530 (open)

## Backlog Cursor
- Next scan: after open PRs merge, look for any remaining alloc patterns; LOW priority dead code cleanup of StringExtensions (ReplaceNonAlphanumericChars, EnsureEndsWith); consider expanding benchmarks to cover Field<T> single-field range query path

## Last Run Tasks
- 2026-07-03: Task 4 (checked PR CI — all clean: #518, #526, #529, #531), Task 2 (scanned remaining hot paths; no new high-impact opportunities; confirmed StringExtensions dead code LOW priority), Task 7 (updated July 2026 issue #530)
