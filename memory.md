# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-07-22

## Build/Test Commands (Validated)
- Restore: `dotnet restore src/Examine.sln`
- Build: `dotnet build src/Examine.sln --configuration Release`
- Test: `dotnet test src/Examine.Test/Examine.Test.csproj -f net8.0`
- Benchmarks: `dotnet run --project src/Examine.Benchmarks --configuration Release`
- FieldQuery benchmarks: `dotnet run --project src/Examine.Benchmarks --configuration Release -- --filter "*FieldQuery*"`

## Efficiency Notes
- Hot path: `CreateSearchResult` (now O(N) single-pass after PR #509)
- Hot path: `AddDocument` - system field value types now cached after PR #512
- Hot path: `AddDocument` - default field-type factories now cached (PR #522)
- Hot path: `CheckQueryForExtractTerms` - reflection replaced with pattern matching (PR #517)
- Hot path: `ManagedQueryInternal` - LINQ state-machine eliminated (PR #517)
- `SearchResult.GetValues` - dead Values fallback to be removed (PR #526 open)
- `LuceneSearchQueryBase.GroupedAnd/Or/Not` - LINQ state-machine allocs eliminated (PR #515, merged)
- `MultiSearchContext`: LINQ state-machine allocs eliminated (PR #515, merged)
- `LuceneQuery.GroupedAnd/Or/Not`: LINQ state-machine allocs eliminated (PR #518, MERGED 2026-07-08)
- `GenerateHash`: removed redundant .ToLower() after "x2" format (PR #519, merged)
- `RemoveStopWords`: Action delegate/innerBuilder/string-concat allocs eliminated (PR #519, merged)
- `AddDocument`: field.Key.StartsWith uses StringComparison.Ordinal (PR #521, merged)
- `IsStandardAnalyzerStopWord`: ToLowerInvariant() replaces ToLower() (PR #521, merged)
- `MultiIndexSearcher.GetSearchContext()`: Lazy<LuceneSearcher[]> caches array; for-loop eliminates SelectIterator (PR #529 open)
- `Field<T>` / `FieldNested<T>`: new single-field RangeQueryInternal<T> overload eliminates string[1] alloc per call (PR #531 open)
- `StringExtensions.EnsureEndsWith` + `ReplaceNonAlphanumericChars`: dead internal code removed (PR #534, MERGED 2026-07-08)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- PRs from perf-improver (separate bot): #527, #533 (open), #540 (new, open 2026-07-22)
- Benchmark infrastructure: `FieldQueryBenchmarks.cs` added (PR #535, open) — measures `Field<int>` typed query allocs vs NuGet versions
- `LuceneSearchQueryBase.SortFields`: lazy-init `List<SortField>` (PR #536 open) — 1 list alloc eliminated per unsorted query (common path)
- `LuceneSearchQuery.Search()`: category TermQuery cached via `??=`; eliminates ExamineValue boxing + TermQuery alloc per categorised Execute() call (PR #537 open)
- Scanned 2026-07-22: no new commits on support/3.x; all known opportunities covered by open PRs

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | Dead `Values` fallback in `SearchResult.GetValues` | Eliminates _fields lazy-init + array alloc on cache-miss | PR #526 open |
| OPEN | Code-Level | MultiIndexSearcher LINQ allocs per search | 2 LINQ iterator allocs eliminated per search | PR #529 open |
| OPEN | Code-Level | string[1] alloc in Field<T> across 4 call sites | 1 array alloc eliminated per Field<T> call | PR #531 open |
| OPEN | Code-Level | List<SortField> eager alloc per unsorted query | 1 list alloc eliminated per query (common path) | PR #536 open |
| OPEN | Code-Level | Category TermQuery recreated per Execute() | ~56-64 B eliminated per categorised search | PR #537 open |
| INFRA | Measurement | FieldQueryBenchmarks for typed Field<T> hot path | NuGet-version benchmark fills gap in benchmark suite | PR #535 open |
| LOW | Code-Level | OrderedDictionary.Values LINQ Select+ToArray | Minor alloc savings; not confirmed hot path | Not pursuing |
| LOW | Code-Level | ObjectExtensions.ConvertObjectToDictionary LINQ | reflection-dominated, skip | Not worth pursuing |

## Completed Work
- 2026-06-25: PR #509 merged — single-pass field collection in CreateSearchResult (O(N²)→O(N))
- 2026-06-25: PR #512 merged — cached system field types in AddDocument + precompute TimeSpan constants
- 2026-06-29: PR #515 merged — LINQ state-machine elimination in GroupedAnd/Or/Not + MultiSearchContext
- 2026-06-29: PR #519 merged — redundant .ToLower() in GenerateHash + lambda/StringBuilder allocs in RemoveStopWords
- 2026-06-29: PR #521 merged — StringComparison.Ordinal in AddDocument StartsWith + ToLowerInvariant in IsStandardAnalyzerStopWord
- 2026-06-29: PR #522 merged — cache default field-type factories in AddDocument loop
- 2026-06-30: PR #517 merged — reflection→pattern matching in CheckQueryForExtractTerms + LINQ inline loop in ManagedQueryInternal
- 2026-07-08: PR #518 MERGED — LuceneQuery GroupedAnd/Or/Not LINQ allocs
- 2026-07-08: PR #534 MERGED — dead string extension methods removed

## Monthly Issues
- June 2026: #510 (closed 2026-07-01)
- July 2026: #530 (open)

## Backlog Cursor
- All high-impact code-level patterns addressed; open PRs cover remaining known opportunities
- Next logical step: wait for maintainer to merge/review open PRs

## Last Run Tasks
- 2026-07-22: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open; perf-improver PR #540 new (not ours)
- 2026-07-21: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs still open
