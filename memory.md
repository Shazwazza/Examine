# Perf Improver Memory - Shazwazza/Examine

## Validated Commands
```bash
# Build
dotnet build src/Examine.sln --configuration Release

# Test
dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --filter "TestCategory!=Benchmarks" -f net8.0

# Benchmarks (BenchmarkDotNet)
dotnet run --project src/Examine.Benchmarks --configuration Release

# Run specific benchmark
dotnet run --project src/Examine.Benchmarks --configuration Release -- --filter "*ManagedQuery*"
```

## Last Run Tasks (2026-08-17 20:44 run)
- Task 3: PR #569 (ordereddict-values, previous run) — CI passing (CodeQL success), no action needed.
- Task 3: Implemented Stack<BooleanQuery> initial capacity fix (LOW priority backlog item) — `new Stack<BooleanQuery>()` (default capacity 4) → `new Stack<BooleanQuery>(1)`. Measured via Stack<int> stand-in (allocation size independent of element type): 72.00 bytes/instance (default) → 64.00 bytes/instance (capacity 1), ~11% reduction, 2M-iteration micro-benchmark with GC.GetAllocatedBytesForCurrentThread(). Created draft PR on branch perf-assist/stack-booleanquery-capacity. Tests: 150 passed, 0 failed, 2 skipped. Build succeeded (0 errors).
- Task 7: Updated monthly activity issue #543

## Last Run Tasks (2026-08-17 earlier run)
- Task 2/3: Implemented remaining LOW-priority backlog item — OrderedDictionary.Values LINQ Select+ToArray → direct array copy. Measured -68% allocs/call (728→232 bytes), -45% time (2M-call micro-benchmark, GC.GetAllocatedBytesForCurrentThread). Created draft PR branch perf-assist/ordereddict-values (PR #569). Tests: 150 passed, 0 failed, 2 skipped.
- Task 7: Updated monthly activity issue #543

## Last Run Tasks (2026-08-14)
- Task 2: Scanned codebase — backlog exhausted, no new high-priority opportunities
- Task 7: Updated August 2026 monthly activity issue #543

## Optimization Backlog
| Priority | Area | Opportunity |
|----------|------|-------------|
| DONE | LuceneSearchExecutor | Redundant LastOrDefault() + new object[0] in search-after path (PR #441, merged) |
| DONE | LuceneSearchExecutor | CheckQueryForExtractTerms reflection + Fields?.ToArray() copy (PR #445, merged) |
| DONE | LuceneSearchExecutor | Minor array allocs (PR #457, merged 2026-06-04) |
| DONE | GroupedAnd/Or/Not + CreateSearchResult | PR #462 — reduce allocations (merged 2026-06-17) |
| DONE | ValueSet constructor | PR #469 — eliminate intermediate dict + generator allocs (merged 2026-06-17) |
| DONE | BaseIndexProvider.IndexItems | PR #479 — fast-path when no validator (merged 2026-06-17) |
| DONE | LuceneSearchExecutor | PR #506 — inline GetMaxDoc() + TryAdd in CreateSearchResult (merged 2026-06-24) |
| DONE | LuceneIndex.AddDocument | PR #512 — cache 3 system field value types (merged 2026-06-25) |
| DONE | LuceneIndex.AddDocument + LuceneSearchExecutor | PR #516 — cache 2 loop factories + early BooleanQuery return + Array.Empty (MERGED 2026-06-30) |
| DONE | SearchContext + LuceneIndex | PR #520 — factory cache + Ordinal StartsWith (MERGED 2026-06-29) |
| DONE | Benchmark infra | PR #524 — ManagedQueryBenchmarks (MERGED 2026-06-30) |
| DONE | LuceneQuery GroupedAnd/Or/Not | efficiency-improver PR #518 — LINQ → for-loop (MERGED 2026-07-08) |
| DONE | StringExtensions | efficiency-improver PR #534 — dead code removed (MERGED 2026-07-08) |
| DONE | FieldValueTypeCollection.GetValueType | PR #527 — GetOrAdd TArg overload, static lambda (MERGED 2026-07-29) |
| DONE | SearchResult Lazy<T> | PR #532 — eliminate Lazy<T>+closure per result; −14.9 KB/query (−4%) (MERGED 2026-07-30) |
| DONE | SearchResult.GetValues | efficiency-improver PR #526 — dead Values fallback removed (MERGED 2026-07-29) |
| DONE | MultiIndexSearcher | efficiency-improver PR #529 — Lazy<LuceneSearcher[]> + for-loop (MERGED 2026-07-29) |
| DONE | LuceneSearchQuery.Field<T> | efficiency-improver PR #531 — string overload eliminates new[]{fieldName} (MERGED 2026-07-29) |
| DONE | FullTextType + GenericAnalyzerFieldValueType | PR #542 closed (not merged), but changes present in codebase |
| DONE | LuceneSearchQuery.Search() | efficiency-improver PR #537 — cache category filter query (MERGED 2026-08-11) |
| DONE | LuceneSearchQueryBase SortFields | efficiency-improver PR #536 — lazy-init SortFields (MERGED 2026-08-11) |
| DONE | SearchContext.SearchableFields | efficiency-improver PR #545 — foreach loop (MERGED 2026-08-13) |
| DONE | ValueSet constructor | efficiency-improver PR #541 — eliminate LINQ ToDictionary allocs (MERGED 2026-08-13) |
| DONE | ReadOnlyFieldDefinitionCollection + LuceneIndex.GetFieldNames | efficiency-improver PR #546 — replace GroupBy + materialize ToArray (MERGED 2026-08-13) |
| LOW | OrderedDictionary.Values | DONE — replaced LINQ Select+ToArray with direct array copy (PR #569, 2026-08-17) |
| LOW | Stack<BooleanQuery> initial capacity | DONE — capacity 4→1, ~11% alloc reduction on Queries stack (draft PR, this run 2026-08-17) |
| EXHAUSTED | ManagedQueryInternal LateBoundQuery | Closure captures _searchContext + fields — inherent to lazy eval, no good fix |
| EXHAUSTED | CreateSearchResult closure | Captures `doc` — required for lazy field loading; Lazy<T> wrapper eliminated by PR #532 |
| EXHAUSTED | ExamineValue boxing | ExamineValue struct boxed to IExamineValue — inherent to interface design |

## Completed Work
- 2026-05-25: PR #441 merged
- 2026-05-29: PR #445 merged
- 2026-06-04: PR #457 merged
- 2026-06-17: PR #462, #469, #479 merged
- 2026-06-24: PR #506 merged
- 2026-06-25: PR #512 merged
- 2026-06-30: PR #516, #524 merged
- 2026-06-29: PR #520 merged
- 2026-07-08: PR #518, #534 merged
- 2026-07-29: PR #527, #526, #529, #531 merged
- 2026-07-30: PR #532 merged (−14.9 KB/query, −4%)
- 2026-08-11: PR #537, #536 merged
- 2026-08-13: PR #541, #545, #546 merged; PR #542 closed (changes in codebase)

## Open PRs
- PR #569 (perf-assist/ordereddict-values) — CI passing as of 2026-08-17, awaiting maintainer review
- New PR (perf-assist/stack-booleanquery-capacity) — created this run 2026-08-17, PR number TBD (check next run)

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~150 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x (at 55978e9 as of 2026-08-14)
- Targets net6.0;net8.0 — GetOrAdd<TArg> available on both (since .NET Core 2.0)
- Benchmark suite covers: concurrent search, bulk indexing, concurrent searcher acquire, QueryBuilder, ValueSet ctor, ManagedQuery
- Monthly issue: August 2026 #543 (updated 2026-08-14)
- efficiency-improver bot merged PRs #545, #546, #541, #537, #536 between 2026-08-11 and 2026-08-13
- Backlog exhausted: remaining items are LOW priority or EXHAUSTED
- ManagedQueryAllFields benchmark: ~371 KB per query execution (baseline), ~356 KB after SearchResult Lazy<T> elimination PR
- SHALLOW CLONE ISSUE: CI checkout is shallow (depth:1 for default branch). git rebase fails with "unrelated histories". Workaround: cherry-pick PR change onto fresh branch from origin/support/3.x and create a new PR.
- GetOrAdd<TArg> pattern: use static lambda + TArg state to avoid closure allocs in ConcurrentDictionary hot paths
- ExamineValue is readonly struct — no heap alloc when created, but boxed when passed as IExamineValue interface
- 2026-08-17: Repo default branch fetched cleanly this run (no shallow-clone issue hit) — origin/support/3.x fetched with --depth=5 successfully, branch created directly off it
- OrderedDictionary.Values micro-benchmark technique: standalone console app referencing Examine.Core.csproj, GC.GetAllocatedBytesForCurrentThread() before/after N iterations, compare via git stash for baseline vs optimized
