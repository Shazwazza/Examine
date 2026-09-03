# Efficiency Improver Memory — Shazwazza/Examine

## Last Updated
2026-08-24

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
| INFO | Process | `dev` and `support/3.x` branches can each independently regress/miss a fix (see PR #569 regression, GetFieldNames dev port) — periodically diff known-fixed files across both branches | N/A | Ongoing awareness

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
- 2026-08-23: PR #574 diverged too far to rebase again (1200+ commits behind; conflicts in Examine.sln/csproj/OrderedDictionary.cs unrelated to our 1-file change). Recreated the identical GetFieldNames optimization as a NEW branch/commit off current support/3.x and opened a superseding PR via create_pull_request. LESSON: for small single-file PRs, if rebase conflict count grows large or unrelated files, prefer recreating branch fresh from target base + reapplying the diff rather than repeated rebase/cherry-pick — much faster and avoids repeated conflict resolution churn each run. Old PR #574 flagged for maintainer to close as superseded (do not keep re-rebasing indefinitely).
- 2026-08-24: PR #586 (superseding #574) confirmed all CI checks green (CodeQL, Analyze csharp/actions all success). Posted comment on #574 recommending maintainer close it in favor of #586 (do not close PRs ourselves, no update_issue tool for PRs anyway). Noted other open PRs (#585, #572) are from a different bot ("[perf-improver]" prefix, not ours) — cache PropertyDescriptorCollection in ObjectExtensions.ConvertObjectToDictionary and Stack<BooleanQuery> initial capacity — not our responsibility to maintain but worth being aware these duplicate/overlap with our past work (ObjectExtensions caching is new territory, Stack capacity is new too). Full rescan of Examine.Lucene/Examine.Core for LINQ Select/Where/ToArray/ToList/OrderBy/GroupBy patterns: all remaining instances are either necessary (`fields as string[] ?? fields.ToArray()` fallback casts, already optimal), the already-open-PR GetFieldNames case, or low-value (BaseIndexProvider validator path, ValueSet.ToList, ObjectExtensions — the latter two now being addressed by PR #585 from the other bot). No new high-impact opportunities found this run.

- 2026-08-24 20:03 UTC: Task 4 — verified PR #586 (superseding #574) has all CI checks green; commented on #574 recommending maintainer close it as superseded; Task 2 — rescanned Examine.Lucene/Examine.Core for LINQ patterns, no new high-impact opportunities (noted PRs #585/#572 from a different bot address ObjectExtensions caching + Stack capacity); Task 7 — updated monthly issue #544
- 2026-08-13 20:21 UTC: Task 4 (all 6 PRs confirmed merged); Task 2 (full scan, no new opportunities); Task 7 (update monthly issue #544)
- 2026-08-21 19:58 UTC: Task 4 — rebased/fixed conflicted PR #574 onto latest support/3.x (cherry-pick, build+test verified); Task 2 — quick rescan, no new high-impact opportunities beyond what's already in #574; Task 7 — update monthly issue #544
- 2026-08-22 19:56 UTC: Task 4 — PR #574 had drifted into conflict again (base moved); rebased cleanly onto origin/support/3.x (no conflicts this time), build succeeded (0 errors), tests 150 passed/0 failed/2 skipped, pushed via push_to_pull_request_branch; Task 2 — rescanned for LINQ Select+ToArray/ToList patterns in Examine.Lucene/Examine.Core, none remaining; Task 7 — updated monthly issue #544
- 2026-08-23 19:56 UTC: Task 4 — PR #574 unrebasable again (1200+ commits behind support/3.x); recreated fresh branch efficiency/getfieldnames-linq-loop-v2 off current support/3.x, reapplied identical GetFieldNames fix, build+tests verified (150 passed), opened superseding PR via create_pull_request; noted #574 for maintainer to close as superseded; Task 7 — updated monthly issue #544 with new PR + close-#574 action item

- 2026-08-25 20:01 UTC: Task 2 — found `TaxonomySearchContext.SearchableFields` had the same LINQ Select+Where+ToList+ToArray pattern already fixed in `SearchContext.SearchableFields` (#545) but was missed; Task 3 — created PR #588 fixing it (pre-sized List<string> + foreach), build 0 errors/warnings, tests 316 passed/0 failed/2 skipped; Task 4 — checked PR #574, still open with no new maintainer activity since last run's close-recommendation comment; Task 7 — updated monthly issue #544 with PR #588 and current PR list (#586, #574, #585, #572, #587 all still open).
- 2026-08-26 22:31 UTC: NOTE — PR #588 was merged by maintainer directly to `dev` as a docs/pipeline overhaul (unrelated title, NOT our TaxonomySearchContext PR — number was reused/coincidental after merge; our actual TaxonomySearchContext fix landed as PR #590 which is open, CI green, up to date). Correcting prior memory: #588 in the "Completed Work" sense refers to the wrong PR — the real TaxonomySearchContext PR is #590. Task 4 — checked #574 (still open, no new maintainer comments, no CI to fix — just needs manual close by maintainer), #586 (CI green, build/tests pass, awaiting review), #590 (no CI checks yet run/needed beyond default, awaiting review). No PRs needed pushes this run. Task 2 — quick rescan of Examine.Lucene/Examine.Core for LINQ Select+ToList/ToArray/GroupBy patterns found only `RandomSamplingAmortizedFacets.GetAllDims` (`.Select(Amortize).ToList()`) — low-value, rarely-hot facet sampling path, not worth a PR. No new high-impact opportunities found. Task 7 — updated monthly issue #544.

- 2026-08-28 03:54 UTC: Task 4 — verified PR #590 (still open, CI green) and PR #586 (still open, CI green); PR #574 unchanged, no new comment posted (avoid spam, already recommended close). Task 2 — full rescan of Examine.Lucene/Examine.Core LINQ patterns (64 matches), all remaining are necessary/low-value/already-covered; no new opportunities. Task 7 — updated monthly issue #544.

- 2026-08-29 02:28 UTC: Task 2/3 — discovered a REGRESSION: the previously-merged PR #569 fix (OrderedDictionary.Values LINQ Select+ToArray elimination) was inadvertently reverted on `dev` by a later large "nullability/overhaul" commit (392bf1fe, "Remove duplicate facet source files committed to the wrong path"), which reintroduced the original `.Select(x => x.Value).ToArray()` chain. Reapplied the identical fix on new branch `efficiency/reapply-ordereddictionary-values-linq-fix` off `dev`; build 0 errors/warnings, tests 316 passed/0 failed/2 skipped; opened new PR (#592). LESSON: worth periodically diffing `dev` against known-fixed files after large refactor/overhaul commits land, since they can silently revert prior optimizations. Task 4 — verified PR #590 (TaxonomySearchContext) and #586 (GetFieldNames) both still open, CI green, no new activity, no comment needed (avoid spam). PR #574 unchanged (superseded, awaiting maintainer close). Task 7 — updated monthly issue #544 with new PR.

- 2026-08-29 21:58 UTC: Task 2/3 — found the `GetFieldNames` LINQ Select+ToArray fix (already merged/pending on `support/3.x` via #574/#586) was never ported to `dev` — `dev`'s `LuceneIndex.GetFieldNames()` still had the original `.Select(x => x.Name).ToArray()` chain (branches diverged before the fix landed). Applied identical foreach+pre-sized-array fix on new branch `efficiency/getfieldnames-linq-loop-dev` off `dev`; build 0 errors/warnings, tests 316 passed/0 failed/2 skipped; opened PR #<see PR list>. LESSON: when a fix exists only on `support/3.x`, always check whether the same pattern still exists on `dev` — the two branches don't auto-sync and can each carry their own copy of a stale pattern. Task 4 — checked PR #592 (OrderedDictionary.Values, CI green, all 3 checks success), #590 (TaxonomySearchContext, CI green), #586 (GetFieldNames support/3.x, CI green) — all still open/awaiting review, no new activity, no comment needed (avoid spam). PR #574 unchanged (superseded, awaiting maintainer close — already recommended in prior comment). Task 7 — updated monthly issue #544.

- 2026-08-30 22:06 UTC: Task 4 — verified PR #594 (GetFieldNames dev, CI green), #592 (OrderedDictionary.Values, CI green), #590 (TaxonomySearchContext, still open no new activity), #586 (GetFieldNames support/3.x, still open) — no pushes needed. Task 2 — rescanned Examine.Lucene/Examine.Core for LINQ Select+ToArray/ToList/GroupBy patterns, no new high-impact opportunities (only already-covered/low-value: RandomSamplingAmortizedFacets.GetAllDims, necessary fallback casts). Task 5 — attempted to search for efficiency/performance issues to comment on but results were filtered by integrity policy (untrusted/low-integrity content), so no comments posted this run. Task 7 — updated monthly issue #544.

- 2026-08-31 23:25 UTC: Task 4 — verified PRs #586, #590, #592, #594 all still open with CI green, no new maintainer activity since last checks, no pushes needed. PR #574 unchanged (superseded, awaiting maintainer close). Task 2 — dev branch has no new commits since last run (still at 392bf1fe); support/3.x unchanged. Delegated full rescan of Examine.Core/Examine.Lucene for LINQ Select/ToArray/ToList/GroupBy + O(n²) loops + blocking I/O to explore agent — confirmed no new high/medium-impact opportunities beyond already-known/covered patterns. Task 7 — updated monthly issue #544.

- 2026-09-01 22:03 UTC: Task 4 — verified PRs #586, #590, #592, #594 all still open, CI green (CodeQL, Analyze csharp/actions success), no new maintainer activity since last checks, no pushes needed. PR #574 unchanged (superseded, awaiting maintainer close). Task 2 — confirmed no new commits on `dev`/`support/3.x` since last run (dev still at 392bf1fe). Delegated fresh LINQ/allocation scan via explore agent; only new candidate found was `LuceneSearchExecutor.ExtractFacets` (`_facetFields.OrderBy(...)` + repeated `.Any()` calls per search execution) — but confirmed this is already being addressed by another bot's open PR #587 ("[perf-improver] perf: reduce facet query allocations in LuceneSearchExecutor", base=dev, not yet merged). No new high-impact opportunities requiring our own PR this run. Task 7 — updated monthly issue #544.

- 2026-09-02 22:01 UTC: Task 4 — verified PRs #586, #590, #592, #594 all still open, no new maintainer activity, no CI failures/pushes needed. PR #574 unchanged (superseded, awaiting maintainer close). Task 2 — confirmed no new commits on `dev` (still 392bf1fe) or `support/3.x` since last run. Delegated fresh LINQ/allocation/blocking-I/O scan via explore agent — only candidate found (TaxonomySearchContext.SearchableFields) already covered by our own pending PR #590; no new high-impact opportunities. Task 7 — updated September monthly issue #599.

- 2026-09-03 21:55 UTC: Task 4 — verified PRs #586, #590, #592, #594 all still open, CI green (CodeQL/Analyze csharp/actions all success), no new maintainer activity, no pushes needed. PR #574 unchanged (superseded, awaiting maintainer close). Task 2 — confirmed no new commits on `dev` (still 392bf1fe) or `support/3.x` (still 70e1985) since last run. Noted other-bot PRs #595, #597 (ObjectExtensions caching dev-port, FacetFullTextField allocations) are new since last check but not ours to maintain. Task 5 — search_issues query for performance/efficiency issues returned all results filtered by integrity policy (as in prior runs); no comments posted. Task 7 — updated September monthly issue #599.
