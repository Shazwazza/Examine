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

The simplest way of querying with Examine is with the `Search` method:

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

### Terms and Phrases

When searching on fields like in the example above you might want to search on more than one word/term. In Examine this can be done by simply adding more terms to the field filter.

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses that has "Hills" or "Rockyroad" or "Hollywood"
 .Field("Address", "Hills Rockyroad Hollywood")
 .Execute();
```

The way that terms are split depends on the Analyzer being used. The StandardAnalyzer is the default. An example of how Analyzers work are:

- StandardAnalyzer - will split a string based on whitespace and 'stop words' (i.e. common words that are not normally searched on like "and")
- WhitespaceAnalyzer - will split a string based only on whitespace
- KeywordAnalyzer - will not split a string and will treat the single string as one term - this means that searching will be done on an exact match

There are many [Analyzers](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Analysis.html) and you can even create your own. See more about analyzers in [configuration](./configuration.md#example---phone-number).

Looking at this example when using the default StandardAnalyser the code above means that the `Address` in this example has to match any the values set in the statement. This is because Examine will create a Lucene query like this one where every word is matched separately: `Address:hills Address:rockyroad Address:hollywood`.

Instead, if you want to search for entries with the values above in that exact order you specified you will need to use the `.Escape()` method. See under [Escape](#escape).

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
  // Look for any addresses with the exact phrase "Hills Rockyroad Hollywood"
 .Field("Address", "Hills Rockyroad Hollywood".Escape())
 .Execute();
```

This creates a query like this instead: `Address:"Hills Rockyroad Hollywood"`. This means that you're now searching for the exact phrase instead of entries where terms appear.

## Range queries

Range Queries allow one to match documents whose field(s) values are between the lower and upper bound specified by the Range Query

### Float Range

Example:

```csharp
var searcher = myIndex.Searcher;
var query = searcher.CreateQuery();
query.RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, minInclusive: true, maxInclusive: true);
var results = query.Execute(QueryOptions.Default);
```

This will return results where the field `SomeFloat` is within the range 0 - 100 (min value and max value included).

### Date Range

Example:

```csharp
var searcher = indexer.Searcher;

var query = searcher.CreateQuery()
  .RangeQuery<DateTime>(
      new[] { "created" },
      new DateTime(2000, 01, 02),
      new DateTime(2000, 01, 05),
      minInclusive: true,
      maxInclusive: false);

var results = query.Execute();
```

This will return results where the field `created` is within the date 2000/01/02 and 2000/01/05 (min value included and max value excluded).

## Booleans, Groups & Sub Groups

_TODO: Fill this in..._

## Boosting

Boosting is the practice of making some parts of your query more relevant than others. This means that you can have terms that will make entries matching that term score higher in the search results.

Example:

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery("content");
query.Field("nodeTypeAlias", "CWS_Home".Boost(20));
var results = query.Execute();
```

This will boost the term `CWS_Home` and make enteries with `nodeTypeAlias:CWS_Home` score higher in the results.

## Proximity

Proximity searching helps in finding entries where words that are within a specific distance away from each other.

Example:

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery("content");

// Get all nodes that contain the words warren and creative within 5 words of each other
query.Field("metaKeywords", "Warren creative".Proximity(5));
var results = query.Execute();
```

## Fuzzy

Fuzzy searching is the practice of finding spellings that are similar to each other. Examine searches based on the [Damerau-Levenshtein Distance](https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance). The parameter given in the `.Fuzzy()` method is the edit distance allowed by default this value is `0.5` if not specified.

The value on `.Fuzzy()` can be between 0 and 2. Any number higher than 2 will be lowered to 2 when creating the query.

Example:

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();

query.Field("Content", "think".Fuzzy(0.1F));
var results = query.Execute();
```

## Escape

Escapes the string within Lucene.

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery("content");

query.Field("__Path", "-1,123,456,789".Escape());
var results = query.Execute();
```

## Wildcards

Examine supports single and multiple-character wildcards on single terms. (Cannot be used with the `.Escape()` method)

### Examine query (Single character)

The `.SingleCharacterWildcard()` method will add a single character wildcard to the end of the term or terms being searched on a field. 

```csharp
var query = searcher.CreateQuery()
    .Field("type", "test".SingleCharacterWildcard());
```

This will match for example: `test` and `tests`

### Examine query (Multiple characters)

The `.MultipleCharacterWildcard()` method will add a multiple characters wildcard to the end of the term or terms being searched on a field.

```csharp
var query = searcher.CreateQuery()
    .Field("type", "test".MultipleCharacterWildcard());
```

This will match for example: `test`, `tests` , `tester`, `testers`

### Lucene native query (Multiple characters)

The multiple wildcard character is `*`. It will match 0 or more characters.

Example

```csharp
var query = searcher.CreateQuery()
    .NativeQuery("equipment:t*pad");
```

This will match for example: `Trackpad` and `Teleportationpad`

Example

```csharp
var query = searcher.CreateQuery()
    .NativeQuery("role:test*");
```

This will match for example: `test`, `tests` and `tester`

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
query.NativeQuery("hello:world");
query.And().Field("Address", "Hills"); // Combine queries
var results = query.Execute();
```

### Combine a custom lucene query with raw lucene query

```csharp
var searcher = indexer.Searcher;
var query = searcher.CreateQuery();

var query = (LuceneSearchQuery)query.NativeQuery("hello:world").And(); // Make query ready for extending
query.LuceneQuery(NumericRangeQuery.NewInt64Range("numTest", 4, 5, true, true)); // Add the raw lucene query
var results = query.Execute();
```

### String Facets

String facets allows for counting the documents that share the same string value. This type of faceting is possible on all faceted index type.

Basic example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
 .Field("Address", "Hills")
 .WithFacets(facets => facets.FacetString("Address")) // Get facets of the Address field
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
    .WithFacets(facets => facets.FacetString("Address", "Hills")) // Get facets of the Address field with specific value
    .Execute();

var addressFacetResults = results.GetFacet("Address"); // Returns the facets for the specific field Address

/* 
* Example value
* Label: Hills, Value: 2 <-- As Hills was the only filtered value we will only get this facet
*/

var hillsValue = addressFacetResults.FacetString("Hills"); // Gets the IFacetValue for the facet Hills
```

MaxCount example
```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .Field("Address", "Hills")
    .WithFacets(facets => facets.FacetString("Address")) // Get facets of the Address field
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
    .WithFacets(facets => facets.FacetString("Address", config => config.MaxCount(2))) // Get facets of the Address field & Gets the top 2 results (The facets with the highest value)
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
    .WithFacets(facets => facets.FacetString("Address")) // Get facets of the Address field from the facet field address_facet (The facet field is automatically read from the FacetsConfig)
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
    .All()
    .WithFacets(facets => facets.FacetDoubleRange("Price", new DoubleRange[] {
        new DoubleRange("0-10", 0, true, 10, true),
        new DoubleRange("11-20", 11, true, 20, true)
    })) // Get facets of the price field
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
    .All()
    .WithFacets(facets => facets.FacetFloatRange("Price", new FloatRange[] {
        new FloatRange("0-10", 0, true, 10, true),
        new FloatRange("11-20", 11, true, 20, true)
    })) // Get facets of the price field
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
    .All()
    .WithFacets(facets => facets.FacetLongRange("Price", new Int64Range[] {
        new Int64Range("0-10", 0, true, 10, true),
        new Int64Range("11-20", 11, true, 20, true)
    })) // Get facets of the price field
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
    .All()
    .WithFacets(facets => facets.FacetLongRange("Created", new Int64Range[] {
        new Int64Range("first", DateTime.UtcNow.AddDays(-1).Ticks, true, DateTime.UtcNow.Ticks, true),
        new Int64Range("last", DateTime.UtcNow.AddDays(1).Ticks, true, DateTime.UtcNow.AddDays(2).Ticks, true)
    })) // Get facets of the price field
    .Execute();

var createdFacetResults = results.GetFacet("Created"); // Returns the facets for the specific field Created

/* 
* Example value
* Label: first, Value: 2
* Label: last, Value: 10
*/

var firstRangeValue = createdFacetResults.Facet("first"); // Gets the IFacetValue for the facet "first"
```
