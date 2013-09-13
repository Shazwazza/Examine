![Examine](http://shazwazza.com/Content/Downloads/ExamineLogo.png)

##What is Examine?

Examine allows you to index and search data easily and wraps the Lucene.Net indexing/searching engine. Lucene is _super_ fast and allows for very fast searching even on very large amounts of data. Examine is provider based so it is very extensible and allows you to configure as many indexes as you like and each may be configured individually. Out of the box Examine gives you abstract implementations of Lucene based indexers and searchers as well as a Fluent API that can be used to search for your data.

##Features
* Examine is based on a provider model, so the sky is the limit with regards to functionality if you need to build it.
* The base index and search providers are all based on Lucene.Net
* The base index providers in Examine have tons of handy events which gives you complete control over the entire indexing process without having to write your own provider. This makes it extremely easy to add custom data to indexes, intercept the data going into the index, and all sorts of other fun stuff.
* Out of the box, Examine has a fluent .Net querying language (it also supports a simple free text string search too!) Example:

```cs
  searchCriteria
    .Id(1080)
    .Or()
    .Field("headerText", "umb".Fuzzy())
    .And()
    .NodeTypeAlias("cws".MultipleCharacterWildcard())
    .Not()
    .NodeName("home");
```

* Hugely extensible through events
* Multiple indexes. You can create as many different indexes as you want which is handy for things like multilingual websites, portal sites, etc...
* Targetted indexing. You can specify via configuration as to what node types, properties, and node subsets (based on parent node) that you want included in your index.
* If you're a Lucene fanatic, you can specify any type of Analzers you want to use for your indexing or searching via configuration.

## Nuget

	PM> Install-Package Examine

## [Releases](https://github.com/Shandem/Examine/releases)

Information and downloads for Examine releases

## [Documentation](https://github.com/Shandem/Examine/wiki)

Documentation on using the Examine API

## Copyright & Licence

&copy; 2013 by Shannon Deminick

This is free software and is licensed under the [Microsoft Public License (Ms-PL)](http://opensource.org/licenses/MS-PL)
