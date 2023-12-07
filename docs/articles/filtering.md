---
title: Filtering
permalink: /filtering
uid: filtering
order: 3
---
Filtering
===

_**Tip**: There are many examples of filtering in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/release/3.0/src/Examine.Test/Examine.Lucene/Search/FluentApiTests.cs) to use as examples/reference._

## Common

Obtain an instance of [`ISearcher`](xref:Examine.ISearcher) for the index to be searched from [`IExamineManager`](xref:Examine.IExamineManager).

### Terms and Phrases

When filtering on fields like in the example above you might want to search on more than one word/term. In Examine this can be done by simply adding more terms to the term filter.

### Term Filter

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that has "Hills" or "Rockyroad" or "Hollywood"
   .WithFilter(
     filter =>
     {
         filter.TermFilter(new FilterTerm("Address", "Hills Rockyroad Hollywood"));
     })
     .All()
 .Execute();
```

### Terms Filter

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that has "Hills" or "Rockyroad" or "Hollywood"
   .WithFilter(
     filter =>
     {
         filter.TermsFilter(new[] {new FilterTerm("Address", "Hills"), new FilterTerm("Address", "Rockyroad"), new FilterTerm("Address", "Hollywood") });
     })
     .All()
 .Execute();
```

### Term Prefix Filter

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that starts with "Hills"
   .WithFilter(
     filter =>
     {
         filter.TermPrefixFilter(new FilterTerm("Address", "Hills"));
     })
     .All()
 .Execute();
```

## Range Filters

Range Filters allow one to match documents whose field(s) values are between the lower and upper bound specified by the Range Filter

### Int Range

Example:

```csharp
var searcher = myIndex.Searcher;
var query = searcher.CreateQuery();
 query.WithFilter(
     filter =>
     {
         filter.IntRangeFilter("SomeInt", 0, 100, minInclusive: true, maxInclusive: true);
     }).All();
var results = query.Execute(QueryOptions.Default);
```

This will return results where the field `SomeInt` is within the range 0 - 100 (min value and max value included).

### Long Range

Example:

```csharp
var searcher = myIndex.Searcher;
var query = searcher.CreateQuery();
 query.WithFilter(
     filter =>
     {
         filter.LongRangeFilter("SomeLong", 0, 100, minInclusive: true, maxInclusive: true);
     }).All();
var results = query.Execute(QueryOptions.Default);
```

This will return results where the field `SomeLong` is within the range 0 - 100 (min value and max value included).

### Float Range

Example:

```csharp
var searcher = myIndex.Searcher;
var query = searcher.CreateQuery();
 query.WithFilter(
     filter =>
     {
         filter.FloatRangeFilter("SomeFloat", 0f, 100f, minInclusive: true, maxInclusive: true);
     }).All();
var results = query.Execute(QueryOptions.Default);
```

This will return results where the field `SomeFloat` is within the range 0 - 100 (min value and max value included).

### Double Range

Example:

```csharp
var searcher = myIndex.Searcher;
var query = searcher.CreateQuery();
 query.WithFilter(
     filter =>
     {
         filter.FloatRangeFilter("SomeDouble", 0.0, 100.0, minInclusive: true, maxInclusive: true);
     }).All();
var results = query.Execute(QueryOptions.Default);
```

This will return results where the field `SomeDouble` is within the range 0 - 100 (min value and max value included).

## Booleans and Chains

### Or

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that start with "Hills" or "Valleys"
   .WithFilter(
     filter =>
     {
         filter.TermPrefixFilter(new FilterTerm("Address", "Hills"))
         .OrFilter()
         filter.TermPrefixFilter(new FilterTerm("Address", "Valleys"));
     })
     .All()
 .Execute();
```

### And

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that has "Hills" and keyword "Examine"
   .WithFilter(
     filter =>
     {
         filter.TermFilter(new FilterTerm("Address", "Hills"))
         .AndFilter()
         filter.TermFilter(new FilterTerm("Keyword", "Examine"));
     })
     .All()
 .Execute();
```

### Not

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that has "Hills" and keyword "Examine"
   .WithFilter(
     filter =>
     {
         filter.TermFilter(new FilterTerm("Address", "Hills"))
         .NotFilter()
         filter.TermFilter(new FilterTerm("Keyword", "Examine"));
     })
     .All()
 .Execute();
```

### And Not

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that has "Hills" and not keyword "Examine"
   .WithFilter(
     filter =>
     {
         filter.TermFilter(new FilterTerm("Address", "Hills"))
         .AndNotFilter(innerFilter => innerFilter.TermFilter(new FilterTerm("Keyword", "Examine")));
     })
     .All()
 .Execute();
```

### Chaining

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
   .WithFilter(
    filter =>
    {
        filter.ChainFilters(chain =>
            chain.Chain(ChainOperation.AND, chainedFilter => chainedFilter.NestedFieldValueExists("nodeTypeAlias")) //AND
                    .Chain(ChainOperation.AND, chainedFilter => chainedFilter.NestedTermPrefix(new FilterTerm("nodeTypeAlias", "CWS_H")))
                        .Chain(ChainOperation.OR, chainedFilter => chainedFilter.NestedTermFilter(new FilterTerm("nodeName", "my name")))
                            .Chain(ChainOperation.ANDNOT, chainedFilter => chainedFilter.NestedTermFilter(new FilterTerm("nodeName", "someone elses name")))
                                .Chain(ChainOperation.XOR, chainedFilter => chainedFilter.NestedTermPrefix(new FilterTerm("nodeName", "my")))
                    );
    })
     .All()
 .Execute();
```

## Spatial

Examine supports Spatial Filtering.
The Examine.Lucene.Spatial package needs to be installed.

### Spatial Operations

Below are the available Spatial Operations in Examine that are supported by the Examine.Lucene.Spatial package. Available operations may vary by provider.

- ExamineSpatialOperation.Intersects
- ExamineSpatialOperation.Overlaps
- ExamineSpatialOperation.IsWithin
- ExamineSpatialOperation.BoundingBoxIntersects
- ExamineSpatialOperation.BoundingBoxWithin
- ExamineSpatialOperation.Contains
- ExamineSpatialOperation.IsDisjointTo
- ExamineSpatialOperation.IsEqualTo

### Spatial Filtering

The `.SpatialOperationFilter()` method adds a filter to the query results to remove any results that do not pass the filter.
The example below demonstrates filtering results where the shape stored in the "spatialWKT" field must intersect the rectangle defined.

```csharp
var query = searcher.CreateQuery()
                .WithFilter(
                        filter => filter.SpatialOperationFilter("spatialWKT", ExamineSpatialOperation.Intersects, (shapeFactory) => shapeFactory.CreateRectangle(0.0, 1.0, 0.0, 1.0))
                );
```

## Custom lucene filter

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();

var query = (LuceneSearchQuery)query.NativeQuery("hello:world").And(); // Make query ready for extending
query.LuceneFilter(new TermFilter(new Term("nodeTypeAlias", "CWS_Home"))); // Add the raw lucene query
var results = query.Execute();
```