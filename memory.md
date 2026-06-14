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

## Last Run Tasks (2026-06-14)
- Task 4: Verified open PRs #462, #469, #479 — no CI failures, no merge conflicts
- Task 2: Quick opportunity scan — backlog updated
- Task 7: Updated monthly activity issue #458

## Optimization Backlog
| Priority | Area | Opportunity |
|----------|------|-------------|
| DONE | LuceneSearchExecutor | Redundant LastOrDefault() + new object[0] in search-after path (PR #441, merged) |
| DONE | LuceneSearchExecutor | CheckQueryForExtractTerms reflection + Fields?.ToArray() copy (PR #445, merged) |
| DONE | LuceneSearchExecutor | Minor array allocs (PR #457, merged 2026-06-04) |
| OPEN PR | GroupedAnd/Or/Not + CreateSearchResult | PR #462 — reduce allocations (not draft, clean) |
| OPEN PR | ValueSet constructor | PR #469 — eliminate intermediate dict + generator allocs (not draft, clean) |
| OPEN PR | BaseIndexProvider.IndexItems | PR #479 — fast-path when no validator (draft, unstable) |
| LOW | CheckQueryForExtractTerms | Early-out for BooleanQuery with empty clauses |
| LOW | SearchContext | SearchableFields two LINQ passes on first load (cached, very minor) |

## Completed Work
- 2026-05-25: PR #441 merged
- 2026-05-29: PR #445 merged
- 2026-06-04: PR #457 merged

## Open PRs (awaiting maintainer review)
- #462: reduce allocations in GroupedAnd/Or/Not and search result field loading
- #469: eliminate intermediate dictionary and generator allocations in ValueSet constructor
- #479: fast-path IndexItems when no validator configured (draft)

## Notes
- No AGENTS.md in this repo
- TreatWarningsAsErrors is on — zero warnings required
- Nullable reference types enabled
- Tests: ~147 tests (net8.0 filter), takes ~2.5 min
- Default branch: support/3.x
- 3 open PRs pending review — avoid creating more until some are merged
