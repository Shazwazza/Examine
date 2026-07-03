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

## Last Run Tasks (2026-07-03)
- Task 3: Created PR #aw_pr_sr_lazy (new PR): eliminate Lazy<T> + inner closure per SearchResult — measured −14.9 KB/query (−4.0%, ManagedQueryAllFields Source: 371.68→356.82 KB); all 150 tests pass
- Task 4: PR #527 still open, base up-to-date, no action needed
- Task 5: No open performance issues found (only #528 = monthly issue)
- Task 7: Updated July 2026 monthly activity issue #528

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
| OPEN PR | SearchResult Lazy<T> | PR #aw_pr_sr_lazy — eliminate Lazy<T>+closure per result; −14.9 KB/query (−4%) |
| OPEN PR | FieldValueTypeCollection.GetValueType | PR #527 — GetOrAdd TArg overload, static lambda — eliminates 1 closure/call |
| NOTE | LuceneQuery GroupedAnd/Or/Not | efficiency-improver PR #518 covers LINQ → for-loop (still open) |
| NOTE | SearchResult.GetValues | efficiency-improver PR #526 covers dead Values fallback (still open) |
| NOTE | MultiIndexSearcher | efficiency-improver PR #529 covers LINQ allocs per search (still open) |
| LOW | OrderedDictionary.Values | Allocates TVal[] via LINQ on every access — not on hot path |
| LOW | ExamineValue boxing | ToExamineValues boxes struct per value (~32B/value); small savings only |
| ANALYZED | CreateSearchResult closures | Lazy<>+2 closures per result (~64B each) — eliminates by changing ctor, but requires API work |
| ANALYZED | AddDocument TryGetValue fast path | Case-sensitivity in _resolvedValueTypes vs FieldDefinitions makes this tricky; savings minimal |

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

## Open PRs (awaiting maintainer review)
- PR #aw_pr_sr_lazy: SearchResult Lazy<T> elimination (−14.9 KB/query −4%)
- PR #527: FieldValueTypeCollection TArg optimization (GetOrAdd static lambda)

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~150 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
- Targets net6.0;net8.0 — GetOrAdd<TArg> available on both (since .NET Core 2.0)
- Benchmark suite covers: concurrent search (1/25/100 threads), bulk indexing, concurrent searcher acquire, QueryBuilder, ValueSet ctor, ManagedQuery
- Monthly issue: July 2026 (#528); June 2026 issue (#513) closed
- efficiency-improver bot also working in parallel: PRs #518, #526, #529 still open
- Lucene.NET upgraded to 4.8.0-beta00018 on 2026-06-29 (#523)
- ExamineValue is readonly struct — no heap alloc when created, but boxed when passed as IExamineValue interface
- GetOrAdd<TArg> pattern: use static lambda + TArg state to avoid closure allocs in ConcurrentDictionary hot paths
- ManagedQueryAllFields benchmark: ~371 KB per query execution (baseline), ~356 KB after SearchResult Lazy<T> elimination PR
- CreateSearchResult: measured Lazy<T>+closure overhead at ~15 KB per 1000 results (not ~64B as estimated — GC allocation not purely proportional to object size)
- _resolvedValueTypes dict uses ordinal string comparison; FieldDefinitions uses InvariantCultureIgnoreCase — mismatch prevents simple TryGetValue fast-path for defined fields
- TODO in ProcessIndexQueueItem: Document reuse could save GC but complex Lucene field lifecycle requirements
