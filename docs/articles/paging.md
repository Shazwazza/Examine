---
title: Paging
permalink: /paging
uid: paging
order: 4
---

Paging
===

_**Tip**: There are many examples of sorting in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Search/FluentApiTests.cs) to use as examples/reference._

## Paging and Limiting results

To limit results we can use the [`QueryOptions`](xref:Examine.Search.QueryOptions) class when executing a search query. The [`QueryOptions`](xref:Examine.Search.QueryOptions) class provides the ability to skip and take.

Examples:

```csharp
 var searcher = indexer.GetSearcher();

 var takeFirstTenInIndex = searcher
   .CreateQuery()
   .All()
   .Execute(QueryOptions.SkipTake(0, 10))

 var skipFiveAndTakeFirstTenInIndex = searcher
   .CreateQuery()
   .All()
   .Execute(QueryOptions.SkipTake(5, 10))

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

By default when using [`Execute()`](xref:Examine.Search.IQueryExecutor#Examine_Search_IQueryExecutor_Execute_Examine_Search_QueryOptions_) or `Execute(QueryOptions.SkipTake(0))` where no take parameter is provided the take of the search will be set to [`QueryOptions.DefaultMaxResults`](xref:Examine.Search.QueryOptions#Examine_Search_QueryOptions_DefaultMaxResults) (500).

## Deep Paging

When using Lucene.NET as the Examine provider it is possible to more efficiently perform deep paging.
Steps:

1. Build and execute your query as normal.
2. Cast the ISearchResults from IQueryExecutor.Execute to ILuceneSearchResults
3. Store ILuceneSearchResults.SearchAfter (SearchAfterOptions) for the next page.
4. Create the same query as the previous request.
5. When calling IQueryExecutor.Execute. Pass in new LuceneQueryOptions(skip,take, SearchAfterOptions); Skip will be ignored, the next take documents will be retrieved after the SearchAfterOptions document.
6. Repeat Steps 2-5 for each page.