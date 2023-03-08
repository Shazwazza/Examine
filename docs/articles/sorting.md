---
layout: page
title: Sorting
permalink: /sorting
uid: sorting
order: 3
---

Sorting
===

_**Tip**: There are many examples of sorting in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Search/FluentApiTests.cs) to use as examples/reference._

## Score

By default search results are ordered by Score descending so there's nothing specific that needs to be done to support this. If you use a different sorting operation then the [`Score`](xref:Examine.ISearchResult#Examine_ISearchResult_Score) value will be 0 for all results.

## Custom sorting

Any field that is a [numerical or date based](https://shazwazza.github.io/Examine/configuration.html#default-value-types) is automatically sortable. To make text based fields sortable you need to explicitly opt-in for that behavior. By default all fields are [`FieldDefinitionTypes.FullText`](https://shazwazza.github.io/Examine/configuration.html#default-value-types) which are not sortable. To make a text field sortable it needs to be [`FieldDefinitionTypes.FullTextSortable`](xref:Examine.FieldDefinitionTypes#Examine_FieldDefinitionTypes_FullTextSortable).

_You cannot sort on both the score and a custom field._

Sorting is done by either the [`OrderBy`](xref:Examine.Search.IOrdering#Examine_Search_IOrdering_OrderBy_Examine_Search_SortableField___) or [`OrderByDescending`](xref:Examine.Search.IOrdering#Examine_Search_IOrdering_OrderByDescending_Examine_Search_SortableField___) methods using a [`SortableField`](xref:Examine.Search.SortableField) and a [`SortType`](xref:Examine.Search.SortType). The [`SortType`](xref:Examine.Search.SortType) should typically match the field definition type (i.e. Int, Long, Double, etc...)

* For [`FieldDefinitionTypes.FullTextSortable`](xref:Examine.FieldDefinitionTypes#Examine_FieldDefinitionTypes_FullTextSortable) use [`SortType.String`](xref:Examine.Search.SortType)
* For [`FieldDefinitionTypes.DateTime`](xref:Examine.FieldDefinitionTypes#Examine_FieldDefinitionTypes_DateTime) use [`SortType.Long`](xref:Examine.Search.SortType).

Example:

```cs
 var searcher = indexer.GetSearcher();

 var orderedResults = searcher
   .CreateQuery("content")
   .Field("writerName", "administrator")
   .OrderBy(new SortableField("name", SortType.String))
   .Execute();
   
var orderedDescendingResults = searcher
   .CreateQuery("content")
   .Field("writerName", "administrator")
   .OrderByDescending(new SortableField("name", SortType.String))
   .Execute();
```

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
