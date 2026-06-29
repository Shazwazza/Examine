# Perf Improver Memory - Shazwazza/Examine

## Validated Commands
```bash
# Build
dotnet build src/Examine.sln --configuration Release

# Test
dotnet test src/Examine.Test/Examine.Test.csproj --configuration Release --filter "TestCategory!=Benchmarks" -f net8.0

# Benchmarks (BenchmarkDotNet)
dotnet run --project src/Examine.Benchmarks --configuration Release
```

## Last Run Tasks (2026-06-29)
- Task 4: PRs #516 and #520 checked — both clean, no CI failures, no conflicts
- Task 2: Noted overlap between PR #520 and efficiency-improver PR #521 (same StringComparison.Ordinal on same AddDocument line)
- Task 7: Updated June 2026 monthly activity issue #513 (fixed #aw_pr518 → #520)

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
| OPEN PR | LuceneIndex.AddDocument + LuceneSearchExecutor | PR #516 — cache 2 loop factories + early BooleanQuery return + Array.Empty |
| OPEN PR | SearchContext + LuceneIndex | PR #520 — factory cache + Ordinal StartsWith (NOTE: conflicts with efficiency-improver PR #521 on same StartsWith line) |
| LOW | OrderedDictionary.Values | Allocates TVal[] via LINQ on every access — not clearly on hot path |
| NOTE | MultiSearchContext | LINQ allocs — efficiency-improver PR #515 already covers this |
| NOTE | ManagedQueryInternal LINQ | efficiency-improver PR #517 covers reflection + LINQ here |
| NOTE | LuceneQuery GroupedAnd/Or/Not | efficiency-improver PR #518 covers this |
| NOTE | GenerateHash/RemoveStopWords | efficiency-improver PR #519 covers redundant ToLower + allocs |
| NOTE | AddDocument+CreateSearcher StringComparison | efficiency-improver PR #521 covers this (overlaps with PR #520) |

## Completed Work
- 2026-05-25: PR #441 merged
- 2026-05-29: PR #445 merged
- 2026-06-04: PR #457 merged
- 2026-06-17: PR #462 merged
- 2026-06-17: PR #469 merged
- 2026-06-17: PR #479 merged
- 2026-06-24: PR #506 merged
- 2026-06-25: PR #512 merged
- 2026-06-26: PR #516 created (open)
- 2026-06-28: PR #520 created (open)

## Open PRs (awaiting maintainer review)
- #516: cache factory lookups in AddDocument + early-return BooleanQuery + Array.Empty<SortField>()
- #520: cache FullText factory in SearchContext.GetFieldValueType + StringComparison.Ordinal for StartsWith

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~150 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
- Targets net6.0;net8.0 — Dictionary.TryAdd available on both
- Benchmark suite covers: concurrent search (1/25/100 threads), bulk indexing (IndexVersionComparison), concurrent searcher acquire, QueryBuilder, ValueSet ctor
- Monthly issue #513 open (June 2026) — last updated 2026-06-29
- Maintainer keeps closing monthly activity issues as "not_planned" — note this pattern
- efficiency-improver bot also working in parallel on similar areas (LINQ allocs, etc.)
- ExamineValue is readonly struct — no heap alloc when created, but boxed when passed as IExamineValue interface
- LuceneSearchQuery creates new instance per search — no caching of category query possible
- IndexingItemEventArgs + DocumentWritingEventArgs are virtual methods — can't skip allocation based on null event
- Backlog now nearly exhausted; remaining low-priority item is OrderedDictionary.Values (not hot path)
- PR #520 conflicts with efficiency-improver PR #521 on AddDocument.StartsWith change — trivial rebase once one merges
