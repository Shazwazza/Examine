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

## Last Run Tasks (2026-06-25)
- Task 3: Created PR #aw_pr_cachefields (cache system field value types in AddDocument + precompute TimeSpan constants in IndexCommiter)
- Task 7: Created new June 2026 monthly activity issue

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
| OPEN PR | LuceneIndex.AddDocument | PR #aw_pr_cachefields — cache 3 system field value types + precompute TimeSpan constants |
| LOW | CheckQueryForExtractTerms | Early-out for BooleanQuery with empty clauses |
| LOW | SearchContext | SearchableFields two LINQ passes on first load (cached, very minor) |
| LOW | GetFieldInternalQuery | Convert.ToInt32(float) → (int)float cast |

## Completed Work
- 2026-05-25: PR #441 merged
- 2026-05-29: PR #445 merged
- 2026-06-04: PR #457 merged
- 2026-06-17: PR #462 merged
- 2026-06-17: PR #469 merged
- 2026-06-17: PR #479 merged
- 2026-06-24: PR #506 merged

## Open PRs (awaiting maintainer review)
- #aw_pr_cachefields: cache system field value types in AddDocument + precompute TimeSpan constants in IndexCommiter.ScheduleCommit

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~149 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
- Targets net6.0;net8.0 — Dictionary.TryAdd available on both
- Benchmark suite covers: concurrent search (1/25/100 threads), bulk indexing, concurrent searcher acquire
- Benchmark gaps: query construction (GroupedAnd/Or/Not), single-threaded search, field access
- Monthly issue #458 closed by @Shazwazza on 2026-06-18 (not_planned)
- Monthly issue #507 closed by @Shazwazza on 2026-06-24 (not_planned)
- Maintainer keeps closing monthly activity issues as "not_planned" — note this pattern
