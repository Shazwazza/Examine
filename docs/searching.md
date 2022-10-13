---
layout: page
title: Searching
permalink: /searching
ref: searching
order: 2
---
Searching
===

_**Tip**: There are many examples of searching in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Search/FluentApiTests.cs) to use as examples/reference._

## All fields (managed queries)

The simplest way of querying with examine is with the `Search` method:

```cs
var results = searcher.Search("hello world");
```

The above is just shorthand for doing this:

```cs
var query = searcher.CreateQuery().ManagedQuery("hello world");
var results = query.Execute(QueryOptions.Default);
```

A Managed query is a search operation that delegates to the underlying field types to determine how the field should
be searched. In most cases the field value type will be 'Full Text', others may be numeric fields, etc... So the query is built up based on the data passed in and what each field type is capable of searching.

## Per field

```csharp
var searcher = myIndex.Searcher; // Get a searcher
var results = searcher.CreateQuery() // Create a query
 .Field("Address", "Hills") // Look for any "Hills" addresses
 .Execute(); // Execute the search
```

## Range queries

### Float Range

```csharp
var searcher = myIndex.Searcher;
var query = searcher.CreateQuery();
var floatFilterQuery = query.RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, minInclusive: true, maxInclusive: true);
var results = floatFilterQuery.Execute(QueryOptions.Default);
```

This will return results where the field `SomeFloat` is within the range 0 - 100 (min value and max value included).

### Date Range

```csharp
var searcher = indexer.Searcher;

var createdQuery = searcher.CreateQuery()
  .RangeQuery<DateTime>(
      new[] { "created" },
      new DateTime(2000, 01, 02),
      new DateTime(2000, 01, 05),
      minInclusive: true,
      maxInclusive: false);

var results = createdQuery.Execute();
```

This will return results where the field `created` is within the date 2000/01/02 and 2000/01/05 (min value included and max value excluded).

## Booleans, Groups & Sub Groups

_TODO: Fill this in..._

## Lucene queries

Find a reference to how to write Lucene queries in the [Lucene 4.8.0 docs](https://lucene.apache.org/core/4_8_0/queryparser/org/apache/lucene/queryparser/classic/package-summary.html#package_description).

### Native Query

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();
var results = query.NativeQuery("hello:world").Execute();
```

### Combine a native query and Fluent API searching.

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();
var nativeQuery = query.NativeQuery("hello:world");
var fullquery = nativeQuery.And().Field("Address", "Hills"); // Combine queries
var results = fullquery.Execute();
```

### Combine a custom lucene query with raw lucene query

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();

var nativeQuery = (LuceneSearchQuery)query.NativeQuery("hello:world").And(); // Make query ready for extending
var fullquery = nativeQuery.LuceneQuery(NumericRangeQuery.NewInt64Range("numTest", 4, 5, true, true)); // Add the raw lucene query
var results = fullquery.Execute();
```

## Boosting, Proximity, Fuzzy & Escape

### Boosting

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery("content");
var NodeTypeAliasQuery = query.Field("nodeTypeAlias", "CWS_Home".Boost(20));
var results = NodeTypeAliasQuery.Execute();
```

This will boost the term `CWS_Home` and make enteries with `nodeTypeAlias:CWS_Home` score higher in the results.

### Proximity

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery("content");

//get all nodes that contain the words warren and creative within 5 words of each other
var ProximityQuery = query.Field("metaKeywords", "Warren creative".Proximity(5));
var results = ProximityQuery.Execute();
```

### Fuzzy

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();

var FuzzyQuery = query.Field("Content", "think".Fuzzy(0.1F));
var results = FuzzyQuery.Execute();
```

### Escape

Escapes the string within Lucene.

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery("content");

var EscapeQuery = query.Field("__Path", "-1,123,456,789".Escape());

var results = EscapeQuery.Execute();
```
