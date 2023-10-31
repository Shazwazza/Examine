---
uid: v2index
title: V1/V2 Quickstart
---

Examine Documentation
===

## What is Examine?

<img align="right" src="https://github.com/Shazwazza/Examine/raw/master/assets/logo-round-small.png?raw=true"> Examine allows you to index and search data easily and wraps the Lucene.Net indexing/searching engine. Lucene is _super_ fast and allows for very fast searching even on very large amounts of data. Examine is very extensible and allows you to configure as many indexes as you like and each may be configured individually. Out of the box Examine gives you a Lucene based index implementation as well as a Fluent API that can be used to search for your data.

Examine is installed via Nuget: [https://www.nuget.org/packages/Examine](https://www.nuget.org/packages/Examine)

## [Conceptual Documentation](xref:v2indexing)
Conceptual documentation is available.


_**Tip**: There are many unit tests in the source code that can be used as Examples of how to do things. There is also a test web project that has plenty of examples of how to configure indexes and search them._

* [Indexing](xref:v2indexing)
* [Configuration](xref:v2configuration)
* [Searching](xref:v2searching)
* [Sorting](xref:v2sorting)

## Minimum requirements

| Examine Version | .NET |
| --------------- | ---- |
| V2 | .NET Standard 2.0 |
| V1 | .NET Framework 4.5.2 |

## Quick Start


#### Examine V2

**Tip**: `IExamineManager` is the gateway to working with examine. It is registered in DI as a singleton and can be injected into your services._

1. Install

    ```powershell
    > dotnet add package Examine --version 2.1.0
    ```

1. Configure Services and create an index

    ```cs

    // Adds Examine Core services
    services.AddExamine();

    // Create a Lucene based index
    services.AddExamineLuceneIndex("MyIndex");
    ```

1. Populate the index

    ```cs
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
    ```

1. Search the index

    ```cs
    // Create a query
    var results = myIndex.Searcher.CreateQuery()
        .Field("Address", "Hills")        // Look for any "Hills" addresses
        .Execute();                       // Execute the search
    ```

#### Examine V1

**Tip**: `IExamineManager` is the gateway to working with examine. It can be registered in DI as a singleton or can be accessed via `ExamineManager.Instance`._

1. Install

    ```powershell
    PM> Install-Package Examine -Version 1.2.2
    ```

1. Create an index

    ```cs
    public void CreateIndexes(IExamineManager examineManager)
    {
        //Create and add a new index to the manager
        var myIndex = examineManager.AddIndex(
            new LuceneIndex(            // Create a Lucene based index
                "MyIndex",              // Named MyIndex
                new SimpleFSDirectory(  // In a location of your choice
                    new DirectoryInfo("C:\\TestIndexes"))));
    }
    ```

1. Populate the index

    ```cs
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
    ```

1. Search the index

    ```cs
    var searcher = myIndex.GetSearcher(); // Get a searcher
    var results = searcher.CreateQuery()  // Create a query
        .Field("Address", "Hills")        // Look for any "Hills" addresses
        .Execute();                       // Execute the search
    ```