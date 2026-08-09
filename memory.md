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

## Last Run Tasks (2026-08-09)
- Task 4: PR #542 CI passing, base still at `fd63863` — no action needed
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
| OPEN PR | FullTextType + GenericAnalyzerFieldValueType | PR #542 — cache sortable field name (supersedes #540, #533) |
| NOTE | LuceneSearchQuery.Search() | efficiency-improver PR #537 — cache category filter query (still open) |
| NOTE | LuceneSearchQueryBase SortFields | efficiency-improver PR #536 — lazy-init SortFields (still open) |
| NOTE | FieldQueryBenchmarks | efficiency-improver PR #535 — adds FieldQueryBenchmarks (still open) |
| NOTE | ValueSet constructor | efficiency-improver PR #541 — eliminate LINQ ToDictionary allocs (still open) |
| NOTE | Various | efficiency-improver PRs #545, #546 also still open |
| LOW | OrderedDictionary.Values | Allocates TVal[] via LINQ on every access — not on hot path |
| EXHAUSTED | ManagedQueryInternal LateBoundQuery | Closure captures _searchContext + fields — inherent to lazy eval, no good fix |
| EXHAUSTED | CreateSearchResult closure | Captures `doc` — required for lazy field loading; Lazy<T> wrapper eliminated by PR #532 |

## Completed Work
- 2026-05-25: PR #441 merged
- 2026-05-29: PR #445 merged
- 2026-06-04: PR #457 merged
- 2026-06-17: PR #462 merged
- 2026-06-17: PR #469 merged
- 2026-06-17: PR #479 merged
- 2026-06-24: PR #506 merged
- 2026-06-25: PR #512 merged
- 2026-06-30: PR #516 merged
- 2026-06-29: PR #520 merged
- 2026-06-30: PR #524 (ManagedQueryBenchmarks) merged by Shazwazza
- 2026-07-08: PR #518 (efficiency-improver) merged — LuceneQuery GroupedAnd/Or/Not LINQ → for-loop
- 2026-07-08: PR #534 (efficiency-improver) merged — dead StringExtensions removed
- 2026-07-29: PR #527 merged by Shazwazza — FieldValueTypeCollection closure elimination
- 2026-07-29: PR #526 (efficiency-improver) merged — dead SearchResult.GetValues fallback removed
- 2026-07-29: PR #529 (efficiency-improver) merged — MultiIndexSearcher Lazy<LuceneSearcher[]>
- 2026-07-29: PR #531 (efficiency-improver) merged — Field<T> single-element string[1] alloc eliminated
- 2026-07-30: PR #532 merged by Shazwazza — SearchResult Lazy<T> + inner closure elimination (−14.9 KB/query, −4%)

## Open PRs (awaiting maintainer review)
- PR #542: Cache sortable field name — supersedes #540 and #533; maintainer should close both after merging
- PR #533: Superseded by #540 and now by #542 — maintainer should close
- PR #540: Superseded by #542 — maintainer should close

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~150 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x (at fd63863 as of 2026-08-06)
- Targets net6.0;net8.0 — GetOrAdd<TArg> available on both (since .NET Core 2.0)
- Benchmark suite covers: concurrent search, bulk indexing, concurrent searcher acquire, QueryBuilder, ValueSet ctor, ManagedQuery
- Monthly issue: August 2026 #543 (updated 2026-08-06)
- efficiency-improver bot also working in parallel: PRs #535, #536, #537, #541, #545, #546 still open
- Lucene.NET upgraded to 4.8.0-beta00018 on 2026-06-29 (#523)
- ExamineValue is readonly struct — no heap alloc when created, but boxed when passed as IExamineValue interface
- GetOrAdd<TArg> pattern: use static lambda + TArg state to avoid closure allocs in ConcurrentDictionary hot paths
- ManagedQueryAllFields benchmark: ~371 KB per query execution (baseline), ~356 KB after SearchResult Lazy<T> elimination PR
- Backlog exhausted: remaining items covered by efficiency-improver PRs or are LOW priority
- PR #532: thread-safety of null-check lazy-init same as existing Values getter; SearchResult not shared across threads
- SHALLOW CLONE ISSUE: CI checkout is shallow (depth:1 for default branch). git rebase fails with "unrelated histories". Workaround: cherry-pick PR change onto fresh branch from origin/support/3.x and create a new PR.
- PR #533 and #540 both superseded by #542 — maintainer to close both
