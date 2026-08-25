# Public API Changes Report

Generated: 2026-08-25 12:13:56

Compares the public API surface of **v3.10.0** against **v4.0.0**, across `Examine.Core`, `Examine.Host` and `Examine.Lucene`.

"Modified" entries are members whose signature changed, which is mostly nullable re-annotation of an otherwise identical member.

## Summary

- **Projects with changes:** 3
- **Total new APIs (safe):** 246
- **Total modified APIs:** 19 ⚠️ signature changes
- **Total breaking additions:** 1 ⚠️ abstract/interface members on existing types
- **Total removed APIs:** 14 ⚠️ **BREAKING**

## Project Breakdown

### Examine.Core

| Kind | Added | Modified | Breaking Additions | Removed |
|---|---:|---:|---:|---:|
| Type | 13 | 0 | 0 | 0 |
| Constant | 25 | 0 | 0 | 0 |
| Enum | 2 | 0 | 0 | 0 |
| Constructor | 6 | 0 | 0 | 1 |
| Property | 21 | 0 | 0 | 0 |
| Static | 5 | 0 | 0 | 0 |
| Member | 9 | 0 | 0 | 0 |
| **Total** | **81** | **0** | **0** | **1** |

#### ⚠️ Removed APIs (BREAKING) (1)

##### Constructor (1)

- `Examine.Search.ExamineValue.ExamineValue()`


#### ✅ Added APIs (Non-Breaking) (81)

##### Type (13)

- `Examine.Search.DoubleRange`
- `Examine.Search.ExamineValueExtensions`
- `Examine.Search.FloatRange`
- `Examine.Search.IExamineValueBoosted`
- `Examine.Search.IFaceting`
- `Examine.Search.IFacetLabel`
- `Examine.Search.IFacetOperations`
- `Examine.Search.IFacetQueryField`
- `Examine.Search.IFacetResult`
- `Examine.Search.IFacetResults`
- `Examine.Search.IFacetValue`
- `Examine.Search.Int64Range`
- `Examine.Search.OrderingExtensions`

##### Constant (25)

- `Examine.ExamineFieldNames.DefaultFacetsName` = "$facets" → *string*
- `Examine.FieldDefinitionTypes.FacetDateDay` = "facetdate.day" → *string*
- `Examine.FieldDefinitionTypes.FacetDateHour` = "facetdate.hour" → *string*
- `Examine.FieldDefinitionTypes.FacetDateMinute` = "facetdate.minute" → *string*
- `Examine.FieldDefinitionTypes.FacetDateMonth` = "facetdate.month" → *string*
- `Examine.FieldDefinitionTypes.FacetDateTime` = "facetdatetime" → *string*
- `Examine.FieldDefinitionTypes.FacetDateYear` = "facetdate.year" → *string*
- `Examine.FieldDefinitionTypes.FacetDouble` = "facetdouble" → *string*
- `Examine.FieldDefinitionTypes.FacetFloat` = "facetfloat" → *string*
- `Examine.FieldDefinitionTypes.FacetFullText` = "facetfulltext" → *string*
- `Examine.FieldDefinitionTypes.FacetFullTextSortable` = "facetfulltextsortable" → *string*
- `Examine.FieldDefinitionTypes.FacetInteger` = "facetint" → *string*
- `Examine.FieldDefinitionTypes.FacetLong` = "facetlong" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDateDay` = "facettaxonomydate.day" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDateHour` = "facettaxonomydate.hour" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDateMinute` = "facettaxonomydate.minute" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDateMonth` = "facettaxonomydate.month" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDateTime` = "facettaxonomydatetime" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDateYear` = "facettaxonomydate.year" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyDouble` = "facettaxonomydouble" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyFloat` = "facettaxonomyfloat" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyFullText` = "facettaxonomyfulltext" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyFullTextSortable` = "facettaxonomyfulltextsortable" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyInteger` = "facettaxonomyint" → *string*
- `Examine.FieldDefinitionTypes.FacetTaxonomyLong` = "facettaxonomylong" → *string*

##### Enum (2)

- `Examine.Search.Examineness.Default = 100` → *Examine.Search.Examineness*
- `Examine.Search.Examineness.Phrase = 7` → *Examine.Search.Examineness*

##### Constructor (6)

- `Examine.Search.DoubleRange.DoubleRange()`
- `Examine.Search.DoubleRange.DoubleRange(string label, double min, bool minInclusive, double max, bool maxInclusive)`
- `Examine.Search.FloatRange.FloatRange()`
- `Examine.Search.FloatRange.FloatRange(string label, float min, bool minInclusive, float max, bool maxInclusive)`
- `Examine.Search.Int64Range.Int64Range()`
- `Examine.Search.Int64Range.Int64Range(string label, long min, bool minInclusive, long max, bool maxInclusive)`

##### Property (21)

- `Examine.Search.DoubleRange.Label.get` → *string*
- `Examine.Search.DoubleRange.Max.get` → *double*
- `Examine.Search.DoubleRange.MaxInclusive.get` → *bool*
- `Examine.Search.DoubleRange.Min.get` → *double*
- `Examine.Search.DoubleRange.MinInclusive.get` → *bool*
- `Examine.Search.FloatRange.Label.get` → *string*
- `Examine.Search.FloatRange.Max.get` → *float*
- `Examine.Search.FloatRange.MaxInclusive.get` → *bool*
- `Examine.Search.FloatRange.Min.get` → *float*
- `Examine.Search.FloatRange.MinInclusive.get` → *bool*
- `Examine.Search.IExamineValueBoosted.Boost.get` → *float*
- `Examine.Search.IFacetLabel.Components.get` → *string[]*
- `Examine.Search.IFacetLabel.Length.get` → *int*
- `Examine.Search.IFacetResults.Facets.get` → *System.Collections.Generic.IReadOnlyDictionary<string, Examine.Search.IFacetResult>*
- `Examine.Search.IFacetValue.Label.get` → *string*
- `Examine.Search.IFacetValue.Value.get` → *float*
- `Examine.Search.Int64Range.Label.get` → *string*
- `Examine.Search.Int64Range.Max.get` → *long*
- `Examine.Search.Int64Range.MaxInclusive.get` → *bool*
- `Examine.Search.Int64Range.Min.get` → *long*
- `Examine.Search.Int64Range.MinInclusive.get` → *bool*

##### Static (5)

- `Examine.Search.ExamineValue.Create(Examine.Search.Examineness vagueness, string value, float level)` → *Examine.Search.IExamineValue*
- `Examine.Search.ExamineValue.Create(Examine.Search.Examineness vagueness, string value)` → *Examine.Search.IExamineValue*
- `Examine.Search.ExamineValueExtensions.WithBoost(this Examine.Search.IExamineValue examineValue, float boost)` → *Examine.Search.IExamineValue*
- `Examine.Search.OrderingExtensions.WithFacets(this Examine.Search.IOrdering ordering, System.Action<Examine.Search.IFacetOperations> facets)` → *Examine.Search.IQueryExecutor*
- `Examine.SearchExtensions.Phrase(this string s)` → *Examine.Search.IExamineValue*

##### Member (9)

- `Examine.Search.IFaceting.WithFacets(System.Action<Examine.Search.IFacetOperations> facets)` → *Examine.Search.IQueryExecutor*
- `Examine.Search.IFacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel*
- `Examine.Search.IFacetOperations.FacetDoubleRange(string field, params Examine.Search.DoubleRange[] doubleRanges)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetOperations.FacetFloatRange(string field, params Examine.Search.FloatRange[] floatRanges)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetOperations.FacetLongRange(string field, params Examine.Search.Int64Range[] longRanges)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetOperations.FacetString(string field, System.Action<Examine.Search.IFacetQueryField> facetConfiguration = null, params string[] values)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetQueryField.MaxCount(int count)` → *Examine.Search.IFacetQueryField*
- `Examine.Search.IFacetResult.Facet(string label)` → *Examine.Search.IFacetValue*
- `Examine.Search.IFacetResult.TryGetFacet(string label, out Examine.Search.IFacetValue facetValue)` → *bool*


### Examine.Host

| Kind | Added | Modified | Breaking Additions | Removed |
|---|---:|---:|---:|---:|
| Constructor | 0 | 1 | 0 | 0 |
| Property | 0 | 2 | 0 | 0 |
| Static | 4 | 6 | 0 | 0 |
| Member | 0 | 1 | 0 | 0 |
| **Total** | **4** | **10** | **0** | **0** |

#### ⚠️ Modified APIs (BREAKING) (10)

_Signature changes → callers and/or derived classes must be updated._

##### Constructor (1)

- `Examine.AspNetCoreApplicationIdentifier.AspNetCoreApplicationIdentifier(System.IServiceProvider services)`
  **Changed to:** `Examine.AspNetCoreApplicationIdentifier.AspNetCoreApplicationIdentifier(System.IServiceProvider! services)`

##### Property (2)

- `Examine.CurrentEnvironmentApplicationRoot.ApplicationRoot.get` → *System.IO.DirectoryInfo*
  **Changed to:** `Examine.CurrentEnvironmentApplicationRoot.ApplicationRoot.get` → *System.IO.DirectoryInfo!*
- `Examine.IApplicationRoot.ApplicationRoot.get` → *System.IO.DirectoryInfo*
  **Changed to:** `Examine.IApplicationRoot.ApplicationRoot.get` → *System.IO.DirectoryInfo!*

##### Static (6)

- `Examine.ServicesCollectionExtensions.AddExamine(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.IO.DirectoryInfo appRootDirectory = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection*
  **Changed to:** `Examine.ServicesCollectionExtensions.AddExamine(this Microsoft.Extensions.DependencyInjection.IServiceCollection! services, System.IO.DirectoryInfo? appRootDirectory = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection, string name, Examine.FieldDefinitionCollection fieldDefinitions = null, Lucene.Net.Analysis.Analyzer analyzer = null, Examine.IValueSetValidator validator = null, System.Collections.Generic.IReadOnlyDictionary<string, Examine.Lucene.IFieldValueTypeFactory> indexValueTypesFactory = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection*
  **Changed to:** `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, Examine.FieldDefinitionCollection? fieldDefinitions, Lucene.Net.Analysis.Analyzer? analyzer, Examine.IValueSetValidator? validator, System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Lucene.IFieldValueTypeFactory!>? indexValueTypesFactory)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection, string name, Examine.FieldDefinitionCollection fieldDefinitions = null, Lucene.Net.Analysis.Analyzer analyzer = null, Examine.IValueSetValidator validator = null, System.Collections.Generic.IReadOnlyDictionary<string, Examine.Lucene.IFieldValueTypeFactory> indexValueTypesFactory = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection*
  **Changed to:** `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, Examine.FieldDefinitionCollection? fieldDefinitions, Lucene.Net.Analysis.Analyzer? analyzer, Examine.IValueSetValidator? validator, System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Lucene.IFieldValueTypeFactory!>? indexValueTypesFactory)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex>(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection, string name, Examine.FieldDefinitionCollection fieldDefinitions = null, Lucene.Net.Analysis.Analyzer analyzer = null, Examine.IValueSetValidator validator = null, System.Collections.Generic.IReadOnlyDictionary<string, Examine.Lucene.IFieldValueTypeFactory> indexValueTypesFactory = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection*
  **Changed to:** `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, Examine.FieldDefinitionCollection? fieldDefinitions, Lucene.Net.Analysis.Analyzer? analyzer, Examine.IValueSetValidator? validator, System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Lucene.IFieldValueTypeFactory!>? indexValueTypesFactory)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneMultiSearcher(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection, string name, string[] indexNames, Lucene.Net.Analysis.Analyzer analyzer = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection*
  **Changed to:** `Examine.ServicesCollectionExtensions.AddExamineLuceneMultiSearcher(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, string![]! indexNames, Lucene.Net.Analysis.Analyzer? analyzer)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineSearcher<TSearcher>(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection, string name, System.Func<System.IServiceProvider, System.Collections.Generic.IList<object>> parameterFactory)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection*
  **Changed to:** `Examine.ServicesCollectionExtensions.AddExamineSearcher<TSearcher>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Func<System.IServiceProvider!, System.Collections.Generic.IList<object!>!>! parameterFactory)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*

##### Member (1)

- `Examine.AspNetCoreApplicationIdentifier.GetApplicationUniqueIdentifier()` → *string*
  **Changed to:** `Examine.AspNetCoreApplicationIdentifier.GetApplicationUniqueIdentifier()` → *string!*


#### ✅ Added APIs (Non-Breaking) (4)

##### Static (4)

- `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Action<Examine.Lucene.LuceneDirectoryIndexOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Action<Examine.Lucene.LuceneDirectoryIndexOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Action<Examine.Lucene.LuceneDirectoryIndexOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- `Examine.ServicesCollectionExtensions.AddExamineLuceneMultiSearcher(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, string![]! indexNames, System.Action<Examine.Lucene.LuceneMultiSearcherOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*


### Examine.Lucene

| Kind | Added | Modified | Breaking Additions | Removed |
|---|---:|---:|---:|---:|
| Type | 22 | 0 | 0 | 1 |
| Constructor | 26 | 5 | 0 | 8 |
| Property | 58 | 2 | 0 | 1 |
| Abstract | 0 | 0 | 1 | 0 |
| Virtual | 13 | 0 | 0 | 1 |
| Override | 10 | 2 | 0 | 1 |
| Static | 2 | 0 | 0 | 0 |
| Member | 30 | 0 | 0 | 1 |
| **Total** | **161** | **9** | **1** | **13** |

#### ⚠️ Removed APIs (BREAKING) (13)

##### Type (1)

- `Examine.Lucene.Providers.LuceneSearcher`

##### Constructor (8)

- `Examine.Lucene.Directories.SyncedFileSystemDirectoryFactory.SyncedFileSystemDirectoryFactory(System.IO.DirectoryInfo localDir, System.IO.DirectoryInfo mainDir, Examine.Lucene.Directories.ILockFactory lockFactory, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, bool tryFixMainIndexIfCorrupt)`
- `Examine.Lucene.Directories.SyncedFileSystemDirectoryFactory.SyncedFileSystemDirectoryFactory(System.IO.DirectoryInfo localDir, System.IO.DirectoryInfo mainDir, Examine.Lucene.Directories.ILockFactory lockFactory, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)`
- `Examine.Lucene.Directories.TempEnvFileSystemDirectoryFactory.TempEnvFileSystemDirectoryFactory(Examine.Lucene.Directories.IApplicationIdentifier applicationIdentifier, Examine.Lucene.Directories.ILockFactory lockFactory)`
- `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator> replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient> clientLogger, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory sourceDirectory, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
- `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
- `Examine.Lucene.Providers.LuceneSearcher.LuceneSearcher(string name, Lucene.Net.Search.SearcherManager searcherManager, Lucene.Net.Analysis.Analyzer analyzer, Examine.Lucene.FieldValueTypeCollection fieldValueTypeCollection, bool isNrt)`
- `Examine.Lucene.Providers.LuceneSearcher.LuceneSearcher(string name, Lucene.Net.Search.SearcherManager searcherManager, Lucene.Net.Analysis.Analyzer analyzer, Examine.Lucene.FieldValueTypeCollection fieldValueTypeCollection)`
- `Examine.Lucene.Search.SearchContext.SearchContext(Lucene.Net.Search.SearcherManager searcherManager, Examine.Lucene.FieldValueTypeCollection fieldValueTypeCollection)`

##### Property (1)

- `Examine.Lucene.Providers.LuceneIndex.IsCancellationRequested.get` → *bool*

##### Virtual (1)

- `Examine.Lucene.Providers.LuceneSearcher.Dispose(bool disposing)` → *void*

##### Override (1)

- `Examine.Lucene.Providers.LuceneSearcher.GetSearchContext()` → *Examine.Lucene.Search.ISearchContext*

##### Member (1)

- `Examine.Lucene.Providers.LuceneSearcher.Dispose()` → *void*


#### ⚠️ Modified APIs (BREAKING) (9)

_Signature changes → callers and/or derived classes must be updated._

##### Constructor (5)

- `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator> replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient> clientLogger, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
  **Changed to:** `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator!>! replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient!>! clientLogger, Examine.Lucene.Providers.LuceneIndex! sourceIndex, Lucene.Net.Store.Directory! sourceDirectory, Lucene.Net.Store.Directory! destinationDirectory, Lucene.Net.Store.Directory? destinationTaxonomyDirectory, System.IO.DirectoryInfo! tempStorage)`
- `Examine.Lucene.Providers.BaseLuceneSearcher.BaseLuceneSearcher(string name, Lucene.Net.Analysis.Analyzer analyzer)`
  **Changed to:** `Examine.Lucene.Providers.BaseLuceneSearcher.BaseLuceneSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneSearcherOptions!>! options)`
- `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string name, System.Collections.Generic.IEnumerable<Examine.IIndex> indexes, Lucene.Net.Analysis.Analyzer analyzer = null)`
  **Changed to:** `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneMultiSearcherOptions!>! options, System.Collections.Generic.IEnumerable<Examine.IIndex!>! indexes)`
- `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string name, System.Lazy<System.Collections.Generic.IEnumerable<Examine.ISearcher>> searchers, Lucene.Net.Analysis.Analyzer analyzer = null)`
  **Changed to:** `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneMultiSearcherOptions!>! options, System.Lazy<System.Collections.Generic.IEnumerable<Examine.ISearcher!>!>! searchers)`
- `Examine.Lucene.Search.LuceneSearchResults.LuceneSearchResults(System.Collections.Generic.IReadOnlyCollection<Examine.ISearchResult> results, int totalItemCount, float maxScore, Examine.Lucene.Search.SearchAfterOptions searchAfterOptions)`
  **Changed to:** `Examine.Lucene.Search.LuceneSearchResults.LuceneSearchResults(System.Collections.Generic.IReadOnlyCollection<Examine.ISearchResult!>! results, int totalItemCount, System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Search.IFacetResult!>! facets, float maxScore, Examine.Lucene.Search.SearchAfterOptions? searchAfterOptions)`

##### Property (2)

- `Examine.Lucene.Providers.MultiIndexSearcher.Searchers.get` → *System.Collections.Generic.IEnumerable<Examine.Lucene.Providers.LuceneSearcher>*
  **Changed to:** `Examine.Lucene.Providers.MultiIndexSearcher.Searchers.get` → *System.Collections.Generic.IEnumerable<Examine.Lucene.Providers.BaseLuceneSearcher!>!*
- `Examine.Lucene.Search.SearchAfterOptions.ShardIndex.get` → *int?*
  **Changed to:** `Examine.Lucene.Search.SearchAfterOptions.ShardIndex.get` → *int*

##### Override (2)

- `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory*
  **Changed to:** `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- `Examine.Lucene.Directories.GenericDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory*
  **Changed to:** `Examine.Lucene.Directories.GenericDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*


#### ⚠️ Breaking Additions (1)

_New abstract or interface members on existing types → all derived classes / implementors must be updated._

##### Abstract (1)

- `Examine.Lucene.Providers.BaseLuceneSearcher.Dispose()` → *void*


#### ✅ Added APIs (Non-Breaking) (161)

##### Type (22)

- `Examine.Lucene.Directories.ITaxonomyDirectoryFactory`
- `Examine.Lucene.FacetExtensions`
- `Examine.Lucene.Indexing.IIndexFacetValueType`
- `Examine.Lucene.LuceneMultiSearcherOptions`
- `Examine.Lucene.LuceneSearcherOptions`
- `Examine.Lucene.Providers.IIndexCommitter`
- `Examine.Lucene.Providers.ILuceneTaxonomySearcher`
- `Examine.Lucene.Search.FacetDoubleField`
- `Examine.Lucene.Search.FacetFloatField`
- `Examine.Lucene.Search.FacetFullTextField`
- `Examine.Lucene.Search.FacetLongField`
- `Examine.Lucene.Search.FacetQueryField`
- `Examine.Lucene.Search.IFacetExtractionContext`
- `Examine.Lucene.Search.IFacetField`
- `Examine.Lucene.Search.ITaxonomySearchContext`
- `Examine.Lucene.Search.ITaxonomySearcherReference`
- `Examine.Lucene.Search.LuceneFacetExtractionContext`
- `Examine.Lucene.Search.LuceneFacetLabel`
- `Examine.Lucene.Search.LuceneFacetOperation`
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions`
- `Examine.Lucene.Search.TaxonomySearchContext`
- `Examine.Lucene.Search.TaxonomySearcherReference`

##### Constructor (26)

- `Examine.Lucene.Directories.GenericDirectoryFactory.GenericDirectoryFactory(System.Func<string!, Lucene.Net.Store.Directory!>! factory, System.Func<string!, Lucene.Net.Store.Directory?>? taxonomyDirectoryFactory)`
- `Examine.Lucene.Indexing.DateTimeType.DateTimeType(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, Lucene.Net.Documents.DateResolution resolution, bool store)`
- `Examine.Lucene.Indexing.DoubleType.DoubleType(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- `Examine.Lucene.Indexing.FullTextType.FullTextType(string! fieldName, bool isFacetable, bool taxonomyIndex, bool sortable, Microsoft.Extensions.Logging.ILoggerFactory! logger, Lucene.Net.Analysis.Analyzer! analyzer)`
- `Examine.Lucene.Indexing.Int32Type.Int32Type(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- `Examine.Lucene.Indexing.Int64Type.Int64Type(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- `Examine.Lucene.Indexing.SingleType.SingleType(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- `Examine.Lucene.LuceneMultiSearcherOptions.LuceneMultiSearcherOptions()`
- `Examine.Lucene.LuceneSearcherOptions.LuceneSearcherOptions()`
- `Examine.Lucene.Search.FacetDoubleField.FacetDoubleField()`
- `Examine.Lucene.Search.FacetDoubleField.FacetDoubleField(string! field, Examine.Search.DoubleRange[]! doubleRanges, string! facetField, bool isTaxonomyIndexed = false)`
- `Examine.Lucene.Search.FacetFloatField.FacetFloatField()`
- `Examine.Lucene.Search.FacetFloatField.FacetFloatField(string! field, Examine.Search.FloatRange[]! floatRanges, string! facetField, bool isTaxonomyIndexed = false)`
- `Examine.Lucene.Search.FacetFullTextField.FacetFullTextField(string! field, string![]! values, string! facetField, int maxCount = 2147483647, bool isTaxonomyIndexed = false)`
- `Examine.Lucene.Search.FacetLongField.FacetLongField()`
- `Examine.Lucene.Search.FacetLongField.FacetLongField(string! field, Examine.Search.Int64Range[]! longRanges, string! facetField, bool isTaxonomyIndexed = false)`
- `Examine.Lucene.Search.FacetQueryField.FacetQueryField(Examine.Lucene.Search.FacetFullTextField! field)`
- `Examine.Lucene.Search.LuceneFacetExtractionContext.LuceneFacetExtractionContext(Lucene.Net.Facet.FacetsCollector! facetsCollector, Examine.Lucene.Search.ISearcherReference! searcherReference, Lucene.Net.Facet.FacetsConfig! facetConfig)`
- `Examine.Lucene.Search.LuceneFacetLabel.LuceneFacetLabel(Lucene.Net.Facet.Taxonomy.FacetLabel! facetLabel)`
- `Examine.Lucene.Search.LuceneFacetOperation.LuceneFacetOperation(Examine.Lucene.Search.LuceneSearchQuery! search)`
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.LuceneFacetSamplingQueryOptions(int sampleSize, long seed)`
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.LuceneFacetSamplingQueryOptions(int sampleSize)`
- `Examine.Lucene.Search.LuceneQueryOptions.LuceneQueryOptions(int skip, int take = 100, Examine.Lucene.Search.SearchAfterOptions? searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false, int skipTakeMaxResults = 10000, bool autoCalculateSkipTakeMaxResults = false, Examine.Lucene.Search.LuceneFacetSamplingQueryOptions? facetSampling = null)`
- `Examine.Lucene.Search.LuceneSearchQuery.LuceneSearchQuery(Examine.Lucene.Search.ISearchContext! searchContext, string? category, Lucene.Net.Analysis.Analyzer! analyzer, Examine.Lucene.Search.LuceneSearchOptions! searchOptions, Examine.Search.BooleanOperation occurance, Lucene.Net.Facet.FacetsConfig! facetsConfig)`
- `Examine.Lucene.Search.TaxonomySearchContext.TaxonomySearchContext(Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager! searcherManager, Examine.Lucene.FieldValueTypeCollection! fieldValueTypeCollection, bool isNrt)`
- `Examine.Lucene.Search.TaxonomySearcherReference.TaxonomySearcherReference(Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager! searcherManager)`

##### Property (58)

- `Examine.Lucene.ExamineReplicator.ConsecutiveReplicationFailures.get` → *int*
- `Examine.Lucene.ExamineReplicator.IsReplicationHealthy.get` → *bool*
- `Examine.Lucene.ExamineReplicator.MaxConsecutiveReplicationFailures.get` → *int*
- `Examine.Lucene.Indexing.DateTimeType.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.Indexing.DoubleType.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.Indexing.FullTextType.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.Indexing.IIndexFacetValueType.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.Indexing.Int32Type.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.Indexing.Int64Type.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.Indexing.SingleType.IsTaxonomyFaceted.get` → *bool*
- `Examine.Lucene.LuceneIndexOptions.FacetsConfig.get` → *Lucene.Net.Facet.FacetsConfig!*
- `Examine.Lucene.LuceneIndexOptions.FacetsConfig.set` → *void*
- `Examine.Lucene.LuceneIndexOptions.UseTaxonomyIndex.get` → *bool*
- `Examine.Lucene.LuceneIndexOptions.UseTaxonomyIndex.set` → *void*
- `Examine.Lucene.LuceneMultiSearcherOptions.IndexNames.get` → *string![]!*
- `Examine.Lucene.LuceneMultiSearcherOptions.IndexNames.set` → *void*
- `Examine.Lucene.LuceneSearcherOptions.Analyzer.get` → *Lucene.Net.Analysis.Analyzer?*
- `Examine.Lucene.LuceneSearcherOptions.Analyzer.set` → *void*
- `Examine.Lucene.LuceneSearcherOptions.FacetConfiguration.get` → *Lucene.Net.Facet.FacetsConfig?*
- `Examine.Lucene.LuceneSearcherOptions.FacetConfiguration.set` → *void*
- `Examine.Lucene.Providers.ILuceneTaxonomySearcher.CategoryCount.get` → *int*
- `Examine.Lucene.Providers.LuceneIndex.IsTaxonomyEnabled.get` → *bool*
- `Examine.Lucene.Search.FacetDoubleField.DoubleRanges.get` → *Examine.Search.DoubleRange[]!*
- `Examine.Lucene.Search.FacetDoubleField.FacetField.get` → *string!*
- `Examine.Lucene.Search.FacetDoubleField.Field.get` → *string!*
- `Examine.Lucene.Search.FacetDoubleField.IsTaxonomyIndexed.get` → *bool*
- `Examine.Lucene.Search.FacetFloatField.FacetField.get` → *string!*
- `Examine.Lucene.Search.FacetFloatField.Field.get` → *string!*
- `Examine.Lucene.Search.FacetFloatField.FloatRanges.get` → *Examine.Search.FloatRange[]!*
- `Examine.Lucene.Search.FacetFloatField.IsTaxonomyIndexed.get` → *bool*
- `Examine.Lucene.Search.FacetFullTextField.FacetField.get` → *string!*
- `Examine.Lucene.Search.FacetFullTextField.Field.get` → *string!*
- `Examine.Lucene.Search.FacetFullTextField.IsTaxonomyIndexed.get` → *bool*
- `Examine.Lucene.Search.FacetFullTextField.MaxCount.get` → *int*
- `Examine.Lucene.Search.FacetFullTextField.Values.get` → *string![]!*
- `Examine.Lucene.Search.FacetLongField.FacetField.get` → *string!*
- `Examine.Lucene.Search.FacetLongField.Field.get` → *string!*
- `Examine.Lucene.Search.FacetLongField.IsTaxonomyIndexed.get` → *bool*
- `Examine.Lucene.Search.FacetLongField.LongRanges.get` → *Examine.Search.Int64Range[]!*
- `Examine.Lucene.Search.IFacetExtractionContext.FacetConfig.get` → *Lucene.Net.Facet.FacetsConfig!*
- `Examine.Lucene.Search.IFacetExtractionContext.FacetsCollector.get` → *Lucene.Net.Facet.FacetsCollector!*
- `Examine.Lucene.Search.IFacetExtractionContext.SearcherReference.get` → *Examine.Lucene.Search.ISearcherReference!*
- `Examine.Lucene.Search.IFacetField.FacetField.get` → *string!*
- `Examine.Lucene.Search.IFacetField.Field.get` → *string!*
- `Examine.Lucene.Search.IFacetField.IsTaxonomyIndexed.get` → *bool*
- `Examine.Lucene.Search.ITaxonomySearcherReference.TaxonomyReader.get` → *Lucene.Net.Facet.Taxonomy.Directory.DirectoryTaxonomyReader!*
- `Examine.Lucene.Search.LuceneFacetExtractionContext.FacetConfig.get` → *Lucene.Net.Facet.FacetsConfig!*
- `Examine.Lucene.Search.LuceneFacetExtractionContext.FacetsCollector.get` → *Lucene.Net.Facet.FacetsCollector!*
- `Examine.Lucene.Search.LuceneFacetExtractionContext.SearcherReference.get` → *Examine.Lucene.Search.ISearcherReference!*
- `Examine.Lucene.Search.LuceneFacetLabel.Components.get` → *string![]!*
- `Examine.Lucene.Search.LuceneFacetLabel.Length.get` → *int*
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.SampleSize.get` → *int*
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.Seed.get` → *long*
- `Examine.Lucene.Search.LuceneQueryOptions.FacetRandomSampling.get` → *Examine.Lucene.Search.LuceneFacetSamplingQueryOptions?*
- `Examine.Lucene.Search.LuceneSearchResults.Facets.get` → *System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Search.IFacetResult!>!*
- `Examine.Lucene.Search.TaxonomySearchContext.SearchableFields.get` → *string![]!*
- `Examine.Lucene.Search.TaxonomySearcherReference.IndexSearcher.get` → *Lucene.Net.Search.IndexSearcher!*
- `Examine.Lucene.Search.TaxonomySearcherReference.TaxonomyReader.get` → *Lucene.Net.Facet.Taxonomy.Directory.DirectoryTaxonomyReader!*

##### Virtual (13)

- `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory?*
- `Examine.Lucene.Indexing.DateTimeType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.DoubleType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.FullTextType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.Int32Type.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.Int64Type.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.SingleType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Providers.LuceneIndex.TaxonomySearcher.get` → *Examine.Lucene.Providers.ILuceneTaxonomySearcher?*
- `Examine.Lucene.Search.LuceneBooleanOperationBase.WithFacets(System.Action<Examine.Search.IFacetOperations!>! facets)` → *Examine.Search.IQueryExecutor!*
- `Examine.Lucene.Search.LuceneFacetExtractionContext.GetFacetCounts(string! facetIndexFieldName, bool isTaxonomyIndexed)` → *Lucene.Net.Facet.Facets!*
- `Examine.Lucene.Search.LuceneSearchQueryBase.GetFieldInternalQuery(string! fieldName, Examine.Search.IExamineValue! fieldValue, bool useQueryParser)` → *Lucene.Net.Search.Query?*
- `Examine.Lucene.Search.LuceneSearchQueryBase.GetFieldInternalQuery(string! fieldName, Examine.Search.IExamineValue! fieldValue)` → *Lucene.Net.Search.Query?*
- `Examine.Lucene.Search.TaxonomySearcherReference.Dispose(bool disposing)` → *void*

##### Override (10)

- `Examine.Lucene.Directories.SyncedFileSystemDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory?*
- `Examine.Lucene.Indexing.DateTimeType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- `Examine.Lucene.Indexing.DoubleType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- `Examine.Lucene.Indexing.FullTextType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- `Examine.Lucene.Indexing.Int32Type.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- `Examine.Lucene.Indexing.Int64Type.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- `Examine.Lucene.Indexing.SingleType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- `Examine.Lucene.Providers.MultiIndexSearcher.Dispose()` → *void*
- `Examine.Lucene.Search.LuceneBooleanOperation.WithFacets(System.Action<Examine.Search.IFacetOperations!>! facets)` → *Examine.Search.IQueryExecutor!*
- `Examine.Lucene.Search.LuceneFacetOperation.ToString()` → *string!*

##### Static (2)

- `Examine.Lucene.FacetExtensions.GetFacet(this Examine.ISearchResults! searchResults, string! field)` → *Examine.Search.IFacetResult?*
- `Examine.Lucene.FacetExtensions.GetFacets(this Examine.ISearchResults! searchResults)` → *System.Collections.Generic.IEnumerable<Examine.Search.IFacetResult!>!*

##### Member (30)

- `Examine.Lucene.Directories.GenericDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory?*
- `Examine.Lucene.Directories.ITaxonomyDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory?*
- `Examine.Lucene.Indexing.IIndexFacetValueType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Providers.IIndexCommitter.CommitError` → *System.EventHandler<Examine.IndexingErrorEventArgs!>!*
- `Examine.Lucene.Providers.IIndexCommitter.CommitNow()` → *void*
- `Examine.Lucene.Providers.IIndexCommitter.Committed` → *System.EventHandler?*
- `Examine.Lucene.Providers.IIndexCommitter.ScheduleCommit()` → *void*
- `Examine.Lucene.Providers.ILuceneTaxonomySearcher.GetOrdinal(string! dim, string![]! path)` → *int*
- `Examine.Lucene.Providers.ILuceneTaxonomySearcher.GetPath(int ordinal)` → *Examine.Search.IFacetLabel!*
- `Examine.Lucene.Providers.LuceneIndex.GetLuceneTaxonomyDirectory()` → *Lucene.Net.Store.Directory?*
- `Examine.Lucene.Search.CustomMultiFieldQueryParser.GetPhraseQueryInternal(string! field, string! queryText)` → *Lucene.Net.Search.Query!*
- `Examine.Lucene.Search.FacetDoubleField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Search.FacetFloatField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Search.FacetFullTextField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Search.FacetLongField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Search.FacetQueryField.MaxCount(int count)` → *Examine.Search.IFacetQueryField!*
- `Examine.Lucene.Search.IFacetExtractionContext.GetFacetCounts(string! facetIndexFieldName, bool isTaxonomyIndexed)` → *Lucene.Net.Facet.Facets!*
- `Examine.Lucene.Search.IFacetField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Search.ITaxonomySearchContext.GetTaxonomyAndSearcher()` → *Examine.Lucene.Search.ITaxonomySearcherReference!*
- `Examine.Lucene.Search.LuceneFacetLabel.CompareTo(Examine.Search.IFacetLabel? other)` → *int*
- `Examine.Lucene.Search.LuceneFacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel!*
- `Examine.Lucene.Search.LuceneFacetOperation.Execute(Examine.Search.QueryOptions? options = null)` → *Examine.ISearchResults!*
- `Examine.Lucene.Search.LuceneFacetOperation.FacetDoubleRange(string! field, params Examine.Search.DoubleRange[]! doubleRanges)` → *Examine.Search.IFacetOperations!*
- `Examine.Lucene.Search.LuceneFacetOperation.FacetFloatRange(string! field, params Examine.Search.FloatRange[]! floatRanges)` → *Examine.Search.IFacetOperations!*
- `Examine.Lucene.Search.LuceneFacetOperation.FacetLongRange(string! field, params Examine.Search.Int64Range[]! longRanges)` → *Examine.Search.IFacetOperations!*
- `Examine.Lucene.Search.LuceneFacetOperation.FacetString(string! field, System.Action<Examine.Search.IFacetQueryField!>? facetConfiguration = null, params string![]! values)` → *Examine.Search.IFacetOperations!*
- `Examine.Lucene.Search.TaxonomySearchContext.GetFieldValueType(string! fieldName)` → *Examine.Lucene.Indexing.IIndexFieldValueType!*
- `Examine.Lucene.Search.TaxonomySearchContext.GetSearcher()` → *Examine.Lucene.Search.ISearcherReference!*
- `Examine.Lucene.Search.TaxonomySearchContext.GetTaxonomyAndSearcher()` → *Examine.Lucene.Search.ITaxonomySearcherReference!*
- `Examine.Lucene.Search.TaxonomySearcherReference.Dispose()` → *void*


## Summary

### ✅ Additions (Non-Breaking)
246 new API(s) have been added. These are **safe changes** that do not break existing code.

### ⚠️ Breaking Changes
19 API(s) have **changed signatures**.

14 API(s) have been **removed**.

1 **abstract or interface member(s)** have been added to existing types (all derived classes / implementors must be updated).

These are **BREAKING CHANGES** that will require:
- Major version bump (e.g., 3.x → 4.0)
- Migration guide for consumers
- Release notes highlighting the breaking changes

## Next Steps

Before releasing:

1. Review all new APIs for:
   - Naming consistency
   - XML documentation completeness
   - Design patterns alignment
   - Backward compatibility

2. Update release notes with these API changes

3. Run the build to ensure no analyzer warnings (RS0016, RS0017)

4. After release, run `.\build\Merge-PublicApiFiles.ps1` to move Unshipped → Shipped

---

*Generated by Get-PublicApiReport.ps1*