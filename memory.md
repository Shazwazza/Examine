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

## Last Run Tasks (2026-06-30)
- Task 4: PRs #516 and #520 both MERGED ✅ (by Shazwazza)
- Task 6: Added ManagedQueryBenchmarks.cs (PR #525 pending — actual number TBD by safe-output)
- Task 7: Updated June 2026 monthly activity issue #513

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
| OPEN PR | Benchmark infra | ManagedQueryBenchmarks.cs — covers ManagedQuery hot path for version comparison |
| NOTE | MultiSearchContext | LINQ allocs — efficiency-improver PR #515 already covers this |
| NOTE | ManagedQueryInternal LINQ | efficiency-improver PR #517 covers reflection + LINQ here |
| NOTE | LuceneQuery GroupedAnd/Or/Not | efficiency-improver PR #518 covers this |
| NOTE | GenerateHash/RemoveStopWords | efficiency-improver PR #519 covers redundant ToLower + allocs |
| NOTE | AddDocument+CreateSearcher StringComparison | efficiency-improver PR #521 covers this (merged into #520) |
| LOW | OrderedDictionary.Values | Allocates TVal[] via LINQ on every access — not on hot path |

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
- 2026-06-30: ManagedQueryBenchmarks PR created (number TBD)

## Open PRs (awaiting maintainer review)
- ManagedQueryBenchmarks PR (new): adds benchmark for ManagedQuery/BaseLuceneSearcher.Search hot path
- efficiency-improver #517: reflection → pattern matching + LINQ → inline loop (ManagedQueryInternal)
- efficiency-improver #518: LINQ in LuceneQuery GroupedAnd/Or/Not

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~150 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
- Targets net6.0;net8.0 — Dictionary.TryAdd available on both
- Benchmark suite covers: concurrent search (1/25/100 threads), bulk indexing, concurrent searcher acquire, QueryBuilder, ValueSet ctor, ManagedQuery (NEW)
- Monthly issue #513 open (June 2026) — updated 2026-06-30
- Backlog now exhausted of hot-path wins; remaining items are LOW priority or covered by efficiency-improver
- efficiency-improver bot also working in parallel on similar areas (LINQ allocs, etc.)
- ExamineValue is readonly struct — no heap alloc when created, but boxed when passed as IExamineValue interface
- ManagedQuery wraps query in LateBoundQuery — GetFieldValueType called during Execute() not during build
- NugetConfig compares Source vs 3.3.0, 3.2.1, 3.1.0, 3.0.1
