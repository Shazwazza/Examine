---
layout: page
title: Searching
permalink: /searching
ref: searching
order: 2
---
Searching
===

_**Tip**: There are many examples of searching in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/dev/src/Examine.Test/Examine.Lucene/Search/FluentApiTests.cs) to use as examples/reference._

## All fields (managed queries)

The simplest way of querying with examine is with the `Search` method:

```cs
var results = searcher.Search("hello world");
```

The above is just shorthand for doing this:

```cs
var query = CreateQuery().ManagedQuery("hello world");
var results = sc.Execute(options);
```

A Managed query is a search operation that delegates to the underlying field types to determine how the field should
be searched. In most cases the field value type will be 'Full Text', others may be numeric fields, etc... So the query is built up based on the data passed in and what each field type is capable of searching.

## Per field

```csharp
var searcher = myIndex.GetSearcher(); // Get a searcher
var results = searcher.CreateQuery() // Create a query
  .Field("Address", "Hills") // Look for any "Hills" addresses
  .Execute(); // Execute the search
```

## Range queries

### Float Range

```csharp
var searcher = indexer.GetSearcher();
var criteria1 = searcher.CreateQuery();  
var filter1 = criteria1.RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, true, true);
```

### Date Range

```csharp
var searcher = indexer.GetSearcher();

var numberSortedCriteria = searcher.CreateQuery()
  .RangeQuery<DateTime>(
      new[] { "created" }, 
      new DateTime(2000, 01, 02), 
      new DateTime(2000, 01, 05), 
      maxInclusive: false);
```

## Booleans, Groups & Sub Groups

_TODO: Fill this in..._

## Lucene queries

### Native Query

```csharp
var searcher = indexer.GetSearcher();

var results = searcher.CreateQuery().NativeQuery("hello:world").Execute();
```

### Combine a custom lucene query with raw lucene query

```csharp
var searcher = indexer.GetSearcher();
var query = (LuceneSearchQuery)searcher.CreateQuery();

// first create the Native Query and suffix with an And() call.
var op = criteria.NativeQuery("hello:world").And();                                

// use the original LuceneSearchQuery instance to append
// the next query value.
query.LuceneQuery(NumericRangeQuery.NewInt64Range("numTest", 4, 5, true, true));

var results = query.Execute();
```

## Boosting, Proximity, Fuzzy & Escape

### Boosting

```csharp
var searcher = indexer.GetSearcher();


var criteria = searcher.CreateQuery("content");

var filter = criteria.Field("nodeTypeAlias", "CWS_Home".Boost(20));
```

### Proximity

```csharp
var searcher = indexer.GetSearcher();

//Arrange

var criteria = searcher.CreateQuery("content");

//get all nodes that contain the words warren and creative within 5 words of each other
var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5));
```

### Fuzzy

```csharp
var searcher = indexer.GetSearcher();
var criteria = searcher.CreateQuery();

var filter = criteria.Field("Content", "think".Fuzzy(0.1F));
```

### Escape

```csharp
var exactcriteria = searcher.CreateQuery("content");

var exactfilter = exactcriteria.Field("__Path", "-1,123,456,789".Escape());

var results2 = exactfilter.Execute();
```

### Lucene Searches

Sometimes you need access to the underlying Lucene.NET APIs to perform a manual Lucene search.

An example of Spatial Search with the Lucene APIs is in the [Examine source code](https://github.com/Shazwazza/Examine/blob/dev/src/Examine.Test/Examine.Lucene/Extensions/SpatialSearch.cs).

```csharp
var searcher = (LuceneSearcher)indexer.Searcher;
var searchContext = searcher.GetSearchContext();

using (ISearcherReference searchRef = searchContext.GetSearcher())
{
  // Access the instance of the underlying Lucene
  // IndexSearcher instance.
  var indexSearcher = searchRef.IndexSearcher;

  // You can then call indexSearcher.Search(...)
  // which is a Lucene API, customize the parameters
  // accordingly and handle the Lucene response.
}
```