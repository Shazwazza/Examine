# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-08-22

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
- `LuceneSearchQueryBase.GroupedAnd/Or/Not` - LINQ state-machine allocs eliminated (PR #515, merged)
- `MultiSearchContext`: LINQ state-machine allocs eliminated (PR #515, merged)
- `LuceneQuery.GroupedAnd/Or/Not`: LINQ state-machine allocs eliminated (PR #518, MERGED)
- `GenerateHash`: removed redundant .ToLower() after "x2" format (PR #519, merged)
- `RemoveStopWords`: Action delegate/innerBuilder/string-concat allocs eliminated (PR #519, merged)
- `AddDocument`: field.Key.StartsWith uses StringComparison.Ordinal (PR #521, merged)
- `IsStandardAnalyzerStopWord`: ToLowerInvariant() replaces ToLower() (PR #521, merged)
- `MultiIndexSearcher.GetSearchContext()`: Lazy<LuceneSearcher[]> caches array (PR #529, MERGED 2026-07-29)
- `Field<T>` / `FieldNested<T>`: new single-field RangeQueryInternal<T> overload (PR #531, MERGED)
- `StringExtensions.EnsureEndsWith` + `ReplaceNonAlphanumericChars`: dead code removed (PR #534, MERGED)
- `SearchResult.GetValues`: dead Values fallback removed (PR #526, MERGED 2026-07-29)
- `ValueSet` constructors: LINQ ToDictionary → pre-sized foreach loops (PR #541, MERGED 2026-08-13)
- `SearchContext.SearchableFields`: LINQ Select+Where+ToArray → foreach loop (PR #545, MERGED 2026-08-13)
- `GetFieldNames`: materialize .ToArray() inside using block (PR #546, MERGED 2026-08-13)
- `ReadOnlyFieldDefinitionCollection`: GroupBy+FirstOrDefault → foreach+TryAdd (PR #546, MERGED 2026-08-13)
- `LuceneSearchQueryBase.SortFields`: lazy-init List eliminates alloc for unsorted queries (PR #536, MERGED 2026-08-11)
- `LuceneSearchQuery._categoryFilterQuery`: cached per instance (PR #537, MERGED 2026-08-11)
- `FieldQueryBenchmarks`: benchmark infrastructure added (PR #535, MERGED 2026-08-11)
- Tests run via NUnit, CI uses `dotnet test`. Test count 150 passed / 2 skipped (net8.0).
- Branch convention: `efficiency/<desc>` off `support/3.x`
- Note: Event args allocation skip (`IndexingItemEventArgs`, `DocumentWritingEventArgs`) is a potential optimization but risks breaking virtual method overrides — needs maintainer input
- Note: `GetDefaultValueTypes` uses `.ToDictionary()` (LINQ state machine), but this is init-time only — not worth optimizing
- Note: `BaseIndexProvider.IndexItems` validator path uses 2 LINQ state machines — only active when validator configured (not common case); fast-path already bypasses when no validator
- Full codebase rescan 2026-08-13: no new high-impact opportunities found; all major patterns comprehensively covered

## Optimisation Backlog
| Priority | Focus Area | Opportunity | Estimated Impact | Status |
|----------|------------|-------------|------------------|--------|
| LOW | Code-Level | `EmptySearchResults.GetEnumerator()` allocates boxed enumerator | 1 alloc per empty result call (rare path) | Not worth doing |
| LOW | Code-Level | Event args allocation skip (`IndexingItemEventArgs`, `DocumentWritingEventArgs`) | Small alloc elimination, but risks breaking virtual overrides | Needs maintainer input |

## Completed Work
- 2026-06-25: PR #509 merged — single-pass field collection in CreateSearchResult (O(N²)→O(N))
- 2026-06-25: PR #512 merged — cached system field types in AddDocument + precompute TimeSpan constants
- 2026-06-29: PR #515 merged — LINQ state-machine elimination in GroupedAnd/Or/Not + MultiSearchContext
- 2026-06-29: PR #519 merged — redundant .ToLower() in GenerateHash + lambda/StringBuilder allocs in RemoveStopWords
- 2026-06-29: PR #521 merged — StringComparison.Ordinal in AddDocument StartsWith + ToLowerInvariant in IsStandardAnalyzerStopWord
- 2026-06-29: PR #522 merged — cache default field-type factories in AddDocument loop
- 2026-06-30: PR #517 merged — reflection→pattern matching in CheckQueryForExtractTerms + LINQ inline loop in ManagedQueryInternal
- 2026-07-08: PR #518 MERGED — LuceneQuery GroupedAnd/Or/Not LINQ allocs
- 2026-07-08: PR #534 MERGED — dead string extension methods removed
- 2026-07-29: PR #531 MERGED — single-field RangeQueryInternal<T> overload (string[1] alloc eliminated)
- 2026-07-29: PR #526 MERGED — dead Values fallback in SearchResult.GetValues removed
- 2026-07-29: PR #529 MERGED — MultiIndexSearcher.GetSearchContext() LINQ allocs eliminated
- 2026-08-11: PR #535 MERGED — FieldQueryBenchmarks for typed Field<T> query hot path
- 2026-08-11: PR #536 MERGED — lazy-init SortFields in LuceneSearchQueryBase
- 2026-08-11: PR #537 MERGED — cache category filter query in LuceneSearchQuery
- 2026-08-13: PR #541 MERGED — eliminate LINQ state-machine allocs in ValueSet constructors
- 2026-08-13: PR #545 MERGED — replace LINQ chain with foreach loop in SearchContext.SearchableFields
- 2026-08-13: PR #546 MERGED — materialize GetFieldNames inside using block; replace GroupBy in ReadOnlyFieldDefinitionCollection

## Monthly Issues
- June 2026: #510 (closed)
- July 2026: #530 (closed 2026-08-01)
- August 2026: #544 (open)

## Backlog Cursor
- All open PRs merged as of 2026-08-13; no remaining high/medium priority opportunities
- All major hot-path code-level patterns addressed
- Next: monitor for new code additions; consider measurement infra improvements
- PR #569 merged 2026-08-14/18 (OrderedDictionary.Values LINQ elimination) — not ours (author: repo committer)
- PR #574 (ours, GetFieldNames LINQ elim) was blocked/conflicted against support/3.x after #569 merged; rebased via cherry-pick 2026-08-21, build+tests pass (150 passed), pushed via push_to_pull_request_branch

## Last Run Tasks
- 2026-08-13 20:21 UTC: Task 4 (all 6 PRs confirmed merged); Task 2 (full scan, no new opportunities); Task 7 (update monthly issue #544)
- 2026-08-21 19:58 UTC: Task 4 — rebased/fixed conflicted PR #574 onto latest support/3.x (cherry-pick, build+test verified); Task 2 — quick rescan, no new high-impact opportunities beyond what's already in #574; Task 7 — update monthly issue #544
- 2026-08-22 19:56 UTC: Task 4 — PR #574 had drifted into conflict again (base moved); rebased cleanly onto origin/support/3.x (no conflicts this time), build succeeded (0 errors), tests 150 passed/0 failed/2 skipped, pushed via push_to_pull_request_branch; Task 2 — rescanned for LINQ Select+ToArray/ToList patterns in Examine.Lucene/Examine.Core, none remaining; Task 7 — updated monthly issue #544
