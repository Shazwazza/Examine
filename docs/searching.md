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

```aspnet
_var searcher = myIndex.GetSearcher(); // Get a searcher
 var results = searcher.CreateQuery() // Create a query
 .Field("Address", "Hills") // Look for any "Hills" addresses
 .Execute(); // Execute the search
```

## Range queries

```aspnet
var searcher = indexer.GetSearcher();
var criteria1 = searcher.CreateQuery();  
var filter1 = criteria1.RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, true, true);
```

## Booleans, Groups & Sub Groups

_TODO: Fill this in..._

## Lucene queries

### Native Query

```aspnet
var searcher = indexer.GetSearcher();  
var criteria = (LuceneSearchQuery)searcher.CreateQuery();  
//combine a custom lucene query with raw lucene query  
var op = criteria.NativeQuery("hello:world").And();
```

### Combine a custom lucene query with raw lucene query

```aspnet
var criteria = (LuceneSearchQuery)searcher.CreateQuery();

//combine a custom lucene query with raw lucene query

var op = criteria.NativeQuery("hello:world").And();                                

               criteria.LuceneQuery(NumericRangeQuery.NewLongRange("numTest", 4, 5, true, true));
```

## Boosting, Proximity, Fuzzy & Escape

_TODO: Fill this in..._
