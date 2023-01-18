---
layout: page
title: Suggesting
permalink: /suggesting
ref: suggesting
order: 2
---
Suggesting and Spell Checking
===

_**Tip**: There are many examples of searching in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Lucene/Suggest/SuggesterApiTests.cs) to use as examples/reference._

## Suggester API

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery().SourceField("FullName");
var results = query.Execute("Sam", new SuggestionOptions(5));
```

This code will run a suggestion for the input text "Sam" with the "FullName" field in the index as the source of terms for the suggestions, returning up to 5 suggestions.

## Lucene Suggesters

To generate suggestions for input text, retreive the ISuggester from the index IIndex.Suggester.

### Analyzing Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery().SourceField("FullName");
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingSuggester));
```

This code will run a suggestion for the input text "Sam" with the "FullName" field in the index as the source of terms for the suggestions, returning up to 5 suggestions.

### Fuzzy Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery().SourceField("FullName");
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.FuzzySuggester));
```

This code will run a Fuzzy suggestion for the input text "Sam" with the "FullName" field in the index as the source of terms for the suggestions, returning up to 5 suggestions.

### Direct SpellChecker Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery().SourceField("FullName");
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.DirectSpellChecker));
```

This code will run a spellchecker suggestion for the input text "Sam" returning up to 5 suggestions.

### Custom Implementation

To use a suggester data source other than a field in the index or to use another Suggester, override the Lookup property of the IIndexFieldValueType for the field.

```cs

    /// <summary>
    /// Defines how a field value is stored in the index and is responsible for generating a query for the field when a managed query is used
    /// </summary>
    public interface IIndexFieldValueType
    {
        ...
        /// <summary>
        /// Returns the lookup for this field type, or null to use the default
        /// </summary>
        Func<IIndexReaderReference, SuggestionOptions, string, LuceneSuggestionResults> Lookup { get; }
        ...
    }
```
