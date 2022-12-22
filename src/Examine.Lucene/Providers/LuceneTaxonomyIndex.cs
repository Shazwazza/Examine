using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;
using static Lucene.Net.Index.IndexWriter;
using Microsoft.Extensions.Options;
using Examine.Lucene.Indexing;
using Examine.Lucene.Directories;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using static Lucene.Net.Util.Packed.PackedInt32s;

namespace Examine.Lucene.Providers
{
    // TODO: I think we should borrow the IBackgroundTaskQueue from Umbraco and use that to do all background
    // task processing? seems it would be a lot simpler? Then can be replaced with aspnetcore implementation

    ///<summary>
    /// Lucene Index with Taxonomy Index
    ///</summary>
    public class LuceneTaxonomyIndex : LuceneIndex, IDisposable, IIndexStats, ReferenceManager.IRefreshListener
    {
        #region Constructors

        public LuceneTaxonomyIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
            : base(loggerFactory, name, indexOptions, CreateIndexCommiter())
        {
            _options = indexOptions.GetNamedOptions(name);
            _logger = loggerFactory.CreateLogger<LuceneIndex>();

            _searcher = new Lazy<LuceneTaxonomySearcher>(CreateSearcher);
        }

        //TODO: The problem with this is that the writer would already need to be configured with a PerFieldAnalyzerWrapper
        // with all of the field definitions in place, etc... but that will most likely never happen
        /// <summary>
        /// Constructor to allow for creating an indexer at runtime - using NRT
        /// </summary>
        internal LuceneTaxonomyIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsMonitor<LuceneIndexOptions> indexOptions,
            IndexWriter writer)
               : base(loggerFactory, name, indexOptions, writer)
        {
            _options = indexOptions.GetNamedOptions(name);
            _logger = loggerFactory.CreateLogger<LuceneIndex>();

            _searcher = new Lazy<LuceneTaxonomySearcher>(CreateSearcher);
        }


        private static Func<LuceneIndex, IIndexCommiter> CreateIndexCommiter() => (index) =>
        {
            if (!(index is LuceneTaxonomyIndex taxonomyIndex))
            {
                throw new NotSupportedException("TaxonomyIndexCommiter only supports LuceneTaxonomyIndex");
            }
            return new TaxonomyIndexCommiter(taxonomyIndex);
        };


        #endregion

        private volatile DirectoryTaxonomyWriter _taxonomyWriter;
        
        private readonly LuceneIndexOptions _options;
        private ControlledRealTimeReopenThread<SearcherTaxonomyManager.SearcherAndTaxonomy> _nrtReopenThread;
        private readonly ILogger<LuceneIndex> _logger;

        /// <summary>
        /// Used to aquire the index writer
        /// </summary>
        private readonly object _writerLocker = new object();

        private readonly Lazy<LuceneTaxonomySearcher> _searcher;


        /// <summary>
        /// Gets a searcher for the index
        /// </summary>
        public override ISearcher Searcher => _searcher.Value;

        #region Protected

        /// <summary>
        /// Collects the data for the fields and adds the document which is then committed into Lucene.Net's index
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="valueSet">The data to index.</param>
        /// <param name="writer">The writer that will be used to update the Lucene index.</param>
        protected override void AddDocument(Document doc, ValueSet valueSet)
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
            _latestGen = IndexWriter.UpdateDocument(new Term(ExamineFieldNames.ItemIdFieldName, valueSet.Id), _options.FacetsConfig.Build(TaxonomyWriter,doc));
        }

        /// <summary>
        /// This queues up a commit for the index so that a commit doesn't happen on every individual write since that is quite expensive
        /// </summary>
        private class TaxonomyIndexCommiter : DisposableObjectSlim, IIndexCommiter
        {
            private readonly LuceneTaxonomyIndex _index;
            private DateTime _timestamp;
            private Timer _timer;
            private readonly object _locker = new object();
            private const int WaitMilliseconds = 2000;

            /// <summary>
            /// The maximum time period that will elapse until we must commit (5 mins)
            /// </summary>
            private const int MaxWaitMilliseconds = 300000;

            public TaxonomyIndexCommiter(LuceneTaxonomyIndex index)
            {
                _index = index;
            }

            public void CommitNow()
            {
                _index._taxonomyWriter?.Commit();
                // Taxonomy Writer must commit before IndexWriter.
                _index.IndexWriter?.IndexWriter?.Commit();
                _index.RaiseIndexCommited(_index, EventArgs.Empty);
            }

            public void ScheduleCommit()
            {
                lock (_locker)
                {
                    if (_timer == null)
                    {
                        //if we've been cancelled then be sure to commit now
                        if (_index.IsCancellationRequested)
                        {
                            // perform the commit
                            CommitNow();
                        }
                        else
                        {
                            //It's the initial call to this at the beginning or after successful commit
                            _timestamp = DateTime.Now;
                            _timer = new Timer(_ => TimerRelease());
                            _timer.Change(WaitMilliseconds, 0);
                        }
                    }
                    else
                    {
                        //if we've been cancelled then be sure to cancel the timer and commit now
                        if (_index.IsCancellationRequested)
                        {
                            //Stop the timer
                            _timer.Change(Timeout.Infinite, Timeout.Infinite);
                            _timer.Dispose();
                            _timer = null;

                            //perform the commit
                            CommitNow();
                        }
                        else if (
                            // must be less than the max
                            DateTime.Now - _timestamp < TimeSpan.FromMilliseconds(MaxWaitMilliseconds) &&
                            // and less than the delay
                            DateTime.Now - _timestamp < TimeSpan.FromMilliseconds(WaitMilliseconds))
                        {
                            //Delay  
                            _timer.Change(WaitMilliseconds, 0);
                        }
                        else
                        {
                            //Cannot delay! the callback will execute on the pending timeout
                        }
                    }
                }
            }


            private void TimerRelease()
            {
                lock (_locker)
                {
                    //if the timer is not null then a commit has been scheduled
                    if (_timer != null)
                    {
                        //Stop the timer
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer.Dispose();
                        _timer = null;

                        try
                        {
                            //perform the commit
                            CommitNow();

                            // after the commit, refresh the searcher
                            _index.WaitForChanges();
                        }
                        catch (Exception e)
                        {
                            // It is unclear how/why this happens but probably indicates index corruption
                            // see https://github.com/Shazwazza/Examine/issues/164
                            _index.OnIndexingError(new IndexingErrorEventArgs(
                                _index,
                                "An error occurred during the index commit operation, if this error is persistent then index rebuilding is necessary",
                                "-1",
                                e));
                        }
                    }
                }
            }

            protected override void DisposeResources() => TimerRelease();
        }

        /// <summary>
        /// Used to create an index writer - this is called in GetIndexWriter (and therefore, GetIndexWriter should not be overridden)
        /// </summary>
        /// <returns></returns>
        private DirectoryTaxonomyWriter CreateTaxonomyWriterInternal()
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

            DirectoryTaxonomyWriter writer = CreateTaxonomyWriter(dir);

            return writer;
        }
        /// <summary>
        /// Method that creates the IndexWriter
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        protected virtual DirectoryTaxonomyWriter CreateTaxonomyWriter(Directory d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }
            var taxonomyWriter = new DirectoryTaxonomyWriter(d);

            return taxonomyWriter;
        }

        public DirectoryTaxonomyWriter TaxonomyWriter
        {
            get
            {
                EnsureIndex(false);

                if (_taxonomyWriter == null)
                {
                    Monitor.Enter(_writerLocker);
                    try
                    {
                        if (_taxonomyWriter == null)
                        {
                            _taxonomyWriter = CreateTaxonomyWriterInternal();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_writerLocker);
                    }

                }

                return _taxonomyWriter;
            }
        }

        #endregion

        #region Private

        private LuceneTaxonomySearcher CreateSearcher()
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
            DirectoryTaxonomyWriter taxonomyWriter = TaxonomyWriter;
            var searcherManager = new SearcherTaxonomyManager(writer.IndexWriter, true, new SearcherFactory(), taxonomyWriter);
            searcherManager.AddListener(this);
            _nrtReopenThread = new ControlledRealTimeReopenThread<SearcherTaxonomyManager.SearcherAndTaxonomy>(writer, searcherManager, 5.0, 1.0)
            {
                Name = $"{Name} Taxonomy NRT Reopen Thread",
                IsBackground = true
            };

            _nrtReopenThread.Start();

            // wait for most recent changes when first creating the searcher
            WaitForChanges();

            return new LuceneTaxonomySearcher(name + "Searcher", searcherManager, FieldAnalyzer, FieldValueTypeCollection, _options.FacetsConfig);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _nrtReopenThread?.Dispose();
                if (_taxonomyWriter != null)
                {
                    try
                    {
                        _taxonomyWriter?.Dispose();
                    }
                    catch (Exception e)
                    {
                        OnIndexingError(new IndexingErrorEventArgs(this, "Error closing the Taxonomy index", "-1", e));
                    }
                }
            }
        }
    }
}
