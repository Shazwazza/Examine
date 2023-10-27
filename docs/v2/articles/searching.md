---
layout: page
title: Searching
permalink: /searching
uid: searching
order: 2
---
Searching
===

_**Tip**: There are many examples of searching in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/release/3.0/src/Examine.Test/Examine.Lucene/Search/FluentApiTests.cs) to use as examples/reference._


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

The way that terms are split depends on the Analyzer being used. The [`StandardAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-common/Lucene.Net.Analysis.Standard.StandardAnalyzer.html) is the default. An example of how Analyzers work are:

- [`StandardAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-common/Lucene.Net.Analysis.Standard.StandardAnalyzer.html) - will split a string based on whitespace and 'stop words' (i.e. common words that are not normally searched on like "and")
- [`WhitespaceAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-common/Lucene.Net.Analysis.Core.WhitespaceAnalyzer.html) - will split a string based only on whitespace
- [`KeywordAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-common/Lucene.Net.Analysis.Core.KeywordAnalyzer.html) - will not split a string and will treat the single string as one term - this means that searching will be done on an exact match

There are many [Analyzers](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Analysis.html) and you can even create your own. See more about analyzers in [configuration](./configuration.md#example---phone-number).

Looking at this example when using the default StandardAnalyser the code above means that the `Address` in this example has to match any the values set in the statement. This is because Examine will create a Lucene query like this one where every word is matched separately: `Address:hills Address:rockyroad Address:hollywood`.

Instead, if you want to search for entries with the values above in that exact order you specified you will need to use the [`.Escape()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Escape_System_String_) method. See under [Escape](#escape).

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

This will boost the term `CWS_Home` and make entries with `nodeTypeAlias:CWS_Home` score higher in the results.

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

Fuzzy searching is the practice of finding spellings that are similar to each other. Examine searches based on the [Damerau-Levenshtein Distance](https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance). The parameter given in the [`.Fuzzy()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Fuzzy_System_String_) method is the edit distance allowed by default this value is `0.5` if not specified.

The value on [`.Fuzzy()`](xref:Examine.SearchExtensions#Examine_SearchExtensions_Fuzzy_System_String_System_Single_) can be between 0 and 2. Any number higher than 2 will be lowered to 2 when creating the query.

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
