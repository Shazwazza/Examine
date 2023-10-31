---
layout: page
title: Suggesting
permalink: /suggesting
ref: suggesting
order: 2
---
Suggesting and Spell Checking
===

_**Tip**: There are many examples of searching in the [`SuggesterApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Lucene/Suggest/SuggesterApiTests.cs) to use as examples/reference._


## Registering Suggesters

On the index to register the Suggesters, create a SuggesterDefinitionCollection and set it on IndexOptions.SuggesterDefinitions

Examples for suggesting or spellchecking based on the "fullName" index field.

```cs
 var suggesters = new SuggesterDefinitionCollection();
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.AnalyzingInfixSuggester, ExamineLuceneSuggesterNames.AnalyzingInfixSuggester, new string[] { "fullName" }));
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.AnalyzingSuggester, ExamineLuceneSuggesterNames.AnalyzingSuggester, new string[] { "fullName" }));
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker, ExamineLuceneSuggesterNames.DirectSpellChecker, new string[] { "fullName" }));
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker_LevensteinDistance, ExamineLuceneSuggesterNames.DirectSpellChecker_LevensteinDistance, new string[] { "fullName" }));
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance, ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance, new string[] { "fullName" }));
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker_NGramDistance, ExamineLuceneSuggesterNames.DirectSpellChecker_NGramDistance, new string[] { "fullName" }));
            suggesters.AddOrUpdate(new SuggesterDefinition(ExamineLuceneSuggesterNames.FuzzySuggester, ExamineLuceneSuggesterNames.FuzzySuggester, new string[] { "fullName" }));
```

## Suggester API

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery();
var results = query.Execute("Sam", new SuggestionOptions(5,ExamineLuceneSuggesterNames.AnalyzingSuggester));
```

This code will run a suggestion for the input text "Sam", returning up to 5 suggestions.

## Lucene Suggesters

To generate suggestions for input text, retreive the ISuggester from the index IIndex.Suggester.

### Analyzing Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery();
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingInfixSuggester));
```

This code will run a suggestion for the input text "Sam", returning up to 5 suggestions, and will highlight the result text which matches the input text..

### Analyzing Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery();
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingSuggester));
```

This code will run a suggestion for the input text "Sam", returning up to 5 suggestions.

### Fuzzy Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery();
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.FuzzySuggester));
```

This code will run a Fuzzy suggestion for the input text "Sam", returning up to 5 suggestions.

## Lucene Spellcheckers

To generate spellchecker suggestions for input text, retreive the ISuggester from the index IIndex.Suggester.

### Direct SpellChecker Suggester

```cs
var suggester = index.Suggester;
var query = suggester.CreateSuggestionQuery();
var results = query.Execute("Sam", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.DirectSpellChecker));
```

This code will run a spellchecker suggestion for the input text "Sam" returning up to 5 suggestions.
