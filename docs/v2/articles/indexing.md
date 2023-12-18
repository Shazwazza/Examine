---
layout: page
title: Indexing
permalink: /indexing
uid: indexing
order: 0
---

Indexing
===
_**Tip**: There are many examples of indexing in the [`LuceneIndexTests` source code](https://github.com/Shazwazza/Examine/blob/dev/src/Examine.Test/Index/LuceneIndexTests.cs) to use as examples/reference._

Examine will index any data you give it within a [`ValueSet`](xref:Examine.ValueSet). You can index one or multiple items at once and there's a few different ways to do that. Each field in a [`ValueSet`](xref:Examine.ValueSet) can also contain one or more values.

A [`ValueSet`](xref:Examine.ValueSet) is fairly simple, it is really just:

* __Id__ _`string`_ - unique identifier for the document
* __Category__ _`string`_ - Required. 1st level categorization
* __ItemType__ _`string`_ - Optional. 2nd level categorization
* __Values__ _`IDictionary<string, List<object>>`_ - Any data associated with the document

It also has some methods that you can use to manipulate it's data.

## Single values

_How to index a single [`ValueSet`](xref:Examine.ValueSet)

[See quickstart](xref:index#quick-start)

## Multiple values

_How to index multiple [`ValueSet`](xref:Examine.ValueSet) at once_

### With Dictionaries (default)

```cs
myIndex.IndexItems(new[]
{
    new ValueSet(
        "SKU123", 
        "Product",
        new Dictionary<string, object>()
        {
            {"Name", "Loud Headphones" },
            {"Brand", "LOUDER" }
        }),
    new ValueSet(
        "SKU987", 
        "Product",
        new Dictionary<string, object>()
        {
            {"Name", "USB-C Cable" },
            {"Brand", "Cablez-R-Us" }
        }),
});
```

### With Objects

```cs

// For example, perhaps you looked up the product from a service
var headphones = ProductService.Get("SKU123");

myIndex.IndexItems(new[]
{
    ValueSet.FromObject(
        headphones.Id, 
        "Product",
        headphones,
    ValueSet.FromObject(
        "SKU987", 
        "Product",
        new         //Anonymous objects work too
        {
            Name = "USB-C Cable",
            Brand = "Cablez-R-Us"
        }),
});
```

### Multiple values per field

It is possible to have multiple values for an individual field, you can just pass in an instance of `IDictionary<string, IEnumerable<object>>` to the [`ValueSet`](xref:Examine.ValueSet) constructor.

```cs
myIndex.IndexItem(new ValueSet(
    Guid.NewGuid().ToString(),
    "TestType",
    new Dictionary<string, IEnumerable<object>>()
    {
        {"Name", new object[]{ "Frank" }},
        // For example, perhaps each address part is a separate value
        {"Address", new object[]{ "Beverly Hills", "90210" } } 
    }));
```

### Strongly typed

As you can see, the values being passed into the ValueSet are type `object`. Examine will determine if the object type maps to a field definition

```cs
myIndex.IndexItem(new ValueSet(
    "SKU987",
    "Product",             
    new Dictionary<string, object>()
    {
        {"Name", "USB-C Cable" },
        {"Brand", "Cablez-R-Us" },
        {"Price", 19.99}  // non-string value
    }));
```

### Synchronously

Be default all indexing is done asynchronously. If you need to run indexing synchronously you should create a synchronous scope. This is for instance a necessary step for unit tests.

```cs
using (var luceneIndex = (myIndex as LuceneIndex))
using (var syncIndexContext = luceneIndex.WithThreadingMode(IndexThreadingMode.Synchronous))
{
    luceneIndex.IndexItem(new ValueSet(
        "SKU987",
        "Product",             
        new Dictionary<string, object>()
        {
            {"Name", "USB-C Cable" },
            {"Brand", "Cablez-R-Us" },
            {"Price", 19.99}  // non-string value
        }));
}
```

## Deleting index data

Data is easily deleted from the index by the unique identifier you provided in your [`ValueSet`](xref:Examine.ValueSet) by using the `DeleteFromIndex` method. For example:

```cs
 indexer.DeleteFromIndex("SKU987");
```

## Events

#### [IIndex.IndexOperationComplete](xref:Examine.IIndex#Examine_IIndex_IndexOperationComplete)

This event is part of the base interface [`IIndex`](xref:Examine.IIndex) so it is available to use on any implementation of an Examine index. This can be useful to know when an indexing operation is completed.

Example of how to listen the event:

```csharp
if (!_examineManager.TryGetIndex(indexName, out var index))
{
    throw new ArgumentException($"Index '{indexName}' not found");
}

index.IndexOperationComplete += IndexOperationComplete;
```

```csharp
private void IndexOperationComplete(object sender, IndexOperationEventArgs e)
{
    // Index operation completed
}
```

#### [IIndex.TransformingIndexValues](xref:Examine.IIndex#Examine_IIndex_TransformingIndexValues)

This event is part of the base interface [`IIndex`](xref:Examine.IIndex) so it is available to use on any implementation of an Examine index. This event allows for customizing the [`ValueSet`](xref:Examine.ValueSet) before it is passed to the indexer to be indexed. You can use this event to add additional field values or modify existing field values.

Example of how to listen the event:

```csharp
if (!_examineManager.TryGetIndex(indexName, out var index))
{
    throw new ArgumentException($"Index '{indexName}' not found");
}

index.TransformingIndexValues += TransformingIndexValues;
```

```csharp
private void TransformingIndexValues(object sender, IndexingItemEventArgs e)
{
    // Customize the ValueSet
}
```

#### [IIndex.IndexingError](xref:Examine.IIndex#Examine_IIndex_IndexingError)

This event is part of the base interface [`IIndex`](xref:Examine.IIndex) so it is available to use on any implementation of an Examine index. This event can be used for reacting to when an error occurs during index. For example, you could add an event handler for this event to facilitate error logging.

Example of how to listen the event:

```csharp
if (!_examineManager.TryGetIndex(indexName, out var index))
{
    throw new ArgumentException($"Index '{indexName}' not found");
}

index.IndexingError += IndexingError;
```

```csharp
private void IndexingError(object sender, IndexingErrorEventArgs e)
{
    // An indexing error occored
}
```

#### [LuceneIndex.DocumentWriting](xref:Examine.Lucene.Providers.LuceneIndex#Examine_Lucene_Providers_LuceneIndex_DocumentWriting)

If using Examine with the default Lucene implementation then the [`IIndex`](xref:Examine.IIndex) implementation will be [`LuceneIndex`](xref:Examine.Lucene.Providers.LuceneIndex). This event provides access to the Lucene [`Document`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Documents.Document.html) object before it gets added to the Lucene Index.

You can use this event to entirely customize how the data is stored in the Lucene index, including adding custom boosting profiles, changing the [`Document`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Documents.Document.html)'s field values or types, etc...

Example of how to listen the event:

```csharp
if (!_examineManager.TryGetIndex(indexName, out var index))
{
    throw new ArgumentException($"Index '{indexName}' not found");
}

if (index is LuceneIndex luceneIndex){
    luceneIndex.DocumentWriting += DocumentWriting;
}
```

```csharp
private void DocumentWriting(object sender, DocumentWritingEventArgs e)
{
    // Customize how the data is stored in the Lucene index
}
```

### [LuceneIndex.IndexCommitted](xref:Examine.Lucene.Providers.LuceneIndex#Examine_Lucene_Providers_LuceneIndex_IndexCommitted)
If using Examine with the default Lucene implementation then the [`IIndex`](xref:Examine.IIndex) implementation will be [`LuceneIndex`](xref:Examine.Lucene.Providers.LuceneIndex). This event is triggered when the index is commited. For example when clearing the index this event is run once when commiting. When rebuilding the event will be run once the rebuild commits a clearing of the index and when it's commiting the rebuilt index.
Example of how to listen the event:

```csharp
if (!_examineManager.TryGetIndex(indexName, out var index))
{
    throw new ArgumentException($"Index '{indexName}' not found");
}

if (index is LuceneIndex luceneIndex){
    luceneIndex.IndexCommitted += IndexCommited;
}
```

```csharp
private void IndexCommited(object sender, EventArgs e)
{
    // Triggered when the index is commited
}
```