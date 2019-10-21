---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults

layout: default
---

_[...Back to home](index)_

Searching
===

_**Tip**: There are many examples of searching in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Search/FluentApiTests.cs) to use as examples/reference._

## Managed queries

_TODO: Fill this in..._

## Per field

```csharp
_var searcher = myIndex.GetSearcher(); // Get a searcher
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
    .RangeQuery<DateTime>(new[] { "created" }, new DateTime(2000, 01,      02), new DateTime(2000, 01, 05), maxInclusive: false);
```

## Booleans, Groups & Sub Groups

_TODO: Fill this in..._

## Lucene queries

### Native Query

```csharp
var searcher = indexer.GetSearcher();  
var criteria = (LuceneSearchQuery)searcher.CreateQuery();  
//combine a custom lucene query with raw lucene query  
var op = criteria.NativeQuery("hello:world").And();
```

### Combine a custom lucene query with raw lucene query

```csharp
var criteria = (LuceneSearchQuery)searcher.CreateQuery();

//combine a custom lucene query with raw lucene query

var op = criteria.NativeQuery("hello:world").And();                                

criteria.LuceneQuery(NumericRangeQuery.NewLongRange("numTest", 4, 5, true, true));
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
