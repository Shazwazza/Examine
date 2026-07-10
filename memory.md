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

## Last Run Tasks (2026-07-10)
- Task 4: PR #533 was "behind" (based on e39c592, branch now at 0ee95db) — recreated changes on current tip and pushed updated branch
- Task 7: Updated July 2026 monthly activity issue #528 with run entry

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
| OPEN PR | SearchResult Lazy<T> | PR #532 — eliminate Lazy<T>+closure per result; −14.9 KB/query (−4%) |
| OPEN PR | FieldValueTypeCollection.GetValueType | PR #527 — GetOrAdd TArg overload, static lambda — eliminates 1 closure/call |
| OPEN PR | FullTextType + GenericAnalyzerFieldValueType | PR #533 — cache sortable field name; eliminates 1 string alloc per doc per sortable field |
| NOTE | SearchResult.GetValues | efficiency-improver PR #526 covers dead Values fallback (still open) |
| NOTE | MultiIndexSearcher | efficiency-improver PR #529 covers LINQ allocs per search (still open) |
| NOTE | LuceneSearchQuery.Field<T> | efficiency-improver PR #531 covers new[] {fieldName} single-element alloc (still open) |
| LOW | OrderedDictionary.Values | Allocates TVal[] via LINQ on every access — not on hot path |
| EXHAUSTED | ManagedQueryInternal LateBoundQuery | Closure captures _searchContext + fields — inherent to lazy eval, no good fix |
| EXHAUSTED | CreateSearchResult closure | Captures `doc` — required for lazy field loading; PR #532 eliminates Lazy<T> wrapper |

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

## Open PRs (awaiting maintainer review)
- PR #532: SearchResult Lazy<T> elimination (−14.9 KB/query −4%) — maintainer commented 2026-07-08; Copilot SWE confirmed thread-safety rationale
- PR #527: FieldValueTypeCollection TArg optimization (GetOrAdd static lambda)
- PR #533: cache sortable field name in FullTextType + GenericAnalyzerFieldValueType — rebased 2026-07-10

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~150 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x (at 0ee95db as of 2026-07-08)
- Targets net6.0;net8.0 — GetOrAdd<TArg> available on both (since .NET Core 2.0)
- Benchmark suite covers: concurrent search, bulk indexing, concurrent searcher acquire, QueryBuilder, ValueSet ctor, ManagedQuery
- Monthly issue: July 2026 (#528); June 2026 issue (#513) closed
- efficiency-improver bot also working in parallel: PRs #526, #529, #531 still open
- Lucene.NET upgraded to 4.8.0-beta00018 on 2026-06-29 (#523)
- ExamineValue is readonly struct — no heap alloc when created, but boxed when passed as IExamineValue interface
- GetOrAdd<TArg> pattern: use static lambda + TArg state to avoid closure allocs in ConcurrentDictionary hot paths
- ManagedQueryAllFields benchmark: ~371 KB per query execution (baseline), ~356 KB after SearchResult Lazy<T> elimination PR
- Backlog exhausted: remaining items covered by efficiency-improver PRs #526/#529/#531 or are LOW priority
- PR #532: thread-safety of null-check lazy-init same as existing Values getter; SearchResult not shared across threads
- PR #533: FullTextType._sortableFieldName cached once in ctor (FieldName immutable); rebased onto 0ee95db 2026-07-10
