using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Examine.Lucene.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Facet.Taxonomy.WriterCache;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Lucene.Net.Facet.Taxonomy.SearcherTaxonomyManager;

namespace Examine.Lucene.Providers
{
    public class LuceneTaxonomyIndex : LuceneIndex
    {
        public LuceneTaxonomyIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions) : base(loggerFactory, name, indexOptions)
        {
            _logger = loggerFactory.CreateLogger<LuceneTaxonomyIndex>();
            _options = indexOptions.CurrentValue;
        }

        internal LuceneTaxonomyIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsMonitor<LuceneIndexOptions> indexOptions,
            IndexWriter writer) : base(loggerFactory, name, indexOptions, writer)
        {
            _logger = loggerFactory.CreateLogger<LuceneTaxonomyIndex>();
            _options = indexOptions.CurrentValue;
        }
        private ILogger<LuceneTaxonomyIndex> _logger;
        private volatile DirectoryTaxonomyWriter _taxonomyWriter;
        private ControlledRealTimeReopenThread<SearcherAndTaxonomy> _nrtTaxonomyReopenThread;
        // tracks the latest Generation value of what has been indexed.This can be used to force update a searcher to this generation.
        private long? _latestGen;
        private readonly LuceneIndexOptions _options;
        private readonly Lazy<LuceneTaxonomySearcher> _searcher;
        private object _taxonomyWriterLocker = new object();

        /// <summary>
        /// Collects the data for the fields and adds the document which is then committed into Lucene.Net's index
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="valueSet">The data to index.</param>
        /// <param name="writer">The writer that will be used to update the Lucene index.</param>
        protected virtual void AddDocument(Document doc, ValueSet valueSet)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("{IndexName} Write lucene doc id:{DocumentId}, category:{DocumentCategory}, type:{DocumentItemType}",
                Name,
                valueSet.Id,
                valueSet.Category,
                valueSet.ItemType);
            }

            //add node id
            IIndexFieldValueType nodeIdValueType = FieldValueTypeCollection.GetValueType(ExamineFieldNames.ItemIdFieldName, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.Raw));
            nodeIdValueType.AddValue(doc, valueSet.Id);

            //add the category
            IIndexFieldValueType categoryValueType = FieldValueTypeCollection.GetValueType(ExamineFieldNames.CategoryFieldName, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.InvariantCultureIgnoreCase));
            categoryValueType.AddValue(doc, valueSet.Category);

            //add the item type
            IIndexFieldValueType indexTypeValueType = FieldValueTypeCollection.GetValueType(ExamineFieldNames.ItemTypeFieldName, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.InvariantCultureIgnoreCase));
            indexTypeValueType.AddValue(doc, valueSet.ItemType);

            foreach (KeyValuePair<string, IReadOnlyList<object>> field in valueSet.Values)
            {
                //check if we have a defined one
                if (FieldDefinitions.TryGetValue(field.Key, out FieldDefinition definedFieldDefinition))
                {
                    IIndexFieldValueType valueType = FieldValueTypeCollection.GetValueType(
                        definedFieldDefinition.Name,
                        FieldValueTypeCollection.ValueTypeFactories.TryGetFactory(definedFieldDefinition.Type, out var valTypeFactory)
                            ? valTypeFactory
                            : FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));

                    foreach (var o in field.Value)
                    {
                        valueType.AddValue(doc, o);
                    }
                }
                else if (field.Key.StartsWith(ExamineFieldNames.SpecialFieldPrefix))
                {
                    //Check for the special field prefix, if this is the case it's indexed as an invariant culture value

                    IIndexFieldValueType valueType = FieldValueTypeCollection.GetValueType(field.Key, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.InvariantCultureIgnoreCase));
                    foreach (var o in field.Value)
                    {
                        valueType.AddValue(doc, o);
                    }
                }
                else
                {
                    // wasn't specifically defined, use FullText as the default

                    IIndexFieldValueType valueType = FieldValueTypeCollection.GetValueType(
                        field.Key,
                        FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));

                    foreach (var o in field.Value)
                    {
                        valueType.AddValue(doc, o);
                    }
                }
            }

            var docArgs = new DocumentWritingEventArgs(valueSet, doc);
            OnDocumentWriting(docArgs);
            if (docArgs.Cancel)
            {
                return;
            }

            // TODO: try/catch with OutOfMemoryException (see docs on UpdateDocument), though i've never seen this in real life
            _latestGen = IndexWriter.UpdateDocument(new Term(ExamineFieldNames.ItemIdFieldName, valueSet.Id), _options.FacetsConfig.Build(TaxonomyWriter, doc));
        }

        /// <summary>
        /// Used to create an index writer - this is called in GetIndexWriter (and therefore, GetIndexWriter should not be overridden)
        /// </summary>
        /// <returns></returns>
        private DirectoryTaxonomyWriter CreateTaxonomyIndexWriterInternal()
        {
            Directory dir = GetLuceneDirectory();

            // Unfortunatley if the appdomain is taken down this will remain locked, so we can 
            // ensure that it's unlocked here in that case.
            try
            {
                if (IsLocked(dir))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Forcing index {IndexName} to be unlocked since it was left in a locked state", Name);
                    }
                    //unlock it!
                    Unlock(dir);
                }
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "The index was locked and could not be unlocked", null, ex));
                return null;
            }

            return CreateTaxonomyIndexWriter(dir);
        }

        /// <summary>
        /// Method that creates the IndexWriter
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        protected virtual DirectoryTaxonomyWriter CreateTaxonomyIndexWriter(Directory d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }

            var taxonomyWriterCache = new LruTaxonomyWriterCache(10000);
            var taxonomyWriter = new DirectoryTaxonomyWriter(d, OpenMode.CREATE_OR_APPEND, taxonomyWriterCache);

            return taxonomyWriter;
        }

        /// <summary>
        /// Gets the ITaxonomyWriter for the current directory
        /// </summary>
        /// <remarks>
        public DirectoryTaxonomyWriter TaxonomyWriter
        {
            get
            {
                //EnsureTaxonomyIndex(false);

                if (_taxonomyWriter == null)
                {
                    Monitor.Enter(_taxonomyWriterLocker);
                    try
                    {
                        if (_taxonomyWriter == null)
                        {
                            _taxonomyWriter = CreateTaxonomyIndexWriterInternal();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_taxonomyWriterLocker);
                    }

                }

                return _taxonomyWriter;
            }
        }

        private LuceneTaxonomySearcher CreateTaxonomySearcher()
        {
            var possibleSuffixes = new[] { "Index", "Indexer" };
            var name = Name;
            foreach (var suffix in possibleSuffixes)
            {
                //trim the "Indexer" / "Index" suffix if it exists
                if (!name.EndsWith(suffix))
                    continue;
                name = name.Substring(0, name.LastIndexOf(suffix, StringComparison.Ordinal));
            }

            TrackingIndexWriter writer = IndexWriter;
            var taxonomySearcherManager = new SearcherTaxonomyManager(writer.IndexWriter, true, new SearcherFactory(), TaxonomyWriter);
            taxonomySearcherManager.AddListener(this);
            _nrtTaxonomyReopenThread = new ControlledRealTimeReopenThread<SearcherTaxonomyManager.SearcherAndTaxonomy>(writer, taxonomySearcherManager, 5.0, 1.0)
            {
                Name = $"{Name} NRT Taxonomy Reopen Thread",
                IsBackground = true
            };

            _nrtTaxonomyReopenThread.Start();

            // wait for most recent changes when first creating the searcher
            WaitForChanges();

            return new LuceneTaxonomySearcher(name + "Searcher", taxonomySearcherManager, FieldAnalyzer, FieldValueTypeCollection, _options.FacetsConfig);

        }


        protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
        {
            // need to lock, we don't want to issue any node writing if there's an index rebuild occuring
            lock (_writerLocker)
            {
                var currentToken = _cancellationToken;

                if (RunAsync)
                {
                    QueueTask(() => PerformIndexItemsInternal(values, currentToken), onComplete, currentToken);
                }
                else
                {
                    var count = 0;
                    try
                    {
                        count = PerformIndexItemsInternal(values, currentToken);
                    }
                    finally
                    {
                        onComplete?.Invoke(new IndexOperationEventArgs(this, count));
                    }
                }
            }
        }

        private int PerformIndexItemsInternal(IEnumerable<ValueSet> valueSets, CancellationToken cancellationToken)
        {
            //check if the index is ready to be written to.
            if (!IndexReady())
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Cannot index queue items, the index is currently locked", null, null));
                return 0;
            }

            //track all of the nodes indexed
            var indexedNodes = 0;

            Interlocked.Increment(ref _activeWrites);

            try
            {
                foreach (var valueSet in valueSets)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var op = new IndexOperation(valueSet, IndexOperationType.Add);
                    if (ProcessQueueItem(op))
                    {
                        indexedNodes++;
                    }
                }

                if (indexedNodes > 0)
                {
                    //this is required to ensure the index is written to during the same thread execution
                    if (!RunAsync)
                    {
                        //commit the changes
                        _committer.CommitNow();

                        // now force any searcher to be updated.
                        WaitForChanges();
                    }
                    else
                    {
                        _committer.ScheduleCommit();
                    }
                }
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Error indexing queue items", null, ex));
            }
            finally
            {
                Interlocked.Decrement(ref _activeWrites);
            }

            return indexedNodes;
        }
    }
}
