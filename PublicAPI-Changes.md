# Public API Changes Report

Generated: 2026-03-06 09:42:08

## Summary

- **Projects with changes:** 3
- **Total new APIs (safe):** 260
- **Total modified APIs:** 11 ⚠️ signature changes
- **Total breaking additions:** 2 ⚠️ abstract/interface members on existing types
- **Total removed APIs:** 15 ⚠️ **BREAKING**

## Project Breakdown

### Examine.Core

| Kind | Added | Modified | Breaking Additions | Removed |
|---|---:|---:|---:|---:|
| Type | 16 | 0 | 0 | 0 |
| Constant | 25 | 0 | 0 | 0 |
| Enum | 2 | 0 | 0 | 0 |
| Constructor | 12 | 0 | 0 | 1 |
| Property | 25 | 0 | 0 | 0 |
| Static | 5 | 0 | 0 | 0 |
| Member | 15 | 0 | 0 | 0 |
| **Total** | **100** | **0** | **0** | **1** |

#### ⚠️ Removed APIs (BREAKING) (1)

##### Constructor (1)

- `Examine.Search.ExamineValue.ExamineValue()`


#### ✅ Added APIs (Non-Breaking) (100)

##### Type (16)

- `Examine.Search.DoubleRange`
- `Examine.Search.ExamineValueExtensions`
- `Examine.Search.FacetLabel`
- `Examine.Search.FacetResult`
- `Examine.Search.FacetValue`
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

##### Constructor (12)

- `Examine.Search.DoubleRange.DoubleRange()`
- `Examine.Search.DoubleRange.DoubleRange(string label, double min, bool minInclusive, double max, bool maxInclusive)`
- `Examine.Search.FacetLabel.FacetLabel()`
- `Examine.Search.FacetLabel.FacetLabel(string dimension, string[] components)`
- `Examine.Search.FacetLabel.FacetLabel(string[] components)`
- `Examine.Search.FacetResult.FacetResult(System.Collections.Generic.IEnumerable<Examine.Search.IFacetValue> values)`
- `Examine.Search.FacetValue.FacetValue()`
- `Examine.Search.FacetValue.FacetValue(string label, float value)`
- `Examine.Search.FloatRange.FloatRange()`
- `Examine.Search.FloatRange.FloatRange(string label, float min, bool minInclusive, float max, bool maxInclusive)`
- `Examine.Search.Int64Range.Int64Range()`
- `Examine.Search.Int64Range.Int64Range(string label, long min, bool minInclusive, long max, bool maxInclusive)`

##### Property (25)

- `Examine.Search.DoubleRange.Label.get` → *string*
- `Examine.Search.DoubleRange.Max.get` → *double*
- `Examine.Search.DoubleRange.MaxInclusive.get` → *bool*
- `Examine.Search.DoubleRange.Min.get` → *double*
- `Examine.Search.DoubleRange.MinInclusive.get` → *bool*
- `Examine.Search.FacetLabel.Components.get` → *string[]*
- `Examine.Search.FacetLabel.Length.get` → *int*
- `Examine.Search.FacetValue.Label.get` → *string*
- `Examine.Search.FacetValue.Value.get` → *float*
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

##### Member (15)

- `Examine.Search.FacetLabel.CompareTo(Examine.Search.IFacetLabel other)` → *int*
- `Examine.Search.FacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel*
- `Examine.Search.FacetResult.Facet(string label)` → *Examine.Search.IFacetValue*
- `Examine.Search.FacetResult.GetEnumerator()` → *System.Collections.Generic.IEnumerator<Examine.Search.IFacetValue>*
- `Examine.Search.FacetResult.TryGetFacet(string label, out Examine.Search.IFacetValue facetValue)` → *bool*
- `Examine.Search.IFaceting.WithFacets(System.Action<Examine.Search.IFacetOperations> facets)` → *Examine.Search.IQueryExecutor*
- `Examine.Search.IFacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel*
- `Examine.Search.IFacetOperations.FacetDoubleRange(string field, params Examine.Search.DoubleRange[] doubleRanges)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetOperations.FacetFloatRange(string field, params Examine.Search.FloatRange[] floatRanges)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetOperations.FacetLongRange(string field, params Examine.Search.Int64Range[] longRanges)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetOperations.FacetString(string field, System.Action<Examine.Search.IFacetQueryField> facetConfiguration = null, params string[] values)` → *Examine.Search.IFacetOperations*
- `Examine.Search.IFacetQueryField.MaxCount(int count)` → *Examine.Search.IFacetQueryField*
- `Examine.Search.IFacetQueryField.SetPath(params string[] path)` → *Examine.Search.IFacetQueryField*
- `Examine.Search.IFacetResult.Facet(string label)` → *Examine.Search.IFacetValue*
- `Examine.Search.IFacetResult.TryGetFacet(string label, out Examine.Search.IFacetValue facetValue)` → *bool*


### Examine.Host

| Kind | Added | Modified | Breaking Additions | Removed |
|---|---:|---:|---:|---:|
| Static | 4 | 0 | 0 | 0 |
| **Total** | **4** | **0** | **0** | **0** |

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
| Constructor | 25 | 6 | 0 | 9 |
| Property | 56 | 2 | 0 | 1 |
| Abstract | 0 | 0 | 2 | 0 |
| Virtual | 9 | 1 | 0 | 1 |
| Override | 9 | 2 | 0 | 1 |
| Static | 2 | 0 | 0 | 0 |
| Member | 33 | 0 | 0 | 1 |
| **Total** | **156** | **11** | **2** | **14** |

#### ⚠️ Removed APIs (BREAKING) (14)

##### Type (1)

- `Examine.Lucene.Providers.LuceneSearcher`

##### Constructor (9)

- `Examine.Lucene.Directories.FileSystemDirectoryFactory.FileSystemDirectoryFactory(System.IO.DirectoryInfo baseDir, Examine.Lucene.Directories.ILockFactory lockFactory)`
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


#### ⚠️ Modified APIs (BREAKING) (11)

_Signature changes → callers and/or derived classes must be updated._

##### Constructor (6)

- `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator> replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient> clientLogger, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
  **Changed to:** `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator!>! replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient!>! clientLogger, Examine.Lucene.Providers.LuceneIndex! sourceIndex, Lucene.Net.Store.Directory! sourceDirectory, Lucene.Net.Store.Directory! destinationDirectory, Lucene.Net.Store.Directory? destinationTaxonomyDirectory, System.IO.DirectoryInfo! tempStorage)`
- `Examine.Lucene.Providers.BaseLuceneSearcher.BaseLuceneSearcher(string name, Lucene.Net.Analysis.Analyzer analyzer)`
  **Changed to:** `Examine.Lucene.Providers.BaseLuceneSearcher.BaseLuceneSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneSearcherOptions!>! options)`
- `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string name, System.Collections.Generic.IEnumerable<Examine.IIndex> indexes, Lucene.Net.Analysis.Analyzer analyzer = null)`
  **Changed to:** `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneMultiSearcherOptions!>! options, System.Collections.Generic.IEnumerable<Examine.IIndex!>! indexes)`
- `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string name, System.Lazy<System.Collections.Generic.IEnumerable<Examine.ISearcher>> searchers, Lucene.Net.Analysis.Analyzer analyzer = null)`
  **Changed to:** `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneMultiSearcherOptions!>! options, System.Lazy<System.Collections.Generic.IEnumerable<Examine.ISearcher!>!>! searchers)`
- `Examine.Lucene.Search.LuceneQueryOptions.LuceneQueryOptions(int skip, int? take = null, Examine.Lucene.Search.SearchAfterOptions searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false, int skipTakeMaxResults = 10000, bool autoCalculateSkipTakeMaxResults = false)`
  **Changed to:** `Examine.Lucene.Search.LuceneQueryOptions.LuceneQueryOptions(int skip, int take = 100, Examine.Lucene.Search.SearchAfterOptions? searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false, int skipTakeMaxResults = 10000, bool autoCalculateSkipTakeMaxResults = false, Examine.Lucene.Search.LuceneFacetSamplingQueryOptions? facetSampling = null)`
- `Examine.Lucene.Search.LuceneSearchResults.LuceneSearchResults(System.Collections.Generic.IReadOnlyCollection<Examine.ISearchResult> results, int totalItemCount, float maxScore, Examine.Lucene.Search.SearchAfterOptions searchAfterOptions)`
  **Changed to:** `Examine.Lucene.Search.LuceneSearchResults.LuceneSearchResults(System.Collections.Generic.IReadOnlyCollection<Examine.ISearchResult!>! results, int totalItemCount, System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Search.IFacetResult!>! facets, float maxScore, Examine.Lucene.Search.SearchAfterOptions? searchAfterOptions)`

##### Property (2)

- `Examine.Lucene.Providers.MultiIndexSearcher.Searchers.get` → *System.Collections.Generic.IEnumerable<Examine.Lucene.Providers.LuceneSearcher>*
  **Changed to:** `Examine.Lucene.Providers.MultiIndexSearcher.Searchers.get` → *System.Collections.Generic.IEnumerable<Examine.Lucene.Providers.BaseLuceneSearcher!>!*
- `Examine.Lucene.Search.SearchAfterOptions.ShardIndex.get` → *int?*
  **Changed to:** `Examine.Lucene.Search.SearchAfterOptions.ShardIndex.get` → *int*

##### Virtual (1)

- `Examine.Lucene.Search.LuceneSearchQueryBase.GetFieldInternalQuery(string fieldName, Examine.Search.IExamineValue fieldValue, bool useQueryParser)` → *Lucene.Net.Search.Query*
  **Changed to:** `Examine.Lucene.Search.LuceneSearchQueryBase.GetFieldInternalQuery(string! fieldName, Examine.Search.IExamineValue! fieldValue)` → *Lucene.Net.Search.Query?*

##### Override (2)

- `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory*
  **Changed to:** `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- `Examine.Lucene.Directories.GenericDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory*
  **Changed to:** `Examine.Lucene.Directories.GenericDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*


#### ⚠️ Breaking Additions (2)

_New abstract or interface members on existing types → all derived classes / implementors must be updated._

##### Abstract (2)

- `Examine.Lucene.Providers.BaseLuceneSearcher.Dispose()` → *void*
- `Examine.Lucene.Search.LuceneBooleanOperationBase.WithFacets(System.Action<Examine.Search.IFacetOperations!>! facets)` → *Examine.Search.IQueryExecutor!*


#### ✅ Added APIs (Non-Breaking) (156)

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

##### Constructor (25)

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
- `Examine.Lucene.Search.FacetFullTextField.FacetFullTextField(string! field, string![]! values, string! facetField, int maxCount = 10, string![]? path = null, bool isTaxonomyIndexed = false)`
- `Examine.Lucene.Search.FacetLongField.FacetLongField()`
- `Examine.Lucene.Search.FacetLongField.FacetLongField(string! field, Examine.Search.Int64Range[]! longRanges, string! facetField, bool isTaxonomyIndexed = false)`
- `Examine.Lucene.Search.FacetQueryField.FacetQueryField(Examine.Lucene.Search.FacetFullTextField! field)`
- `Examine.Lucene.Search.LuceneFacetExtractionContext.LuceneFacetExtractionContext(Lucene.Net.Facet.FacetsCollector! facetsCollector, Examine.Lucene.Search.ISearcherReference! searcherReference, Lucene.Net.Facet.FacetsConfig! facetConfig)`
- `Examine.Lucene.Search.LuceneFacetLabel.LuceneFacetLabel(Lucene.Net.Facet.Taxonomy.FacetLabel! facetLabel)`
- `Examine.Lucene.Search.LuceneFacetOperation.LuceneFacetOperation(Examine.Lucene.Search.LuceneSearchQuery! search)`
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.LuceneFacetSamplingQueryOptions(int sampleSize, long seed)`
- `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.LuceneFacetSamplingQueryOptions(int sampleSize)`
- `Examine.Lucene.Search.LuceneSearchQuery.LuceneSearchQuery(Examine.Lucene.Search.ISearchContext! searchContext, string? category, Lucene.Net.Analysis.Analyzer! analyzer, Examine.Lucene.Search.LuceneSearchOptions! searchOptions, Examine.Search.BooleanOperation occurance, Lucene.Net.Facet.FacetsConfig! facetsConfig)`
- `Examine.Lucene.Search.TaxonomySearchContext.TaxonomySearchContext(Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager! searcherManager, Examine.Lucene.FieldValueTypeCollection! fieldValueTypeCollection, bool isNrt)`
- `Examine.Lucene.Search.TaxonomySearcherReference.TaxonomySearcherReference(Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager! searcherManager)`

##### Property (56)

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
- `Examine.Lucene.Search.FacetFullTextField.Path.get` → *string![]?*
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

##### Virtual (9)

- `Examine.Lucene.Indexing.DateTimeType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.DoubleType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.FullTextType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.Int32Type.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.Int64Type.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Indexing.SingleType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Providers.LuceneIndex.TaxonomySearcher.get` → *Examine.Lucene.Providers.ILuceneTaxonomySearcher?*
- `Examine.Lucene.Search.LuceneFacetExtractionContext.GetFacetCounts(string! facetIndexFieldName, bool isTaxonomyIndexed)` → *Lucene.Net.Facet.Facets!*
- `Examine.Lucene.Search.TaxonomySearcherReference.Dispose(bool disposing)` → *void*

##### Override (9)

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

##### Member (33)

- `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory?*
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
- `Examine.Lucene.Search.FacetQueryField.SetPath(params string![]! path)` → *Examine.Search.IFacetQueryField!*
- `Examine.Lucene.Search.IFacetExtractionContext.GetFacetCounts(string! facetIndexFieldName, bool isTaxonomyIndexed)` → *Lucene.Net.Facet.Facets!*
- `Examine.Lucene.Search.IFacetField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- `Examine.Lucene.Search.ITaxonomySearchContext.GetTaxonomyAndSearcher()` → *Examine.Lucene.Search.ITaxonomySearcherReference!*
- `Examine.Lucene.Search.LuceneFacetLabel.CompareTo(Examine.Search.IFacetLabel? other)` → *int*
- `Examine.Lucene.Search.LuceneFacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel!*
- `Examine.Lucene.Search.LuceneFacetOperation.Execute(Examine.Search.QueryOptions? options = null)` → *Examine.ISearchResults!*
- `Examine.Lucene.Search.LuceneFacetOperation.Facet(string! field, System.Action<Examine.Search.IFacetQueryField!>? facetConfiguration = null)` → *Examine.Search.IFacetOperations!*
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
260 new API(s) have been added. These are **safe changes** that do not break existing code.

### ⚠️ Breaking Changes
11 API(s) have **changed signatures**.

15 API(s) have been **removed**.

2 **abstract or interface member(s)** have been added to existing types (all derived classes / implementors must be updated).

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