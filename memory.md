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

## Last Run Tasks (2026-06-24)
- Task 3: Created PR #aw_pr_maxdoc (inline GetMaxDoc + TryAdd in CreateSearchResult)
- Task 7: Created new June 2026 monthly activity issue #aw_monthly_jun2026b (previous #458 closed by @Shazwazza on 2026-06-18)

## Optimization Backlog
| Priority | Area | Opportunity |
|----------|------|-------------|
| DONE | LuceneSearchExecutor | Redundant LastOrDefault() + new object[0] in search-after path (PR #441, merged) |
| DONE | LuceneSearchExecutor | CheckQueryForExtractTerms reflection + Fields?.ToArray() copy (PR #445, merged) |
| DONE | LuceneSearchExecutor | Minor array allocs (PR #457, merged 2026-06-04) |
| DONE | GroupedAnd/Or/Not + CreateSearchResult | PR #462 — reduce allocations (merged 2026-06-17) |
| DONE | ValueSet constructor | PR #469 — eliminate intermediate dict + generator allocs (merged 2026-06-17) |
| DONE | BaseIndexProvider.IndexItems | PR #479 — fast-path when no validator (merged 2026-06-17) |
| OPEN PR | LuceneSearchExecutor | PR #aw_pr_maxdoc — inline GetMaxDoc() + TryAdd in CreateSearchResult (branch: perf-assist/inline-getmaxdoc-and-tryadd) |
| MEDIUM | LuceneIndex.AddDocument | Cache 3 fixed system field value types (GetValueType×3 via ConcurrentDictionary.GetOrAdd per doc) |
| LOW | CheckQueryForExtractTerms | Early-out for BooleanQuery with empty clauses |
| LOW | SearchContext | SearchableFields two LINQ passes on first load (cached, very minor) |
| LOW | LuceneIndex.ScheduleCommit | TimeSpan.FromMilliseconds constants not cached as static readonly; double DateTime.Now read |
| LOW | GetFieldInternalQuery | Convert.ToInt32(float) → (int)float cast |

## Completed Work
- 2026-05-25: PR #441 merged
- 2026-05-29: PR #445 merged
- 2026-06-04: PR #457 merged
- 2026-06-17: PR #462 merged
- 2026-06-17: PR #469 merged
- 2026-06-17: PR #479 merged

## Open PRs (awaiting maintainer review)
- #aw_pr_maxdoc: inline GetMaxDoc() (remove second SearcherManager acquire) + TryAdd in CreateSearchResult

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~148 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
- Targets net6.0;net8.0 — Dictionary.TryAdd available on both
- Benchmark suite covers: concurrent search (1/25/100 threads), bulk indexing, concurrent searcher acquire
- Benchmark gaps: query construction (GroupedAnd/Or/Not), single-threaded search, field access
- Monthly issue #458 closed by @Shazwazza on 2026-06-18; new issue #aw_monthly_jun2026b created 2026-06-24
