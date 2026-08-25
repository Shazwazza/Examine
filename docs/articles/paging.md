---
title: Paging
permalink: /paging
uid: paging
order: 4
---

Paging
===

_**Tip**: There are many examples of paging in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/dev/src/Examine.Test/Examine.Lucene/Search/FluentApiTests.cs) to use as examples/reference._

## Paging and Limiting results

To limit results we can use the [`QueryOptions`](xref:Examine.Search.QueryOptions) class when executing a search query. The [`QueryOptions`](xref:Examine.Search.QueryOptions) class provides the ability to skip and take.

Examples:

```csharp
 var searcher = indexer.Searcher;

 var takeFirstTenInIndex = searcher
   .CreateQuery()
   .All()
   .Execute(QueryOptions.SkipTake(0, 10));

 var skipFiveAndTakeFirstTenInIndex = searcher
   .CreateQuery()
   .All()
   .Execute(QueryOptions.SkipTake(5, 10));

 var takeThreeResults = searcher
   .CreateQuery("content")
   .Field("writerName", "administrator")
   .OrderBy(new SortableField("name", SortType.String))
   .Execute(QueryOptions.SkipTake(0, 3));
   
var takeSevenHundredResults = searcher
   .CreateQuery("content")
   .Field("writerName", "administrator")
   .OrderByDescending(new SortableField("name", SortType.String))
   .Execute(QueryOptions.SkipTake(0, 700));
```

By default when using [`Execute()`](xref:Examine.Search.IQueryExecutor#Examine_Search_IQueryExecutor_Execute_Examine_Search_QueryOptions_) or `Execute(QueryOptions.SkipTake(0))` where no take parameter is provided the take of the search will be set to [`QueryOptions.DefaultMaxResults`](xref:Examine.Search.QueryOptions#Examine_Search_QueryOptions_DefaultMaxResults) (100).

## Deep Paging

Skip/take paging gets progressively more expensive the deeper you page, because Lucene has to collect and rank every document up to `skip + take` before discarding the ones you skipped. For large result sets, use "search after" paging instead: each page is retrieved by telling Lucene which document the previous page ended on.

This is a Lucene.NET specific feature, exposed through [`LuceneQueryOptions`](xref:Examine.Lucene.Search.LuceneQueryOptions), [`SearchAfterOptions`](xref:Examine.Lucene.Search.SearchAfterOptions) and [`ILuceneSearchResults`](xref:Examine.Lucene.Search.ILuceneSearchResults).

1. Build the query as normal.
2. Execute it with a [`LuceneQueryOptions`](xref:Examine.Lucene.Search.LuceneQueryOptions), and either call [`ExecuteWithLucene`](xref:Examine.Lucene.Search.LuceneSearchExtensions#Examine_Lucene_Search_LuceneSearchExtensions_ExecuteWithLucene_Examine_Search_IQueryExecutor_Examine_Search_QueryOptions_) or cast the [`ISearchResults`](xref:Examine.ISearchResults) to [`ILuceneSearchResults`](xref:Examine.Lucene.Search.ILuceneSearchResults).
3. Keep [`ILuceneSearchResults.SearchAfter`](xref:Examine.Lucene.Search.ILuceneSearchResults#Examine_Lucene_Search_ILuceneSearchResults_SearchAfter) from that result.
4. Rebuild the same query for the next page and pass the stored [`SearchAfterOptions`](xref:Examine.Lucene.Search.SearchAfterOptions) into a new [`LuceneQueryOptions`](xref:Examine.Lucene.Search.LuceneQueryOptions). `skip` is ignored when `searchAfter` is supplied - the next `take` documents after that document are returned.
5. Repeat for each subsequent page.

```csharp
var searcher = indexer.Searcher;

var query = searcher.CreateQuery("content")
    .Field("writerName", "administrator")
    .OrderByDescending(new SortableField("id", SortType.Int));

// First page: skip 0, take 10
var page1 = query.ExecuteWithLucene(new LuceneQueryOptions(0, 10));

foreach (var result in page1)
{
    // ...
}

// Second page: continue after the last document of page 1.
// Skip is ignored when SearchAfterOptions is supplied.
var page2 = query.ExecuteWithLucene(new LuceneQueryOptions(0, 10, page1.SearchAfter));
```

`SearchAfter` is `null` when there is nothing more to page through, so it doubles as the termination condition:

```csharp
var query = searcher.CreateQuery("content").Field("writerName", "administrator");

SearchAfterOptions? searchAfter = null;

do
{
    var page = query.ExecuteWithLucene(new LuceneQueryOptions(0, 100, searchAfter));

    foreach (var result in page)
    {
        // ...
    }

    searchAfter = page.SearchAfter;
}
while (searchAfter is not null);
```

Deep paging works with [faceted queries](xref:searching#faceting). Facet counts are calculated over the full matching set rather than the current page, so they do not change as you page.

### Skip/take limits

When you are not using `SearchAfter`, [`LuceneQueryOptions.SkipTakeMaxResults`](xref:Examine.Lucene.Search.LuceneQueryOptions#Examine_Lucene_Search_LuceneQueryOptions_SkipTakeMaxResults) caps the size of the data set that can be paged. It defaults to [`QueryOptions.AbsoluteMaxResults`](xref:Examine.Search.QueryOptions#Examine_Search_QueryOptions_AbsoluteMaxResults). Raising it allows deeper skip/take paging at a performance cost - prefer `SearchAfter` for that case.

[`AutoCalculateSkipTakeMaxResults`](xref:Examine.Lucene.Search.LuceneQueryOptions#Examine_Lucene_Search_LuceneQueryOptions_AutoCalculateSkipTakeMaxResults) instead pre-calculates the document count in the index and uses that as the cap. This costs an extra count query on every search execution.
```csharp
var results = query.ExecuteWithLucene(
    new LuceneQueryOptions(0, 50, skipTakeMaxResults: 50_000));
```

### Scoring options

Score tracking is off by default because it costs work that most queries do not need.

* [`TrackDocumentScores`](xref:Examine.Lucene.Search.LuceneQueryOptions#Examine_Lucene_Search_LuceneQueryOptions_TrackDocumentScores) - populate [`ISearchResult.Score`](xref:Examine.ISearchResult#Examine_ISearchResult_Score) on each result.
* [`TrackDocumentMaxScore`](xref:Examine.Lucene.Search.LuceneQueryOptions#Examine_Lucene_Search_LuceneQueryOptions_TrackDocumentMaxScore) - populate [`ILuceneSearchResults.MaxScore`](xref:Examine.Lucene.Search.ILuceneSearchResults#Examine_Lucene_Search_ILuceneSearchResults_MaxScore). Without it, `MaxScore` is [`float.NaN`](https://learn.microsoft.com/en-us/dotnet/api/system.single.nan).

```csharp
var results = query.ExecuteWithLucene(
    new LuceneQueryOptions(0, 10, trackDocumentScores: true, trackDocumentMaxScore: true));

var maxScore = results.MaxScore;
```

### Facet sampling

For very large result sets, [`LuceneFacetSamplingQueryOptions`](xref:Examine.Lucene.Search.LuceneFacetSamplingQueryOptions) trades exact facet counts for speed by sampling the matching documents rather than counting all of them.

```csharp
var results = query.ExecuteWithLucene(
    new LuceneQueryOptions(0, 10, facetSampling: new LuceneFacetSamplingQueryOptions(sampleSize: 1000, seed: 42)));
```
