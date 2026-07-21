# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-07-21

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
- `ObjectExtensions.ConvertObjectToDictionary`: LINQ cast+where pattern noted as LOW priority (reflection-dominated, not hot path)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- PRs from perf-improver (separate bot): #527, #532, #533 (all open/awaiting review)
- Benchmark infrastructure: `FieldQueryBenchmarks.cs` added (PR #535, open) — measures `Field<int>` typed query allocs vs NuGet versions
- `LuceneSearchQueryBase.SortFields`: lazy-init `List<SortField>` (PR #536 open) — 1 list alloc eliminated per unsorted query (common path)
- `LuceneSearchQuery.Search()`: category TermQuery cached via `??=`; eliminates ExamineValue boxing + TermQuery alloc per categorised Execute() call (PR #537 open)
- `OrderedDictionary.Values`: creates new array via LINQ Select+ToArray on every call — LOW priority (not a confirmed hot path)
- Scanned 2026-07-12 through 2026-07-21: no new commits on support/3.x; all known opportunities covered by open PRs

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| OPEN | Code-Level | Dead Values fallback in SearchResult.GetValues | Eliminates _fields lazy-init + array alloc on miss | PR #526 open |
| OPEN | Code-Level | MultiIndexSearcher LINQ allocs per search | 2 LINQ iterator allocs eliminated per search | PR #529 open |
| OPEN | Code-Level | string[1] alloc in Field<T> across 4 call sites | 1 array alloc eliminated per Field<T> call | PR #531 open |
| OPEN | Code-Level | List<SortField> eager alloc per unsorted query | 1 list alloc eliminated per query (common path) | PR #536 open |
| OPEN | Code-Level | Category TermQuery recreated per Execute() | ~56-64 B eliminated per categorised search | PR #537 open |
| INFRA | Measurement | FieldQueryBenchmarks for typed Field<T> hot path | NuGet-version benchmark fills gap in benchmark suite | PR #535 open |
| LOW | Code-Level | OrderedDictionary.Values LINQ Select+ToArray | Minor alloc savings (1 SelectIterator); not confirmed hot path | Not pursuing |
| LOW | Code-Level | ObjectExtensions.ConvertObjectToDictionary LINQ | reflection-dominated, skip | Not worth pursuing |

## Completed Work
- 2026-05-25: PR #438 merged — CreateSearchResult dedup fix
- 2026-05-29: PR #446 merged — ValueSet redundant copy fix
- 2026-06-01: PR #448 merged — Cast<IExamineValue>() elimination
- 2026-06-03/04: PRs #457, #462 merged — LINQ alloc + GroupedAnd/Or reductions
- 2026-06-17: PRs #475, #479, #481 merged — Cast+array copy in LuceneQuery, IndexItems fast-path, string[] copy
- 2026-06-19: PR #490 merged — ThreadStatic HashSet + single-pass SearchableFields
- 2026-06-25: PR #509 merged — single-pass field collection in CreateSearchResult (O(N²)→O(N))
- 2026-06-25: PR #512 merged — cached system field types in AddDocument + precompute TimeSpan constants
- 2026-06-29: PR #515 merged — LINQ state-machine elimination in GroupedAnd/Or/Not + MultiSearchContext
- 2026-06-29: PR #519 merged — redundant .ToLower() in GenerateHash + lambda/StringBuilder allocs in RemoveStopWords
- 2026-06-29: PR #521 merged — StringComparison.Ordinal in AddDocument StartsWith + ToLowerInvariant in IsStandardAnalyzerStopWord
- 2026-06-29: PR #522 merged — cache default field-type factories in AddDocument loop
- 2026-06-30: PR #517 merged — reflection→pattern matching in CheckQueryForExtractTerms + LINQ inline loop in ManagedQueryInternal
- 2026-06-30: PR #526 created — remove dead Values fallback in SearchResult.GetValues
- 2026-07-01: PR #529 created — cache LuceneSearcher[] in MultiIndexSearcher; eliminate 2 LINQ iterator allocs per search
- 2026-07-02: PR #531 created — single-field RangeQueryInternal<T> overload; eliminate string[1] alloc per Field<T> call
- 2026-07-04: PR #534 created — remove dead EnsureEndsWith + ReplaceNonAlphanumericChars internal methods + 2 unused imports
- 2026-07-07: PR #535 created — FieldQueryBenchmarks.cs; NuGet-version benchmark for typed Field<T> query hot path
- 2026-07-08: PR #518 MERGED (Shazwazza) — LuceneQuery GroupedAnd/Or/Not LINQ allocs
- 2026-07-08: PR #534 MERGED (Shazwazza) — dead string extension methods removed
- 2026-07-08: PR #536 created — lazy-init SortFields; eliminate List<SortField> per unsorted query
- 2026-07-09: PR #537 created — cache category TermQuery in LuceneSearchQuery; eliminate ~56-64 B alloc per categorised Execute() call

## Monthly Issues
- June 2026: #510 (closed 2026-07-01)
- July 2026: #530 (open)

## Backlog Cursor
- All high-impact code-level patterns addressed; open PRs cover remaining known opportunities
- Scanned full codebase 2026-07-12 through 2026-07-21: no new commits on support/3.x; no new opportunities
- Next logical step: wait for maintainer to merge/review open PRs

## Last Run Tasks
- 2026-07-21: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open
- 2026-07-20: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open
- 2026-07-19: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open
- 2026-07-18: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open
- 2026-07-17: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open
- 2026-07-16: Task 7 (updated July 2026 issue #530); no new commits on support/3.x; all 6 efficiency-improver PRs (#526, #529, #531, #535, #536, #537) still open
