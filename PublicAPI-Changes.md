# Public API Changes Report

Generated: 2025-12-17 14:55:44

## Summary

- **Projects with changes:** 3
- **Total new APIs:** 270
- **Total removed APIs:** 27 ⚠️ **BREAKING CHANGES**

## ⚠️ Breaking Changes

The following projects have **removed APIs** which constitute breaking changes:

### Examine.Core

#### Removed APIs (1) - BREAKING

The following public APIs have been **removed**:

- **Constructor**: `Examine.Search.ExamineValue.ExamineValue()`


### Examine.Lucene

#### Removed APIs (26) - BREAKING

The following public APIs have been **removed**:

- **Constructor**: `Examine.Lucene.Directories.FileSystemDirectoryFactory.FileSystemDirectoryFactory(System.IO.DirectoryInfo baseDir, Examine.Lucene.Directories.ILockFactory lockFactory)`
- **Constructor**: `Examine.Lucene.Directories.GenericDirectoryFactory.GenericDirectoryFactory(System.Func<string, Lucene.Net.Store.Directory> factory)`
- **Constructor**: `Examine.Lucene.Directories.SyncedFileSystemDirectoryFactory.SyncedFileSystemDirectoryFactory(System.IO.DirectoryInfo localDir, System.IO.DirectoryInfo mainDir, Examine.Lucene.Directories.ILockFactory lockFactory, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, bool tryFixMainIndexIfCorrupt)`
- **Constructor**: `Examine.Lucene.Directories.SyncedFileSystemDirectoryFactory.SyncedFileSystemDirectoryFactory(System.IO.DirectoryInfo localDir, System.IO.DirectoryInfo mainDir, Examine.Lucene.Directories.ILockFactory lockFactory, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)`
- **Constructor**: `Examine.Lucene.Directories.TempEnvFileSystemDirectoryFactory.TempEnvFileSystemDirectoryFactory(Examine.Lucene.Directories.IApplicationIdentifier applicationIdentifier, Examine.Lucene.Directories.ILockFactory lockFactory)`
- **Constructor**: `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator> replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient> clientLogger, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
- **Constructor**: `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator> replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient> clientLogger, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory sourceDirectory, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
- **Constructor**: `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Examine.Lucene.Providers.LuceneIndex sourceIndex, Lucene.Net.Store.Directory destinationDirectory, System.IO.DirectoryInfo tempStorage)`
- **Constructor**: `Examine.Lucene.Providers.BaseLuceneSearcher.BaseLuceneSearcher(string name, Lucene.Net.Analysis.Analyzer analyzer)`
- **Property**: `Examine.Lucene.Providers.LuceneIndex.IsCancellationRequested.get` → *bool*
- **Type**: `Examine.Lucene.Providers.LuceneSearcher`
- **Member**: `Examine.Lucene.Providers.LuceneSearcher.Dispose()` → *void*
- **Constructor**: `Examine.Lucene.Providers.LuceneSearcher.LuceneSearcher(string name, Lucene.Net.Search.SearcherManager searcherManager, Lucene.Net.Analysis.Analyzer analyzer, Examine.Lucene.FieldValueTypeCollection fieldValueTypeCollection, bool isNrt)`
- **Constructor**: `Examine.Lucene.Providers.LuceneSearcher.LuceneSearcher(string name, Lucene.Net.Search.SearcherManager searcherManager, Lucene.Net.Analysis.Analyzer analyzer, Examine.Lucene.FieldValueTypeCollection fieldValueTypeCollection)`
- **Constructor**: `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string name, System.Collections.Generic.IEnumerable<Examine.IIndex> indexes, Lucene.Net.Analysis.Analyzer analyzer = null)`
- **Constructor**: `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string name, System.Lazy<System.Collections.Generic.IEnumerable<Examine.ISearcher>> searchers, Lucene.Net.Analysis.Analyzer analyzer = null)`
- **Property**: `Examine.Lucene.Providers.MultiIndexSearcher.Searchers.get` → *System.Collections.Generic.IEnumerable<Examine.Lucene.Providers.LuceneSearcher>*
- **Constructor**: `Examine.Lucene.Search.LuceneQueryOptions.LuceneQueryOptions(int skip, int? take = null, Examine.Lucene.Search.SearchAfterOptions searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false, int skipTakeMaxResults = 10000, bool autoCalculateSkipTakeMaxResults = false)`
- **Constructor**: `Examine.Lucene.Search.LuceneSearchResults.LuceneSearchResults(System.Collections.Generic.IReadOnlyCollection<Examine.ISearchResult> results, int totalItemCount, float maxScore, Examine.Lucene.Search.SearchAfterOptions searchAfterOptions)`
- **Property**: `Examine.Lucene.Search.SearchAfterOptions.ShardIndex.get` → *int?*
- **Constructor**: `Examine.Lucene.Search.SearchContext.SearchContext(Lucene.Net.Search.SearcherManager searcherManager, Examine.Lucene.FieldValueTypeCollection fieldValueTypeCollection)`
- **Override**: `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory*
- **Override**: `Examine.Lucene.Directories.GenericDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory*
- **Override**: `Examine.Lucene.Providers.LuceneSearcher.GetSearchContext()` → *Examine.Lucene.Search.ISearchContext*
- **Virtual**: `Examine.Lucene.Providers.LuceneSearcher.Dispose(bool disposing)` → *void*
- **Virtual**: `Examine.Lucene.Search.LuceneSearchQueryBase.GetFieldInternalQuery(string fieldName, Examine.Search.IExamineValue fieldValue, bool useQueryParser)` → *Lucene.Net.Search.Query*


## ✅ New APIs (Non-Breaking)

The following projects have **new APIs** added:

### Examine.Core

#### New APIs (100)

The following public APIs have been added:

- **Constant**: `Examine.ExamineFieldNames.DefaultFacetsName` = "$facets" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDateDay` = "facetdate.day" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDateHour` = "facetdate.hour" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDateMinute` = "facetdate.minute" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDateMonth` = "facetdate.month" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDateTime` = "facetdatetime" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDateYear` = "facetdate.year" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetDouble` = "facetdouble" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetFloat` = "facetfloat" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetFullText` = "facetfulltext" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetFullTextSortable` = "facetfulltextsortable" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetInteger` = "facetint" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetLong` = "facetlong" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDateDay` = "facettaxonomydate.day" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDateHour` = "facettaxonomydate.hour" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDateMinute` = "facettaxonomydate.minute" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDateMonth` = "facettaxonomydate.month" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDateTime` = "facettaxonomydatetime" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDateYear` = "facettaxonomydate.year" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyDouble` = "facettaxonomydouble" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyFloat` = "facettaxonomyfloat" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyFullText` = "facettaxonomyfulltext" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyFullTextSortable` = "facettaxonomyfulltextsortable" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyInteger` = "facettaxonomyint" → *string*
- **Constant**: `Examine.FieldDefinitionTypes.FacetTaxonomyLong` = "facettaxonomylong" → *string*
- **Type**: `Examine.Search.DoubleRange`
- **Constructor**: `Examine.Search.DoubleRange.DoubleRange()`
- **Constructor**: `Examine.Search.DoubleRange.DoubleRange(string label, double min, bool minInclusive, double max, bool maxInclusive)`
- **Property**: `Examine.Search.DoubleRange.Label.get` → *string*
- **Property**: `Examine.Search.DoubleRange.Max.get` → *double*
- **Property**: `Examine.Search.DoubleRange.MaxInclusive.get` → *bool*
- **Property**: `Examine.Search.DoubleRange.Min.get` → *double*
- **Property**: `Examine.Search.DoubleRange.MinInclusive.get` → *bool*
- **Member**: `Examine.Search.Examineness.Default = 100` → *Examine.Search.Examineness*
- **Member**: `Examine.Search.Examineness.Phrase = 7` → *Examine.Search.Examineness*
- **Type**: `Examine.Search.ExamineValueExtensions`
- **Type**: `Examine.Search.FacetLabel`
- **Member**: `Examine.Search.FacetLabel.CompareTo(Examine.Search.IFacetLabel other)` → *int*
- **Property**: `Examine.Search.FacetLabel.Components.get` → *string[]*
- **Constructor**: `Examine.Search.FacetLabel.FacetLabel()`
- **Constructor**: `Examine.Search.FacetLabel.FacetLabel(string dimension, string[] components)`
- **Constructor**: `Examine.Search.FacetLabel.FacetLabel(string[] components)`
- **Property**: `Examine.Search.FacetLabel.Length.get` → *int*
- **Member**: `Examine.Search.FacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel*
- **Type**: `Examine.Search.FacetResult`
- **Member**: `Examine.Search.FacetResult.Facet(string label)` → *Examine.Search.IFacetValue*
- **Constructor**: `Examine.Search.FacetResult.FacetResult(System.Collections.Generic.IEnumerable<Examine.Search.IFacetValue> values)`
- **Member**: `Examine.Search.FacetResult.GetEnumerator()` → *System.Collections.Generic.IEnumerator<Examine.Search.IFacetValue>*
- **Member**: `Examine.Search.FacetResult.TryGetFacet(string label, out Examine.Search.IFacetValue facetValue)` → *bool*
- **Type**: `Examine.Search.FacetValue`
- **Constructor**: `Examine.Search.FacetValue.FacetValue()`
- **Constructor**: `Examine.Search.FacetValue.FacetValue(string label, float value)`
- **Property**: `Examine.Search.FacetValue.Label.get` → *string*
- **Property**: `Examine.Search.FacetValue.Value.get` → *float*
- **Type**: `Examine.Search.FloatRange`
- **Constructor**: `Examine.Search.FloatRange.FloatRange()`
- **Constructor**: `Examine.Search.FloatRange.FloatRange(string label, float min, bool minInclusive, float max, bool maxInclusive)`
- **Property**: `Examine.Search.FloatRange.Label.get` → *string*
- **Property**: `Examine.Search.FloatRange.Max.get` → *float*
- **Property**: `Examine.Search.FloatRange.MaxInclusive.get` → *bool*
- **Property**: `Examine.Search.FloatRange.Min.get` → *float*
- **Property**: `Examine.Search.FloatRange.MinInclusive.get` → *bool*
- **Type**: `Examine.Search.IExamineValueBoosted`
- **Property**: `Examine.Search.IExamineValueBoosted.Boost.get` → *float*
- **Type**: `Examine.Search.IFaceting`
- **Member**: `Examine.Search.IFaceting.WithFacets(System.Action<Examine.Search.IFacetOperations> facets)` → *Examine.Search.IQueryExecutor*
- **Type**: `Examine.Search.IFacetLabel`
- **Property**: `Examine.Search.IFacetLabel.Components.get` → *string[]*
- **Property**: `Examine.Search.IFacetLabel.Length.get` → *int*
- **Member**: `Examine.Search.IFacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel*
- **Type**: `Examine.Search.IFacetOperations`
- **Member**: `Examine.Search.IFacetOperations.FacetDoubleRange(string field, params Examine.Search.DoubleRange[] doubleRanges)` → *Examine.Search.IFacetOperations*
- **Member**: `Examine.Search.IFacetOperations.FacetFloatRange(string field, params Examine.Search.FloatRange[] floatRanges)` → *Examine.Search.IFacetOperations*
- **Member**: `Examine.Search.IFacetOperations.FacetLongRange(string field, params Examine.Search.Int64Range[] longRanges)` → *Examine.Search.IFacetOperations*
- **Member**: `Examine.Search.IFacetOperations.FacetString(string field, System.Action<Examine.Search.IFacetQueryField> facetConfiguration = null, params string[] values)` → *Examine.Search.IFacetOperations*
- **Type**: `Examine.Search.IFacetQueryField`
- **Member**: `Examine.Search.IFacetQueryField.MaxCount(int count)` → *Examine.Search.IFacetQueryField*
- **Member**: `Examine.Search.IFacetQueryField.SetPath(params string[] path)` → *Examine.Search.IFacetQueryField*
- **Type**: `Examine.Search.IFacetResult`
- **Member**: `Examine.Search.IFacetResult.Facet(string label)` → *Examine.Search.IFacetValue*
- **Member**: `Examine.Search.IFacetResult.TryGetFacet(string label, out Examine.Search.IFacetValue facetValue)` → *bool*
- **Type**: `Examine.Search.IFacetResults`
- **Property**: `Examine.Search.IFacetResults.Facets.get` → *System.Collections.Generic.IReadOnlyDictionary<string, Examine.Search.IFacetResult>*
- **Type**: `Examine.Search.IFacetValue`
- **Property**: `Examine.Search.IFacetValue.Label.get` → *string*
- **Property**: `Examine.Search.IFacetValue.Value.get` → *float*
- **Type**: `Examine.Search.Int64Range`
- **Constructor**: `Examine.Search.Int64Range.Int64Range()`
- **Constructor**: `Examine.Search.Int64Range.Int64Range(string label, long min, bool minInclusive, long max, bool maxInclusive)`
- **Property**: `Examine.Search.Int64Range.Label.get` → *string*
- **Property**: `Examine.Search.Int64Range.Max.get` → *long*
- **Property**: `Examine.Search.Int64Range.MaxInclusive.get` → *bool*
- **Property**: `Examine.Search.Int64Range.Min.get` → *long*
- **Property**: `Examine.Search.Int64Range.MinInclusive.get` → *bool*
- **Type**: `Examine.Search.OrderingExtensions`
- **Static**: `Examine.Search.ExamineValue.Create(Examine.Search.Examineness vagueness, string value)` → *Examine.Search.IExamineValue*
- **Static**: `Examine.Search.ExamineValue.Create(Examine.Search.Examineness vagueness, string value, float level)` → *Examine.Search.IExamineValue*
- **Static**: `Examine.Search.ExamineValueExtensions.WithBoost(this Examine.Search.IExamineValue examineValue, float boost)` → *Examine.Search.IExamineValue*
- **Static**: `Examine.Search.OrderingExtensions.WithFacets(this Examine.Search.IOrdering ordering, System.Action<Examine.Search.IFacetOperations> facets)` → *Examine.Search.IQueryExecutor*
- **Static**: `Examine.SearchExtensions.Phrase(this string s)` → *Examine.Search.IExamineValue*


### Examine.Host

#### New APIs (4)

The following public APIs have been added:

- **Static**: `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Action<Examine.Lucene.LuceneDirectoryIndexOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- **Static**: `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Action<Examine.Lucene.LuceneDirectoryIndexOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- **Static**: `Examine.ServicesCollectionExtensions.AddExamineLuceneIndex<TIndex>(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, System.Action<Examine.Lucene.LuceneDirectoryIndexOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*
- **Static**: `Examine.ServicesCollectionExtensions.AddExamineLuceneMultiSearcher(this Microsoft.Extensions.DependencyInjection.IServiceCollection! serviceCollection, string! name, string![]! indexNames, System.Action<Examine.Lucene.LuceneMultiSearcherOptions!>? configuration = null)` → *Microsoft.Extensions.DependencyInjection.IServiceCollection!*


### Examine.Lucene

#### New APIs (166)

The following public APIs have been added:

- **Abstract**: `Examine.Lucene.Directories.DirectoryFactoryBase.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- **Abstract**: `Examine.Lucene.Providers.BaseLuceneSearcher.Dispose()` → *void*
- **Abstract**: `Examine.Lucene.Search.LuceneBooleanOperationBase.WithFacets(System.Action<Examine.Search.IFacetOperations!>! facets)` → *Examine.Search.IQueryExecutor!*
- **Member**: `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- **Member**: `Examine.Lucene.Directories.GenericDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- **Member**: `Examine.Lucene.Directories.GenericDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- **Constructor**: `Examine.Lucene.Directories.GenericDirectoryFactory.GenericDirectoryFactory(System.Func<string!, Lucene.Net.Store.Directory!>! factory, System.Func<string!, Lucene.Net.Store.Directory!>! taxonomyDirectoryFactory)`
- **Member**: `Examine.Lucene.Directories.IDirectoryFactory.CreateTaxonomyDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- **Constructor**: `Examine.Lucene.ExamineReplicator.ExamineReplicator(Microsoft.Extensions.Logging.ILogger<Examine.Lucene.ExamineReplicator!>! replicatorLogger, Microsoft.Extensions.Logging.ILogger<Examine.Lucene.LoggingReplicationClient!>! clientLogger, Examine.Lucene.Providers.LuceneIndex! sourceIndex, Lucene.Net.Store.Directory! sourceDirectory, Lucene.Net.Store.Directory! destinationDirectory, Lucene.Net.Store.Directory! destinationTaxonomyDirectory, System.IO.DirectoryInfo! tempStorage)`
- **Type**: `Examine.Lucene.FacetExtensions`
- **Constructor**: `Examine.Lucene.Indexing.DateTimeType.DateTimeType(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, Lucene.Net.Documents.DateResolution resolution, bool store)`
- **Property**: `Examine.Lucene.Indexing.DateTimeType.IsTaxonomyFaceted.get` → *bool*
- **Constructor**: `Examine.Lucene.Indexing.DoubleType.DoubleType(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- **Property**: `Examine.Lucene.Indexing.DoubleType.IsTaxonomyFaceted.get` → *bool*
- **Constructor**: `Examine.Lucene.Indexing.FullTextType.FullTextType(string! fieldName, bool isFacetable, bool taxonomyIndex, bool sortable, Microsoft.Extensions.Logging.ILoggerFactory! logger, Lucene.Net.Analysis.Analyzer! analyzer)`
- **Property**: `Examine.Lucene.Indexing.FullTextType.IsTaxonomyFaceted.get` → *bool*
- **Type**: `Examine.Lucene.Indexing.IIndexFacetValueType`
- **Member**: `Examine.Lucene.Indexing.IIndexFacetValueType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Property**: `Examine.Lucene.Indexing.IIndexFacetValueType.IsTaxonomyFaceted.get` → *bool*
- **Constructor**: `Examine.Lucene.Indexing.Int32Type.Int32Type(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- **Property**: `Examine.Lucene.Indexing.Int32Type.IsTaxonomyFaceted.get` → *bool*
- **Constructor**: `Examine.Lucene.Indexing.Int64Type.Int64Type(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- **Property**: `Examine.Lucene.Indexing.Int64Type.IsTaxonomyFaceted.get` → *bool*
- **Property**: `Examine.Lucene.Indexing.SingleType.IsTaxonomyFaceted.get` → *bool*
- **Constructor**: `Examine.Lucene.Indexing.SingleType.SingleType(string! fieldName, bool isFacetable, bool taxonomyIndex, Microsoft.Extensions.Logging.ILoggerFactory! logger, bool store)`
- **Property**: `Examine.Lucene.LuceneIndexOptions.FacetsConfig.get` → *Lucene.Net.Facet.FacetsConfig!*
- **Property**: `Examine.Lucene.LuceneIndexOptions.FacetsConfig.set` → *void*
- **Type**: `Examine.Lucene.LuceneMultiSearcherOptions`
- **Property**: `Examine.Lucene.LuceneMultiSearcherOptions.IndexNames.get` → *string![]!*
- **Property**: `Examine.Lucene.LuceneMultiSearcherOptions.IndexNames.set` → *void*
- **Constructor**: `Examine.Lucene.LuceneMultiSearcherOptions.LuceneMultiSearcherOptions()`
- **Type**: `Examine.Lucene.LuceneSearcherOptions`
- **Property**: `Examine.Lucene.LuceneSearcherOptions.Analyzer.get` → *Lucene.Net.Analysis.Analyzer?*
- **Property**: `Examine.Lucene.LuceneSearcherOptions.Analyzer.set` → *void*
- **Property**: `Examine.Lucene.LuceneSearcherOptions.FacetConfiguration.get` → *Lucene.Net.Facet.FacetsConfig?*
- **Property**: `Examine.Lucene.LuceneSearcherOptions.FacetConfiguration.set` → *void*
- **Constructor**: `Examine.Lucene.LuceneSearcherOptions.LuceneSearcherOptions()`
- **Constructor**: `Examine.Lucene.Providers.BaseLuceneSearcher.BaseLuceneSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneSearcherOptions!>! options)`
- **Type**: `Examine.Lucene.Providers.IIndexCommitter`
- **Member**: `Examine.Lucene.Providers.IIndexCommitter.CommitError` → *System.EventHandler<Examine.IndexingErrorEventArgs!>!*
- **Member**: `Examine.Lucene.Providers.IIndexCommitter.CommitNow()` → *void*
- **Member**: `Examine.Lucene.Providers.IIndexCommitter.Committed` → *System.EventHandler?*
- **Member**: `Examine.Lucene.Providers.IIndexCommitter.ScheduleCommit()` → *void*
- **Type**: `Examine.Lucene.Providers.ILuceneTaxonomySearcher`
- **Property**: `Examine.Lucene.Providers.ILuceneTaxonomySearcher.CategoryCount.get` → *int*
- **Member**: `Examine.Lucene.Providers.ILuceneTaxonomySearcher.GetOrdinal(string! dim, string![]! path)` → *int*
- **Member**: `Examine.Lucene.Providers.ILuceneTaxonomySearcher.GetPath(int ordinal)` → *Examine.Search.IFacetLabel!*
- **Member**: `Examine.Lucene.Providers.LuceneIndex.GetLuceneTaxonomyDirectory()` → *Lucene.Net.Store.Directory!*
- **Constructor**: `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneMultiSearcherOptions!>! options, System.Collections.Generic.IEnumerable<Examine.IIndex!>! indexes)`
- **Constructor**: `Examine.Lucene.Providers.MultiIndexSearcher.MultiIndexSearcher(string! name, Microsoft.Extensions.Options.IOptionsMonitor<Examine.Lucene.LuceneMultiSearcherOptions!>! options, System.Lazy<System.Collections.Generic.IEnumerable<Examine.ISearcher!>!>! searchers)`
- **Property**: `Examine.Lucene.Providers.MultiIndexSearcher.Searchers.get` → *System.Collections.Generic.IEnumerable<Examine.Lucene.Providers.BaseLuceneSearcher!>!*
- **Member**: `Examine.Lucene.Search.CustomMultiFieldQueryParser.GetPhraseQueryInternal(string! field, string! queryText)` → *Lucene.Net.Search.Query!*
- **Type**: `Examine.Lucene.Search.FacetDoubleField`
- **Property**: `Examine.Lucene.Search.FacetDoubleField.DoubleRanges.get` → *Examine.Search.DoubleRange[]!*
- **Member**: `Examine.Lucene.Search.FacetDoubleField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Constructor**: `Examine.Lucene.Search.FacetDoubleField.FacetDoubleField()`
- **Constructor**: `Examine.Lucene.Search.FacetDoubleField.FacetDoubleField(string! field, Examine.Search.DoubleRange[]! doubleRanges, string! facetField, bool isTaxonomyIndexed = false)`
- **Property**: `Examine.Lucene.Search.FacetDoubleField.FacetField.get` → *string!*
- **Property**: `Examine.Lucene.Search.FacetDoubleField.Field.get` → *string!*
- **Property**: `Examine.Lucene.Search.FacetDoubleField.IsTaxonomyIndexed.get` → *bool*
- **Type**: `Examine.Lucene.Search.FacetFloatField`
- **Member**: `Examine.Lucene.Search.FacetFloatField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Property**: `Examine.Lucene.Search.FacetFloatField.FacetField.get` → *string!*
- **Constructor**: `Examine.Lucene.Search.FacetFloatField.FacetFloatField()`
- **Constructor**: `Examine.Lucene.Search.FacetFloatField.FacetFloatField(string! field, Examine.Search.FloatRange[]! floatRanges, string! facetField, bool isTaxonomyIndexed = false)`
- **Property**: `Examine.Lucene.Search.FacetFloatField.Field.get` → *string!*
- **Property**: `Examine.Lucene.Search.FacetFloatField.FloatRanges.get` → *Examine.Search.FloatRange[]!*
- **Property**: `Examine.Lucene.Search.FacetFloatField.IsTaxonomyIndexed.get` → *bool*
- **Type**: `Examine.Lucene.Search.FacetFullTextField`
- **Member**: `Examine.Lucene.Search.FacetFullTextField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Property**: `Examine.Lucene.Search.FacetFullTextField.FacetField.get` → *string!*
- **Constructor**: `Examine.Lucene.Search.FacetFullTextField.FacetFullTextField(string! field, string![]! values, string! facetField, int maxCount = 10, string![]? path = null, bool isTaxonomyIndexed = false)`
- **Property**: `Examine.Lucene.Search.FacetFullTextField.Field.get` → *string!*
- **Property**: `Examine.Lucene.Search.FacetFullTextField.IsTaxonomyIndexed.get` → *bool*
- **Property**: `Examine.Lucene.Search.FacetFullTextField.MaxCount.get` → *int*
- **Property**: `Examine.Lucene.Search.FacetFullTextField.Path.get` → *string![]?*
- **Property**: `Examine.Lucene.Search.FacetFullTextField.Values.get` → *string![]!*
- **Type**: `Examine.Lucene.Search.FacetLongField`
- **Member**: `Examine.Lucene.Search.FacetLongField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Property**: `Examine.Lucene.Search.FacetLongField.FacetField.get` → *string!*
- **Constructor**: `Examine.Lucene.Search.FacetLongField.FacetLongField()`
- **Constructor**: `Examine.Lucene.Search.FacetLongField.FacetLongField(string! field, Examine.Search.Int64Range[]! longRanges, string! facetField, bool isTaxonomyIndexed = false)`
- **Property**: `Examine.Lucene.Search.FacetLongField.Field.get` → *string!*
- **Property**: `Examine.Lucene.Search.FacetLongField.IsTaxonomyIndexed.get` → *bool*
- **Property**: `Examine.Lucene.Search.FacetLongField.LongRanges.get` → *Examine.Search.Int64Range[]!*
- **Type**: `Examine.Lucene.Search.FacetQueryField`
- **Constructor**: `Examine.Lucene.Search.FacetQueryField.FacetQueryField(Examine.Lucene.Search.FacetFullTextField! field)`
- **Member**: `Examine.Lucene.Search.FacetQueryField.MaxCount(int count)` → *Examine.Search.IFacetQueryField!*
- **Member**: `Examine.Lucene.Search.FacetQueryField.SetPath(params string![]! path)` → *Examine.Search.IFacetQueryField!*
- **Type**: `Examine.Lucene.Search.IFacetExtractionContext`
- **Property**: `Examine.Lucene.Search.IFacetExtractionContext.FacetConfig.get` → *Lucene.Net.Facet.FacetsConfig!*
- **Property**: `Examine.Lucene.Search.IFacetExtractionContext.FacetsCollector.get` → *Lucene.Net.Facet.FacetsCollector!*
- **Member**: `Examine.Lucene.Search.IFacetExtractionContext.GetFacetCounts(string! facetIndexFieldName, bool isTaxonomyIndexed)` → *Lucene.Net.Facet.Facets!*
- **Property**: `Examine.Lucene.Search.IFacetExtractionContext.SearcherReference.get` → *Examine.Lucene.Search.ISearcherReference!*
- **Type**: `Examine.Lucene.Search.IFacetField`
- **Member**: `Examine.Lucene.Search.IFacetField.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Property**: `Examine.Lucene.Search.IFacetField.FacetField.get` → *string!*
- **Property**: `Examine.Lucene.Search.IFacetField.Field.get` → *string!*
- **Property**: `Examine.Lucene.Search.IFacetField.IsTaxonomyIndexed.get` → *bool*
- **Type**: `Examine.Lucene.Search.ITaxonomySearchContext`
- **Member**: `Examine.Lucene.Search.ITaxonomySearchContext.GetTaxonomyAndSearcher()` → *Examine.Lucene.Search.ITaxonomySearcherReference!*
- **Type**: `Examine.Lucene.Search.ITaxonomySearcherReference`
- **Property**: `Examine.Lucene.Search.ITaxonomySearcherReference.TaxonomyReader.get` → *Lucene.Net.Facet.Taxonomy.Directory.DirectoryTaxonomyReader!*
- **Type**: `Examine.Lucene.Search.LuceneFacetExtractionContext`
- **Property**: `Examine.Lucene.Search.LuceneFacetExtractionContext.FacetConfig.get` → *Lucene.Net.Facet.FacetsConfig!*
- **Property**: `Examine.Lucene.Search.LuceneFacetExtractionContext.FacetsCollector.get` → *Lucene.Net.Facet.FacetsCollector!*
- **Constructor**: `Examine.Lucene.Search.LuceneFacetExtractionContext.LuceneFacetExtractionContext(Lucene.Net.Facet.FacetsCollector! facetsCollector, Examine.Lucene.Search.ISearcherReference! searcherReference, Lucene.Net.Facet.FacetsConfig! facetConfig)`
- **Property**: `Examine.Lucene.Search.LuceneFacetExtractionContext.SearcherReference.get` → *Examine.Lucene.Search.ISearcherReference!*
- **Type**: `Examine.Lucene.Search.LuceneFacetLabel`
- **Member**: `Examine.Lucene.Search.LuceneFacetLabel.CompareTo(Examine.Search.IFacetLabel? other)` → *int*
- **Property**: `Examine.Lucene.Search.LuceneFacetLabel.Components.get` → *string![]!*
- **Property**: `Examine.Lucene.Search.LuceneFacetLabel.Length.get` → *int*
- **Constructor**: `Examine.Lucene.Search.LuceneFacetLabel.LuceneFacetLabel(Lucene.Net.Facet.Taxonomy.FacetLabel! facetLabel)`
- **Member**: `Examine.Lucene.Search.LuceneFacetLabel.Subpath(int length)` → *Examine.Search.IFacetLabel!*
- **Type**: `Examine.Lucene.Search.LuceneFacetOperation`
- **Member**: `Examine.Lucene.Search.LuceneFacetOperation.Execute(Examine.Search.QueryOptions? options = null)` → *Examine.ISearchResults!*
- **Member**: `Examine.Lucene.Search.LuceneFacetOperation.Facet(string! field, System.Action<Examine.Search.IFacetQueryField!>? facetConfiguration = null)` → *Examine.Search.IFacetOperations!*
- **Member**: `Examine.Lucene.Search.LuceneFacetOperation.FacetDoubleRange(string! field, params Examine.Search.DoubleRange[]! doubleRanges)` → *Examine.Search.IFacetOperations!*
- **Member**: `Examine.Lucene.Search.LuceneFacetOperation.FacetFloatRange(string! field, params Examine.Search.FloatRange[]! floatRanges)` → *Examine.Search.IFacetOperations!*
- **Member**: `Examine.Lucene.Search.LuceneFacetOperation.FacetLongRange(string! field, params Examine.Search.Int64Range[]! longRanges)` → *Examine.Search.IFacetOperations!*
- **Member**: `Examine.Lucene.Search.LuceneFacetOperation.FacetString(string! field, System.Action<Examine.Search.IFacetQueryField!>? facetConfiguration = null, params string![]! values)` → *Examine.Search.IFacetOperations!*
- **Constructor**: `Examine.Lucene.Search.LuceneFacetOperation.LuceneFacetOperation(Examine.Lucene.Search.LuceneSearchQuery! search)`
- **Type**: `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions`
- **Constructor**: `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.LuceneFacetSamplingQueryOptions(int sampleSize)`
- **Constructor**: `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.LuceneFacetSamplingQueryOptions(int sampleSize, long seed)`
- **Property**: `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.SampleSize.get` → *int*
- **Property**: `Examine.Lucene.Search.LuceneFacetSamplingQueryOptions.Seed.get` → *long*
- **Property**: `Examine.Lucene.Search.LuceneQueryOptions.FacetRandomSampling.get` → *Examine.Lucene.Search.LuceneFacetSamplingQueryOptions?*
- **Constructor**: `Examine.Lucene.Search.LuceneQueryOptions.LuceneQueryOptions(int skip, int take = 100, Examine.Lucene.Search.SearchAfterOptions? searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false, int skipTakeMaxResults = 10000, bool autoCalculateSkipTakeMaxResults = false, Examine.Lucene.Search.LuceneFacetSamplingQueryOptions? facetSampling = null)`
- **Constructor**: `Examine.Lucene.Search.LuceneSearchQuery.LuceneSearchQuery(Examine.Lucene.Search.ISearchContext! searchContext, string? category, Lucene.Net.Analysis.Analyzer! analyzer, Examine.Lucene.Search.LuceneSearchOptions! searchOptions, Examine.Search.BooleanOperation occurance, Lucene.Net.Facet.FacetsConfig! facetsConfig)`
- **Property**: `Examine.Lucene.Search.LuceneSearchResults.Facets.get` → *System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Search.IFacetResult!>!*
- **Constructor**: `Examine.Lucene.Search.LuceneSearchResults.LuceneSearchResults(System.Collections.Generic.IReadOnlyCollection<Examine.ISearchResult!>! results, int totalItemCount, System.Collections.Generic.IReadOnlyDictionary<string!, Examine.Search.IFacetResult!>! facets, float maxScore, Examine.Lucene.Search.SearchAfterOptions? searchAfterOptions)`
- **Property**: `Examine.Lucene.Search.SearchAfterOptions.ShardIndex.get` → *int*
- **Type**: `Examine.Lucene.Search.TaxonomySearchContext`
- **Member**: `Examine.Lucene.Search.TaxonomySearchContext.GetFieldValueType(string! fieldName)` → *Examine.Lucene.Indexing.IIndexFieldValueType!*
- **Member**: `Examine.Lucene.Search.TaxonomySearchContext.GetSearcher()` → *Examine.Lucene.Search.ISearcherReference!*
- **Member**: `Examine.Lucene.Search.TaxonomySearchContext.GetTaxonomyAndSearcher()` → *Examine.Lucene.Search.ITaxonomySearcherReference!*
- **Property**: `Examine.Lucene.Search.TaxonomySearchContext.SearchableFields.get` → *string![]!*
- **Constructor**: `Examine.Lucene.Search.TaxonomySearchContext.TaxonomySearchContext(Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager! searcherManager, Examine.Lucene.FieldValueTypeCollection! fieldValueTypeCollection, bool isNrt)`
- **Type**: `Examine.Lucene.Search.TaxonomySearcherReference`
- **Member**: `Examine.Lucene.Search.TaxonomySearcherReference.Dispose()` → *void*
- **Property**: `Examine.Lucene.Search.TaxonomySearcherReference.IndexSearcher.get` → *Lucene.Net.Search.IndexSearcher!*
- **Property**: `Examine.Lucene.Search.TaxonomySearcherReference.TaxonomyReader.get` → *Lucene.Net.Facet.Taxonomy.Directory.DirectoryTaxonomyReader!*
- **Constructor**: `Examine.Lucene.Search.TaxonomySearcherReference.TaxonomySearcherReference(Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager! searcherManager)`
- **Override**: `Examine.Lucene.Indexing.DateTimeType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- **Override**: `Examine.Lucene.Indexing.DoubleType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- **Override**: `Examine.Lucene.Indexing.FullTextType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- **Override**: `Examine.Lucene.Indexing.Int32Type.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- **Override**: `Examine.Lucene.Indexing.Int64Type.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- **Override**: `Examine.Lucene.Indexing.SingleType.AddValue(Lucene.Net.Documents.Document! doc, object? value)` → *void*
- **Override**: `Examine.Lucene.Providers.MultiIndexSearcher.Dispose()` → *void*
- **Override**: `Examine.Lucene.Search.LuceneBooleanOperation.WithFacets(System.Action<Examine.Search.IFacetOperations!>! facets)` → *Examine.Search.IQueryExecutor!*
- **Override**: `Examine.Lucene.Search.LuceneFacetOperation.ToString()` → *string!*
- **Static**: `Examine.Lucene.FacetExtensions.GetFacet(this Examine.ISearchResults! searchResults, string! field)` → *Examine.Search.IFacetResult?*
- **Static**: `Examine.Lucene.FacetExtensions.GetFacets(this Examine.ISearchResults! searchResults)` → *System.Collections.Generic.IEnumerable<Examine.Search.IFacetResult!>!*
- **Virtual**: `Examine.Lucene.Directories.FileSystemDirectoryFactory.CreateDirectory(Examine.Lucene.Providers.LuceneIndex! luceneIndex, bool forceUnlock)` → *Lucene.Net.Store.Directory!*
- **Virtual**: `Examine.Lucene.Indexing.DateTimeType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Virtual**: `Examine.Lucene.Indexing.DoubleType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Virtual**: `Examine.Lucene.Indexing.FullTextType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Virtual**: `Examine.Lucene.Indexing.Int32Type.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Virtual**: `Examine.Lucene.Indexing.Int64Type.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Virtual**: `Examine.Lucene.Indexing.SingleType.ExtractFacets(Examine.Lucene.Search.IFacetExtractionContext! facetExtractionContext, Examine.Lucene.Search.IFacetField! field)` → *System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string!, Examine.Search.IFacetResult!>>!*
- **Virtual**: `Examine.Lucene.Providers.LuceneIndex.TaxonomySearcher.get` → *Examine.Lucene.Providers.ILuceneTaxonomySearcher?*
- **Virtual**: `Examine.Lucene.Search.LuceneFacetExtractionContext.GetFacetCounts(string! facetIndexFieldName, bool isTaxonomyIndexed)` → *Lucene.Net.Facet.Facets!*
- **Virtual**: `Examine.Lucene.Search.LuceneSearchQueryBase.GetFieldInternalQuery(string! fieldName, Examine.Search.IExamineValue! fieldValue)` → *Lucene.Net.Search.Query?*
- **Virtual**: `Examine.Lucene.Search.TaxonomySearcherReference.Dispose(bool disposing)` → *void*


## Summary

### ✅ Additions (Non-Breaking)
270 new API(s) have been added. These are **safe changes** that do not break existing code.

### ⚠️ Breaking Changes27 API(s) have been **removed**. These are **BREAKING CHANGES** that will require:
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