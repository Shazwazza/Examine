![Nuget](https://img.shields.io/nuget/v/Examine) [![Examine Build](https://github.com/Shazwazza/Examine/actions/workflows/build.yml/badge.svg)](https://github.com/Shazwazza/Examine/actions/workflows/build.yml) [![Build Status](https://dev.azure.com/shazwazza/Examine/_apis/build/status/Shazwazza.Examine?branchName=dev)](https://dev.azure.com/shazwazza/Examine/_build/latest?definitionId=4&branchName=dev) 

Examine
===

---
_❤️ If you use and like Examine please consider [becoming a GitHub Sponsor](https://github.com/sponsors/Shazwazza/) ❤️_

## What is Examine?

<img align="right" src="https://raw.githubusercontent.com/Shazwazza/Examine/master/assets/logo-round-small.png"> Examine allows you to index and search data easily and wraps the Lucene.Net indexing/searching engine. Lucene is _super_ fast and allows for very fast searching even on very large amounts of data. Examine is very extensible and allows you to configure as many indexes as you like and each may be configured individually. Out of the box Examine gives you a Lucene based index implementation as well as a Fluent API that can be used to search for your data.

## Installation

_via Nuget_

	PM> Install-Package Examine

## Quick start

_**Tip**: `IExamineManager` is the gateway to working with examine. It's a singleton service that is registered in DI._

1. Configure Services and create an index

    ```cs

    // Adds Examine Core services
    services.AddExamine();

    // Create a Lucene based index
    services.AddExamineLuceneIndex("MyIndex");
    ```
1. Populate the index

    ```cs
    if (examineManager.TryGetIndex("MyIndex", out var myIndex))
    {
        // Add a "ValueSet" (document) to the index 
        // which can contain any data you want.
        myIndex.IndexItem(new ValueSet(
            Guid.NewGuid().ToString(),  //Give the doc an ID of your choice
            "MyCategory",               //Each doc has a "Category"
            new Dictionary<string, object>()
            {
                {"Name", "Frank" },
                {"Address", "Beverly Hills, 90210" }
            }));
    }
    ```
1. Search the index

    ```cs
    var searcher = myIndex.Searcher; // Get a searcher
    var results = searcher.CreateQuery()  // Create a query
        .Field("Address", "Hills")        // Look for any "Hills" addresses
        .Execute();                       // Execute the search
    ```

## [Releases](https://github.com/Shandem/Examine/releases)

Information and downloads for Examine releases

## Documentation

The [documentation site is here](https://shazwazza.github.io/Examine/index.html)

_**Tip**: There are many unit tests in the source code that can be used as Examples of how to do things. There is also a test web project that has plenty of examples of how to configure indexes and search them._

* [Indexing](https://shazwazza.github.io/Examine/articles/indexing.html)
* [Configuration](https://shazwazza.github.io/Examine/articles/configuration.html)
* [Searching](https://shazwazza.github.io/Examine/articles/searching.html)
* [Sorting](https://shazwazza.github.io/Examine/articles/sorting.html)

## Copyright & Licence

&copy; 2023 by Shannon Deminick

This is free software and is licensed under the [Microsoft Public License (Ms-PL)](http://opensource.org/licenses/MS-PL)

<a href="https://www.freepik.com/free-photos-vectors/flat">Flat vector created by freepik - www.freepik.com</a>
