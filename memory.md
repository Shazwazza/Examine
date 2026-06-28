# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-06-28

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
- `CheckQueryForExtractTerms`: reflection replaced with pattern matching (PR #517 open)
- `ManagedQueryInternal`: Select+Where LINQ replaced with inline loop (PR #517 open)
- `LuceneQuery.GroupedAnd/Or/Not`: LINQ state machine allocs eliminated (PR #518 open)
- `GenerateHash`: removed redundant .ToLower() after "x2" format (PR #519 open)
- `RemoveStopWords`: Action delegate/innerBuilder/string-concat allocs eliminated (PR #519 open)
- `AddDocument`: field.Key.StartsWith uses StringComparison.Ordinal (PR #aw_pr_ord open)
- `IsStandardAnalyzerStopWord`: ToLowerInvariant() replaces ToLower() (PR #aw_pr_ord open)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- `StringExtensions.ReplaceNonAlphanumericChars` is dead code (no callers found) with O(N²) pattern
- `StringExtensions.EnsureEndsWith` is dead code (no callers found)
- PR #520 by perf-improver: caches FullText factory in SearchContext + StringComparison.Ordinal for StartsWith in SearchContext

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | LINQ state-machine allocs in GroupedAnd/Or/Not + MultiSearchContext | Eliminate 1 heap obj per grouped-query call | PR #515 open |
| OPEN | Code-Level | Reflection in CheckQueryForExtractTerms + LINQ in ManagedQueryInternal | 3 reflection calls + 2 LINQ allocs eliminated | PR #517 open |
| OPEN | Code-Level | LuceneQuery GroupedAnd/Or/Not LINQ allocs | 6 SelectIterator allocs eliminated | PR #518 open |
| OPEN | Code-Level | GenerateHash redundant .ToLower() + RemoveStopWords lambda/StringBuilder allocs | 16-20 string allocs saved per hash; delegate+StringBuilder per stop-word call | PR #519 open |
| OPEN | Code-Level | AddDocument StartsWith StringComparison.Ordinal + IsStandardAnalyzerStopWord ToLowerInvariant | Ordinal byte-compare replaces culture-aware compare in indexing hot path | PR #aw_pr_ord open |
| LOW | Code-Level | `StringExtensions.ReplaceNonAlphanumericChars` dead code with inefficient pattern | Cleanup if dead; or optimize if used | identified |
| LOW | Code-Level | `StringExtensions.EnsureEndsWith` dead code with alloc-heavy EndsWith | Cleanup if dead | identified |

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
- 2026-06-26: PR #517 created — reflection→pattern matching in CheckQueryForExtractTerms + LINQ inline loop in ManagedQueryInternal
- 2026-06-27: PR #518 created — LINQ state-machine elimination in LuceneQuery (6 sites)
- 2026-06-27: PR #519 created — redundant .ToLower() in GenerateHash + lambda/StringBuilder allocs in RemoveStopWords
- 2026-06-28: PR #aw_pr_ord created — StringComparison.Ordinal in AddDocument StartsWith + ToLowerInvariant in IsStandardAnalyzerStopWord

## Backlog Cursor
- Next scan: explore dead code removal (EnsureEndsWith, ReplaceNonAlphanumericChars); investigate caching GetRequiredFactory(FullText) and GetRequiredFactory(InvariantCultureIgnoreCase) in AddDocument loop; check SearchResult.GetValues simplification

## Last Run Tasks
- 2026-06-28: Task 3 (StringComparison.Ordinal in AddDocument + ToLowerInvariant in stop-word check), Task 7 (update June 2026 issue #510)
