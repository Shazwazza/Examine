---
layout: page
title: Sorting
permalink: /sorting.html
ref: sorting
order: 3
---

Sorting
===

_**Tip**: There are many examples of sorting in the [`FluentApiTests` source code](https://github.com/Shazwazza/Examine/blob/master/src/Examine.Test/Search/FluentApiTests.cs) to use as examples/reference._

## Score

By default search results are ordered by Score descending so there's nothing specific that needs to be done to support this. If you use a different sorting operation then the `Score` value will be 0 for all results.

## Custom sorting

_TODO: Fill this in..._

## Limiting results

_TODO: Fill this in..._

## Paging

There's a [blog post writeup here](https://shazwazza.com/post/paging-with-examine/) on how to properly page with Examine (and Lucene).

There are 2 important parts to this:

* The [Skip](https://github.com/Shazwazza/Examine/blob/master/src/Examine/ISearchResults.cs#L11) method on the `ISearchResults` object
* The [Search](https://github.com/Shazwazza/Examine/blob/master/src/Examine/Providers/BaseSearchProvider.cs#L22) overload on the `BaseSearchProvider` where you can specify `maxResults`

`ISearchResults.Skip` is very different from the Linq Skip method so you need to be sure you are using the `Skip` method on the `ISearchResults` object. This tells Lucene to skip over a specific number of results without allocating the result objects. If you use Linq’s Skip method on the underlying `IEnumerable<SearchResult>` of `ISearchResults`, this will allocate all of the result objects and then filter them in memory which is what you don’t want to do.

Lucene isn't perfect for paging because it doesn’t natively support the Linq equivalent to "Skip/Take" _(UPDATE: In an upcoming Examine version, it can natively support this!)_. It understands Skip (as above) but doesn't understand Take, instead it only knows how to limit the max results so that it doesn’t allocate every result, most of which you would probably not need when paging.

With the combination of `ISearchResult.Skip` and `maxResults`, we can tell Lucene to:

* Skip over a certain number of results without allocating them and tell Lucene
* only allocate a certain number of results after skipping

### Example

```cs
//for example purposes, we want to show page #4 (which is pageIndex of 3)
var pageIndex = 3;   
//for this example, the page size is 10 items
var pageSize = 10;
var searchResult = searchProvider.Search(criteria, 
   //don't return more results than we need for the paging
   //this is the 'trick' - we need to load enough search results to fill
   //all pages from 1 to the current page of 4
   maxResults: pageSize*(pageIndex + 1));
//then we use the Skip method to tell Lucene to not allocate search results
//for the first 3 pages
var pagedResults = searchResult.Skip(pageIndex*pageSize);
var totalResults = searchResult.TotalItemCount;
```