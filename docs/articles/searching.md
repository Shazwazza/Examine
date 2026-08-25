---
title: Searching
permalink: /searching
uid: searching
order: 2
---
Searching
===

_**Tip**: There are many examples of searching in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/dev/src/Examine.Test/Examine.Lucene/Search/FluentApiTests.cs) to use as examples/reference._


## Common

Obtain an instance of [`ISearcher`](xref:Examine.ISearcher) for the index to be searched from [`IExamineManager`](xref:Examine.IExamineManager).

## All fields (managed queries)

The simplest way of querying with Examine is with the [`Search`](xref:Examine.ISearcher#Examine_ISearcher_Search_System_String_Examine_Search_QueryOptions_) method:

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

To create a query using the fluent builder query syntax start by calling [`ISearcher.CreateQuery()`](xref:Examine.ISearcher#Examine_ISearcher_Search_System_String_Examine_Search_QueryOptions_) then chain options as required.
Finally call [`Execute`](xref:Examine.Search.IQueryExecutor#Examine_Search_IQueryExecutor_Execute_Examine_Search_QueryOptions_) to execute the query.

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

The way that terms are split depends on the Analyzer being used. The [`StandardAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/analysis-common/Lucene.Net.Analysis.Standard.StandardAnalyzer.html) is the default. An example of how Analyzers work are:

- [`StandardAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/analysis-common/Lucene.Net.Analysis.Standard.StandardAnalyzer.html) - will split a string based on whitespace and 'stop words' (i.e. common words that are not normally searched on like "and")
- [`WhitespaceAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/analysis-common/Lucene.Net.Analysis.Core.WhitespaceAnalyzer.html) - will split a string based only on whitespace
- [`KeywordAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/analysis-common/Lucene.Net.Analysis.Core.KeywordAnalyzer.html) - will not split a string and will treat the single string as one term - this means that searching will be done on an exact match

There are many [Analyzers](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/core/Lucene.Net.Analysis.html) and you can even create your own. See more about analyzers in [configuration](./configuration.md#example---phone-number).

Looking at this example when using the default StandardAnalyser the code above means that the `Address` in this example has to match any the values set in the statement. This is because Examine will create a Lucene query like this one where every word is matched separately: `Address:hills Address:rockyroad Address:hollywood`.

Instead, if you want to search for entries with the values above in that exact order you specified you will need to use the [`.Escape()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Escape_System_String_) method. See under [Escape and phrase queries](#escape-and-phrase-queries).

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
var results = searcher.CreateQuery()
    .RangeQuery<float>(new[] { "SomeFloat" }, 0f, 100f, minInclusive: true, maxInclusive: true)
    .Execute(QueryOptions.Default);
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

By default a query created with [`CreateQuery()`](xref:Examine.ISearcher#Examine_ISearcher_CreateQuery_System_String_Examine_Search_BooleanOperation_) combines its clauses with `And`. Pass a [`BooleanOperation`](xref:Examine.Search.BooleanOperation) to change that default.

Every clause returns an [`IBooleanOperation`](xref:Examine.Search.IBooleanOperation), which exposes [`And()`](xref:Examine.Search.IBooleanOperation#Examine_Search_IBooleanOperation_And), [`Or()`](xref:Examine.Search.IBooleanOperation#Examine_Search_IBooleanOperation_Or) and [`Not()`](xref:Examine.Search.IBooleanOperation#Examine_Search_IBooleanOperation_Not) to chain the next clause.

```csharp
var results = searcher.CreateQuery("content")
    .Field("nodeName", "hello")
    .And().Field("bodyText", "world")
    .Not().Field("writerName", "administrator")
    .Execute();
```

### Grouped fields

To match the same term or terms across several fields, use [`GroupedOr`](xref:Examine.Search.IQuery#Examine_Search_IQuery_GroupedOr_System_Collections_Generic_IEnumerable_System_String__System_String___), [`GroupedAnd`](xref:Examine.Search.IQuery#Examine_Search_IQuery_GroupedAnd_System_Collections_Generic_IEnumerable_System_String__System_String___) or [`GroupedNot`](xref:Examine.Search.IQuery#Examine_Search_IQuery_GroupedNot_System_Collections_Generic_IEnumerable_System_String__System_String___). The first argument is the set of fields, the rest are the terms.

```csharp
// Match "hello" in either nodeName or bodyText
var results = searcher.CreateQuery("content")
    .GroupedOr(new[] { "nodeName", "bodyText" }, "hello")
    .Execute();
```

Each of these also accepts [`IExamineValue`](xref:Examine.Search.IExamineValue) terms, so wildcards, fuzziness and boosting can be applied per term:

```csharp
var results = searcher.CreateQuery("content")
    .GroupedOr(
        new[] { "nodeName", "bodyText" },
        "hello".Boost(10),
        "world".Fuzzy())
    .Execute();
```

### Sub groups

[`Group`](xref:Examine.Search.IQuery#Examine_Search_IQuery_Group_System_Func_Examine_Search_INestedQuery_Examine_Search_INestedBooleanOperation__Examine_Search_BooleanOperation_) builds a nested, bracketed query. The inner callback uses [`INestedQuery`](xref:Examine.Search.INestedQuery), which mirrors the outer query API. The `defaultOp` parameter controls how the clauses inside the group combine, defaulting to `Or`.

```csharp
// nodeTypeAlias:CWS_Home AND (nodeName:home OR bodyText:home)
var results = searcher.CreateQuery("content")
    .Field("nodeTypeAlias", "CWS_Home")
    .And().Group(group => group
        .Field("nodeName", "home")
        .Or().Field("bodyText", "home"))
    .Execute();
```

[`And`](xref:Examine.Search.IBooleanOperation#Examine_Search_IBooleanOperation_And_System_Func_Examine_Search_INestedQuery_Examine_Search_INestedBooleanOperation__Examine_Search_BooleanOperation_), [`Or`](xref:Examine.Search.IBooleanOperation#Examine_Search_IBooleanOperation_Or_System_Func_Examine_Search_INestedQuery_Examine_Search_INestedBooleanOperation__Examine_Search_BooleanOperation_) and [`AndNot`](xref:Examine.Search.IBooleanOperation#Examine_Search_IBooleanOperation_AndNot_System_Func_Examine_Search_INestedQuery_Examine_Search_INestedBooleanOperation__Examine_Search_BooleanOperation_) have overloads that take the same nested callback, which is how sub groups are combined with the rest of the query.

## Selecting fields

By default all stored fields are loaded for every result. When only a few fields are needed, restricting the loaded set reduces the work done per result.

```csharp
// Load a single field
var results = searcher.CreateQuery()
    .Field("Address", "Hills")
    .SelectField("Name")
    .Execute();

// Load a specific set of fields
var results2 = searcher.CreateQuery()
    .Field("Address", "Hills")
    .SelectFields(new HashSet<string> { "Name", "Address" })
    .Execute();

// Explicitly load everything (the default)
var results3 = searcher.CreateQuery()
    .Field("Address", "Hills")
    .SelectAllFields()
    .Execute();
```

_**Note**: each of these replaces the previously selected set rather than adding to it - calling [`SelectField`](xref:Examine.Search.IOrdering#Examine_Search_IOrdering_SelectField_System_String_) twice loads only the field named in the second call. Use [`SelectFields`](xref:Examine.Search.IOrdering#Examine_Search_IOrdering_SelectFields_System_Collections_Generic_ISet_System_String__) to load more than one field, and [`SelectAllFields`](xref:Examine.Search.IOrdering#Examine_Search_IOrdering_SelectAllFields) to reset back to the default._

## Boosting

Boosting is the practice of making some parts of your query more relevant than others. This means that you can have terms that will make entries matching that term score higher in the search results.

Example:

```csharp
var searcher = indexer.Searcher;
var results = searcher.CreateQuery("content")
    .Field("nodeTypeAlias", "CWS_Home".Boost(20))
    .Execute();
```

This will boost the term `CWS_Home` and make entries with `nodeTypeAlias:CWS_Home` score higher in the results.

[`Boost`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Boost_System_String_System_Single_) creates a new [`IExamineValue`](xref:Examine.Search.IExamineValue) from a string. To apply a boost to a term that already has other behavior - a wildcard or a fuzzy match, for example - use [`WithBoost`](xref:Examine.Search.ExamineValueExtensions#Examine_Search_ExamineValueExtensions_WithBoost_Examine_Search_IExamineValue_System_Single_) instead:

```csharp
var results = searcher.CreateQuery("content")
    // A fuzzy match that is also boosted
    .Field("nodeName", "hello".Fuzzy(0.7f).WithBoost(20))
    // A multiple character wildcard that is also boosted
    .Or().Field("bodyText", "hell".MultipleCharacterWildcard().WithBoost(5))
    .Execute();
```

## Proximity

Proximity searching helps in finding entries where words that are within a specific distance away from each other.

Example:

```csharp
var searcher = indexer.Searcher;
var results = searcher.CreateQuery("content")
    // Get all nodes that contain the words warren and creative within 5 words of each other
    .Field("metaKeywords", "Warren creative".Proximity(5))
    .Execute();
```

## Fuzzy

Fuzzy searching is the practice of finding spellings that are similar to each other. Examine searches based on the [Damerau-Levenshtein Distance](https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance). The parameter given in the [`.Fuzzy()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Fuzzy_System_String_) method is the edit distance allowed by default this value is `0.5` if not specified.

The value on [`.Fuzzy()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Fuzzy_System_String_System_Single_) can be between 0 and 2. Any number higher than 2 will be lowered to 2 when creating the query.

Example:

```csharp
var searcher = indexer.Searcher;
var results = searcher.CreateQuery()
    .Field("Content", "think".Fuzzy(0.1F))
    .Execute();
```

## Escape and phrase queries

[`Escape()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Escape_System_String_) turns a string into a single quoted phrase, so the whole value is matched verbatim rather than being split into terms by the analyzer.

```csharp
var searcher = indexer.Searcher;
var results = searcher.CreateQuery("content")
    .Field("__Path", "-1,123,456,789".Escape())
    .Execute();
```

[`Phrase()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Phrase_System_String_) is the same operation under a name that describes the intent more directly - `Escape()` is implemented as a call to `Phrase()`. Use whichever reads better: `Escape()` when the point is to neutralize special characters, `Phrase()` when the point is to match an exact sequence of words.

```csharp
var results = searcher.CreateQuery()
    // Matches the exact phrase, not the three terms separately
    .Field("Address", "Hills Rockyroad Hollywood".Phrase())
    .Execute();
```

Neither can be combined with the wildcard methods below.

## Wildcards

Examine supports single and multiple-character wildcards on single terms. (Cannot be used with the `.Escape()` method)

### Examine query (Single character)

The [`.SingleCharacterWildcard()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Escape_System_String_) method will add a single character wildcard to the end of the term or terms being searched on a field. 

```csharp
var query = searcher.CreateQuery()
    .Field("type", "test".SingleCharacterWildcard());
```

This will match for example: `test` and `tests`

### Examine query (Multiple characters)

The [`.MultipleCharacterWildcard()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_MultipleCharacterWildcard_System_String_) method will add a multiple characters wildcard to the end of the term or terms being searched on a field.

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


## Faceting

Faceting counts the documents matching a query, grouped by the value of a field or by a numeric range. It is the headline addition in Examine V4.

Faceting is opt-in per field. A field must be mapped to one of the `Facet*` [value types](xref:configuration#value-types) before it can be faceted on - see [facets configuration](xref:configuration#facets-configuration).

Facets are requested with [`WithFacets`](xref:Examine.Search.IFaceting#Examine_Search_IFaceting_WithFacets_System_Action_Examine_Search_IFacetOperations__), which is available on any query once it has a clause. Inside the callback, add one or more facets using the [`IFacetOperations`](xref:Examine.Search.IFacetOperations) methods:

* [`FacetString`](xref:Examine.Search.IFacetOperations#Examine_Search_IFacetOperations_FacetString_System_String_System_Action_Examine_Search_IFacetQueryField__System_String___) - facet on a string field, optionally filtered to specific values
* [`FacetLongRange`](xref:Examine.Search.IFacetOperations#Examine_Search_IFacetOperations_FacetLongRange_System_String_Examine_Search_Int64Range___) - facet on `int`/`long`/`DateTime` ranges
* [`FacetDoubleRange`](xref:Examine.Search.IFacetOperations#Examine_Search_IFacetOperations_FacetDoubleRange_System_String_Examine_Search_DoubleRange___) - facet on `double` ranges
* [`FacetFloatRange`](xref:Examine.Search.IFacetOperations#Examine_Search_IFacetOperations_FacetFloatRange_System_String_Examine_Search_FloatRange___) - facet on `float` ranges

Results are read back with the [`GetFacet`](xref:Examine.Lucene.FacetExtensions#Examine_Lucene_FacetExtensions_GetFacet_Examine_ISearchResults_System_String_) and [`GetFacets`](xref:Examine.Lucene.FacetExtensions#Examine_Lucene_FacetExtensions_GetFacets_Examine_ISearchResults_) extension methods.

### Reading facet results

[`GetFacet`](xref:Examine.Lucene.FacetExtensions#Examine_Lucene_FacetExtensions_GetFacet_Examine_ISearchResults_System_String_) returns a nullable [`IFacetResult`](xref:Examine.Search.IFacetResult) - it is `null` when the query did not request a facet for that field. [`IFacetResult`](xref:Examine.Search.IFacetResult) is an `IEnumerable<`[`IFacetValue`](xref:Examine.Search.IFacetValue)`>`, and each [`IFacetValue`](xref:Examine.Search.IFacetValue) has a `Label` (`string`) and a `Value` (`float`, the document count).

```csharp
var results = searcher.CreateQuery()
    .Field("Address", "Hills")
    .WithFacets(facets => facets.FacetString("Address"))
    .Execute();

// null if no "Address" facet was requested
var addressFacet = results.GetFacet("Address");

if (addressFacet is not null)
{
    foreach (var value in addressFacet)
    {
        Console.WriteLine($"{value.Label}: {value.Value}");
    }
}
```

[`IFacetResult.Facet(label)`](xref:Examine.Search.IFacetResult#Examine_Search_IFacetResult_Facet_System_String_) looks up a single value and is also nullable, so [`TryGetFacet`](xref:Examine.Search.IFacetResult#Examine_Search_IFacetResult_TryGetFacet_System_String_Examine_Search_IFacetValue__) is usually the more convenient form:

```csharp
if (addressFacet is not null && addressFacet.TryGetFacet("Hills", out var hills))
{
    Console.WriteLine($"Hills: {hills!.Value}");
}
```

To enumerate every facet on the result set rather than naming one field, use [`GetFacets`](xref:Examine.Lucene.FacetExtensions#Examine_Lucene_FacetExtensions_GetFacets_Examine_ISearchResults_):

```csharp
foreach (IFacetResult facet in results.GetFacets())
{
    foreach (var value in facet)
    {
        Console.WriteLine($"{value.Label}: {value.Value}");
    }
}
```

_**Note**: both methods throw [`NotSupportedException`](https://learn.microsoft.com/en-us/dotnet/api/system.notsupportedexception) if the results did not come from a faceted query._

### String facets

String facets count the documents that share the same string value. This works with both the `Facet*` and `FacetTaxonomy*` field value types.

#### Basic example

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

var hillsValue = addressFacetResults?.Facet("Hills"); // Gets the IFacetValue for the facet Hills
```

#### Filtered value example

Pass one or more values to restrict the facet to only those labels.

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .Field("Address", "Hills")
    .WithFacets(facets => facets.FacetString("Address", values: "Hills"))
    .Execute();

var addressFacetResults = results.GetFacet("Address");

/* 
* Example value
* Label: Hills, Value: 2 <-- As Hills was the only filtered value we will only get this facet
*/
```

#### MaxCount example

[`IFacetQueryField.MaxCount`](xref:Examine.Search.IFacetQueryField#Examine_Search_IFacetQueryField_MaxCount_System_Int32_) limits how many facet values are returned, keeping the highest counts.

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .Field("Address", "Hills")
    .WithFacets(facets => facets.FacetString("Address"))
    .Execute();

var addressFacetResults = results.GetFacet("Address");

/* 
* Example value
* Label: Hills, Value: 2
* Label: Hollywood, Value: 10
* Label: London, Value: 12
*/

results = searcher.CreateQuery()
    .Field("Address", "Hills")
    // Gets the top 2 results (the facets with the highest value)
    .WithFacets(facets => facets.FacetString("Address", config => config.MaxCount(2)))
    .Execute();

addressFacetResults = results.GetFacet("Address");

/* 
* Example value (Notice only 2 values are present)
* Label: Hollywood, Value: 10
* Label: London, Value: 12
*/
```

#### Custom facet index field example

By default string facets are stored under a shared `$facets` field. [`FacetsConfig.SetIndexFieldName`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/facet/Lucene.Net.Facet.FacetsConfig.html) stores a field's facets separately. The query does not change - the facet field name is read from the [`FacetsConfig`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/facet/Lucene.Net.Facet.FacetsConfig.html) automatically.

```csharp
// Setup
services.AddExamineLuceneIndex("MyIndex", options =>
{
    // Set the indexing of your fields to use the facet type
    options.FieldDefinitions = new FieldDefinitionCollection(
        new FieldDefinition("Address", FieldDefinitionTypes.FacetFullText));

    // Store facets of this field under facet_address instead of the default $facets
    options.FacetsConfig.SetIndexFieldName("Address", "facet_address");
});


var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .Field("Address", "Hills")
    .WithFacets(facets => facets.FacetString("Address"))
    .Execute();

var addressFacetResults = results.GetFacet("Address");

/* 
* Example value
* Label: Hills, Value: 2
* Label: Hollywood, Value: 10
*/
```

### Numeric range facets

Numeric range facets group documents into ranges you define, for example a price band. Each range is labelled, and the label is what comes back as the [`IFacetValue.Label`](xref:Examine.Search.IFacetValue#Examine_Search_IFacetValue_Label).

Examine has three range types in the [`Examine.Search`](xref:Examine.Search) namespace - [`DoubleRange`](xref:Examine.Search.DoubleRange), [`FloatRange`](xref:Examine.Search.FloatRange) and [`Int64Range`](xref:Examine.Search.Int64Range) - each constructed as `(label, min, minInclusive, max, maxInclusive)`. Use [`Int64Range`](xref:Examine.Search.Int64Range) for `int`, `long` and `DateTime` fields.

#### Double range example

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .All()
    .WithFacets(facets => facets.FacetDoubleRange("Price",
        new DoubleRange("0-10", 0, true, 10, true),
        new DoubleRange("11-20", 11, true, 20, true)))
    .Execute();

var priceFacetResults = results.GetFacet("Price");

/* 
* Example value
* Label: 0-10, Value: 2
* Label: 11-20, Value: 10
*/

var firstRangeValue = priceFacetResults?.Facet("0-10"); // Gets the IFacetValue for the facet "0-10"
```

#### Float range example

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .All()
    .WithFacets(facets => facets.FacetFloatRange("Price",
        new FloatRange("0-10", 0f, true, 10f, true),
        new FloatRange("11-20", 11f, true, 20f, true)))
    .Execute();

var priceFacetResults = results.GetFacet("Price");

/* 
* Example value
* Label: 0-10, Value: 2
* Label: 11-20, Value: 10
*/
```

#### Int/Long range example

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .All()
    .WithFacets(facets => facets.FacetLongRange("Price",
        new Int64Range("0-10", 0, true, 10, true),
        new Int64Range("11-20", 11, true, 20, true)))
    .Execute();

var priceFacetResults = results.GetFacet("Price");

/* 
* Example value
* Label: 0-10, Value: 2
* Label: 11-20, Value: 10
*/
```

#### DateTime range example

`DateTime` fields are stored as ticks, so range bounds are expressed with [`DateTime.Ticks`](https://learn.microsoft.com/en-us/dotnet/api/system.datetime.ticks).

```csharp
var searcher = myIndex.Searcher;
var results = searcher.CreateQuery()
    .All()
    .WithFacets(facets => facets.FacetLongRange("Created",
        new Int64Range("first", DateTime.UtcNow.AddDays(-1).Ticks, true, DateTime.UtcNow.Ticks, true),
        new Int64Range("last", DateTime.UtcNow.AddDays(1).Ticks, true, DateTime.UtcNow.AddDays(2).Ticks, true)))
    .Execute();

var createdFacetResults = results.GetFacet("Created");

/* 
* Example value
* Label: first, Value: 2
* Label: last, Value: 10
*/
```

### Taxonomy facets

When an index is configured to use the [taxonomy sidecar index](xref:configuration#hierarchical-and-taxonomy-facets-configuration), faceted queries should be issued against [`LuceneIndex.TaxonomySearcher`](xref:Examine.Lucene.Providers.LuceneIndex#Examine_Lucene_Providers_LuceneIndex_TaxonomySearcher) rather than `Searcher`. The query API is otherwise identical.

```csharp
var taxonomySearcher = myIndex.TaxonomySearcher!;

var results = taxonomySearcher.CreateQuery("content")
    .Field("writerName", "administrator")
    .WithFacets(facets => facets.FacetString("nodeName"))
    .Execute();

var nodeNameFacet = results.GetFacet("nodeName");
```

[`ILuceneTaxonomySearcher.CategoryCount`](xref:Examine.Lucene.Providers.ILuceneTaxonomySearcher#Examine_Lucene_Providers_ILuceneTaxonomySearcher_CategoryCount) reports the number of categories currently held in the taxonomy index, which is useful when verifying that hierarchical facets are being written as expected.

### Facets and paging

Faceted queries work with [deep paging](xref:paging#deep-paging). Facet counts are calculated over the whole matching set, not just the current page, so they stay stable as you page through results.

## Lucene queries

Find a reference to how to write Lucene queries in the [Lucene 4.8.0 docs](https://lucene.apache.org/core/4_8_0/queryparser/org/apache/lucene/queryparser/classic/package-summary.html#package_description).

### Native Query

```csharp
var searcher = indexer.Searcher;
var results = searcher.CreateQuery()
    .NativeQuery("hello:world")
    .Execute();
```

### Combine a native query and Fluent API searching.

```csharp
var searcher = indexer.Searcher;
var results = searcher.CreateQuery()
    .NativeQuery("hello:world")
    .And().Field("Address", "Hills") // Combine queries
    .Execute();
```

### Combine a custom lucene query with raw lucene query

```csharp
var searcher = indexer.Searcher;
var criteria = (LuceneSearchQuery)searcher.CreateQuery();

// Make the query ready for extending
var op = criteria.NativeQuery("hello:world").And();

// Add the raw lucene query
var results = criteria
    .LuceneQuery(NumericRangeQuery.NewInt64Range("numTest", 4, 5, true, true))
    .Execute();
```
