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

### String Facets

String facets allows for counting the documents that share the same string value. This type of faceting is possible on all faceted index type.

Basic example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Field("Address", "Hills")
 .And()
 .Facet("Address") // Get facets of the Address field
 .Execute();

var addressFacetResults = results.GetFacet("Address"); // Returns the facets for the specific field Address

/* 
* Example value
* Label: Hills, Value: 2
* Label: Hollywood, Value: 10
*/

var hillsValue = addressFacetResults.Facet("Hills"); // Gets the IFacetValue for the facet Hills
```

Filtered value example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Field("Address", "Hills")
 .And()
 .Facet("Address", "Hills") // Get facets of the Address field
 .Execute();

var addressFacetResults = results.GetFacet("Address"); // Returns the facets for the specific field Address

/* 
* Example value
* Label: Hills, Value: 2 <-- As Hills was the only filtered value we will only get this facet
*/

var hillsValue = addressFacetResults.Facet("Hills"); // Gets the IFacetValue for the facet Hills
```

MaxCount example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Field("Address", "Hills")
 .And()
 .Facet("Address") // Get facets of the Address field
 .Execute();

var addressFacetResults = results.GetFacet("Address"); // Returns the facets for the specific field Address

/* 
* Example value
* Label: Hills, Value: 2
* Label: Hollywood, Value: 10
* Label: London, Value: 12
*/

results = searcher.CreateQuery()
 .Field("Address", "Hills")
 .And()
 .Facet("Address") // Get facets of the Address field
    .MaxCount(2) // Gets the top 2 results (The facets with the highest value)
 .Execute();

addressFacetResults = results.GetFacet("Address"); // Returns the facets for the specific field Address

/* 
* Example value (Notice only 2 values are present)
* Label: Hollywood, Value: 10
* Label: London, Value: 12
*/
```

FacetField example
```csharp
// Setup

// Create a config
var facetsConfig = new FacetsConfig();

// Set the index field name to facet_address. This will store facets of this field under facet_address instead of the default $facets. This requires you to use FacetField in your Facet query. (Only works on string facets).
facetsConfig.SetIndexFieldName("Address", "facet_address");

services.AddExamineLuceneIndex("MyIndex",
    // Set the indexing of your fields to use the facet type
    fieldDefinitions: new FieldDefinitionCollection(
        new FieldDefinition("Address", FieldDefinitionTypes.FacetFullText)
        ),
    // Pass your config
    facetsConfig: facetsConfig
    );


var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Field("Address", "Hills")
 .And()
 .Facet("Address") // Get facets of the Address field
    .FacetField("address_facet")
 .Execute();

var addressFacetResults = results.GetFacet("Address"); // Returns the facets for the specific field Address

/* 
* Example value
* Label: Hills, Value: 2
* Label: Hollywood, Value: 10
*/
```

### Numeric Range facet

Numeric range facets can be used with numbers and get facets for numeric ranges. For example, it would group documents of the same price range.

There's two categories of numeric ranges - `DoubleRanges` and `Int64Range` for double/float values and int/long/datetime values respectively.

Double range example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Facet("Price", new DoubleRange[] {
  new DoubleRange("0-10", 0, true, 10, true),
  new DoubleRange("11-20", 11, true, 20, true)
 }) // Get facets of the price field
 .And()
 .All()
 .Execute();

var priceFacetResults = results.GetFacet("Price"); // Returns the facets for the specific field Price

/* 
* Example value
* Label: 0-10, Value: 2
* Label: 11-20, Value: 10
*/

var firstRangeValue = priceFacetResults.Facet("0-10"); // Gets the IFacetValue for the facet "0-10"
```

Float range example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Facet("Price", new DoubleRange[] {
  new DoubleRange("0-10", 0, true, 10, true),
  new DoubleRange("11-20", 11, true, 20, true)
 }) // Get facets of the price field
    .IsFloat(true) // Marks that the underlying field is a float
 .And()
 .All()
 .Execute();

var priceFacetResults = results.GetFacet("Price"); // Returns the facets for the specific field Price

/* 
* Example value
* Label: 0-10, Value: 2
* Label: 11-20, Value: 10
*/

var firstRangeValue = priceFacetResults.Facet("0-10"); // Gets the IFacetValue for the facet "0-10"
```

Int/Long range example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Facet("Price", new Int64Range[] {
  new Int64Range("0-10", 0, true, 10, true),
  new Int64Range("11-20", 11, true, 20, true)
 }) // Get facets of the price field
 .And()
 .All()
 .Execute();

var priceFacetResults = results.GetFacet("Price"); // Returns the facets for the specific field Price

/* 
* Example value
* Label: 0-10, Value: 2
* Label: 11-20, Value: 10
*/

var firstRangeValue = priceFacetResults.Facet("0-10"); // Gets the IFacetValue for the facet "0-10"
```

DateTime range example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Facet("Created", new Int64Range[] {
  new Int64Range("first", DateTime.UtcNow.AddDays(-1).Ticks, true, DateTime.UtcNow.Ticks, true),
  new Int64Range("last", DateTime.UtcNow.AddDays(1).Ticks, true, DateTime.UtcNow.AddDays(2).Ticks, true)
 }) // Get facets of the price field
 .And()
 .All()
 .Execute();

var createdFacetResults = results.GetFacet("Created"); // Returns the facets for the specific field Created

/* 
* Example value
* Label: first, Value: 2
* Label: last, Value: 10
*/

var firstRangeValue = createdFacetResults.Facet("first"); // Gets the IFacetValue for the facet "first"
```