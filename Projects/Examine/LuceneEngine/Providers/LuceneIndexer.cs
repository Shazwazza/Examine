using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Cru;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Indexing.Filters;
using Examine.LuceneEngine.Indexing.ValueTypes;
using Examine.Providers;
using Examine.Session;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using Examine.LuceneEngine.Config;
using Lucene.Net.Util;
using System.ComponentModel;
using System.Xml;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Abstract object containing all of the logic used to use Lucene as an indexer
    ///</summary>    
    public abstract class LuceneIndexer : BaseIndexProvider, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Constructor used for provider instantiation
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected LuceneIndexer()
        {
            InitFieldTypes(GetDefaultIndexValueTypes());
        }

        /// <summary>
        /// Constructor to create an indexer at runtime
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <param name="facetConfiguration"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer">Specifies the default analyzer to use per field</param>
        /// <param name="indexValueTypes">
        /// Specifies the index value types to use for this indexer, if this is not specified then the result of LuceneIndexer.GetDefaultIndexValueTypes() will be used.
        /// This is generally used to initialize any custom value types for your indexer since the value type collection cannot be modified at runtime.
        /// </param>
        protected LuceneIndexer(
            IEnumerable<FieldDefinition> fieldDefinitions,             
            Lucene.Net.Store.Directory luceneDirectory, 
            Analyzer defaultAnalyzer,
            FacetConfiguration facetConfiguration = null, 
            IDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null)
            : base(fieldDefinitions)
        {
            InitFieldTypes(indexValueTypes ?? GetDefaultIndexValueTypes());

            LuceneIndexFolder = null;
            _directory = luceneDirectory;

            FacetConfiguration = facetConfiguration ?? new FacetConfiguration();

            IndexingAnalyzer = defaultAnalyzer;

            //Create the legacy searcher
            InternalSearcher = new LuceneSearcher(luceneDirectory, IndexingAnalyzer);

            EnsureIndex(false);
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This constructor should no be used, the async flag has no relevance")]
        protected LuceneIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, bool async)
            : this(indexerData, workingFolder, analyzer)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("IIndexCriteria should no longer be used")]
        protected LuceneIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer)
            : base(indexerData)
        {
            InitFieldTypes(GetDefaultIndexValueTypes());

            //set up our folders based on the index path
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));

            IndexingAnalyzer = analyzer;

            //create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
            InternalSearcher = new LuceneSearcher(workingFolder, IndexingAnalyzer);

            EnsureIndex(false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This constructor should no be used, the async flag has no relevance")]
        protected LuceneIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer, bool async)
            : this(indexerData, luceneDirectory, analyzer)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("IIndexCriteria should no longer be used")]
        protected LuceneIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer)
            : base(indexerData)
        {
            InitFieldTypes(GetDefaultIndexValueTypes());

            LuceneIndexFolder = null;
            _directory = luceneDirectory;

            IndexingAnalyzer = analyzer;

            //create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
            InternalSearcher = new LuceneSearcher(luceneDirectory, IndexingAnalyzer);

            EnsureIndex(false);
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist. This will also create an instruction
        /// file declaring the computer name that is part taking in the indexing. This file will then be used to
        /// determine the master indexer machine in a load balanced environment (if one exists).
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            //Need to check if the index set or IndexerData is specified...

            DirectoryInfo workingFolder = null;

            if (config["indexSet"] == null && IndexerData == null)
            {
                //if we don't have either, then we'll try to set the index set by naming conventions
                var found = false;
                if (name.EndsWith("Indexer"))
                {

                    var setNameByConvension = name.Remove(name.LastIndexOf("Indexer")) + "IndexSet";
                    //check if we can assign the index set by naming convention
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set != null)
                    {
                        //we've found an index set by naming conventions :)
                        IndexSetName = set.SetName;

                        var indexSet = IndexSets.Instance.Sets[IndexSetName];

                        //if tokens are declared in the path, then use them (i.e. {machinename} )
                        indexSet.ReplaceTokensInIndexPath();

                        //get the index criteria and ensure folder
                        IndexerData = GetIndexerData(indexSet);

                        //now set the index folders
                        workingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
                        LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));

                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            }
            else if (config["indexSet"] != null)
            {
                //if an index set is specified, ensure it exists and initialize the indexer based on the set

                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                {
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");
                }
                else
                {
                    IndexSetName = config["indexSet"];

                    var indexSet = IndexSets.Instance.Sets[IndexSetName];

                    //if tokens are declared in the path, then use them (i.e. {machinename} )
                    indexSet.ReplaceTokensInIndexPath();

                    //get the index criteria and ensure folder
                    IndexerData = GetIndexerData(indexSet);

                    //now set the index folders
                    workingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
                    LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
                }
            }

            //at this point we must have an index set and we'll setup facet config for it
            //in some cases the config may have been set programatically from code, in that case we'll not 
            //reassign based on config
            if (IndexSets.Instance.Sets[IndexSetName].FacetConfiguration == null || IndexSets.Instance.Sets[IndexSetName].FacetConfiguration.IsEmpty)
            {
                IndexSets.Instance.Sets[IndexSetName].FacetConfiguration =
                    IndexSets.Instance.Sets[IndexSetName].GetFacetConfiguration(this, FacetConfiguration);    
            }
            //then assign it to ourselves
            FacetConfiguration = IndexSets.Instance.Sets[IndexSetName].FacetConfiguration;

            if (config["analyzer"] != null)
            {
                //this should be a fully qualified type
                var analyzerType = Type.GetType(config["analyzer"]);
                IndexingAnalyzer = (Analyzer)Activator.CreateInstance(analyzerType);
            }
            else
            {
                IndexingAnalyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }

            //create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
            InternalSearcher = new LuceneSearcher(workingFolder, IndexingAnalyzer);

            EnsureIndex(false);
        }

        #endregion

        #region Constants & Fields

        /// <summary>
        /// The prefix characters denoting a special field stored in the lucene index for use internally
        /// </summary>
        public const string SpecialFieldPrefix = "__";

        /// <summary>
        /// The prefix added to a field when it is included in the index for sorting
        /// </summary>
        public const string SortedFieldNamePrefix = "__Sort_";

        /// <summary>
        /// Used to store a non-tokenized key for the document
        /// </summary>
        public const string IndexTypeFieldName = "__IndexType";

        /// <summary>
        /// Used to store a non-tokenized type for the document
        /// </summary>
        public const string IndexNodeIdFieldName = "__NodeId";

        /// <summary>
        /// used to thread lock calls for creating and verifying folders
        /// </summary>
        private readonly object _folderLocker = new object();

        private SearcherContext _searcherContext;
        private bool _searcherContextCreated;        
        private object _searchContextCreateLock = new object();

        /// <summary>
        /// Indicates that the index was created
        /// </summary>
        private bool _indexIsNew;

        private Dictionary<string, List<string>> _fieldMappings;
        private object _fieldMappingsLock = new object();
        private bool _fieldMappingCreated;

        /// <summary>
        /// We need an internal searcher used to search against our own index.
        /// This is used for finding all descendant nodes of a current node when deleting indexes.
        /// </summary>
        [Obsolete("This should no longer be used, use the SearcherContext instead to perform any internal searching")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected BaseLuceneSearcher InternalSearcher { get; private set; }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, Func<string, IIndexValueType>> GetDefaultIndexValueTypes()
        {
            return new Dictionary<string, Func<string, IIndexValueType>>
            {
                {"number", name => new Int32Type(name)},
                {FieldDefinitionTypes.Integer.ToLowerInvariant(), name => new Int32Type(name)},
                {FieldDefinitionTypes.Float.ToLowerInvariant(), name => new SingleType(name)},
                {FieldDefinitionTypes.Double.ToLowerInvariant(), name => new DoubleType(name)},
                {FieldDefinitionTypes.Long.ToLowerInvariant(), name => new Int64Type(name)},
                {"date", name => new DateTimeType(name, DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateTime.ToLowerInvariant(), name => new DateTimeType(name, DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateYear.ToLowerInvariant(), name => new DateTimeType(name, DateTools.Resolution.YEAR)},
                {FieldDefinitionTypes.DateMonth.ToLowerInvariant(), name => new DateTimeType(name, DateTools.Resolution.MONTH)},
                {FieldDefinitionTypes.DateDay.ToLowerInvariant(), name => new DateTimeType(name, DateTools.Resolution.DAY)},
                {FieldDefinitionTypes.DateHour.ToLowerInvariant(), name => new DateTimeType(name, DateTools.Resolution.HOUR)},
                {FieldDefinitionTypes.DateMinute.ToLowerInvariant(), name => new DateTimeType(name, DateTools.Resolution.MINUTE)},
                {FieldDefinitionTypes.Raw.ToLowerInvariant(), name => new RawStringType(name)},
                {FieldDefinitionTypes.RawFacet.ToLowerInvariant(), name => new FacetType(name)},
                {FieldDefinitionTypes.Facet.ToLowerInvariant(), name => new FacetType(name).SetSeparator(",")},
                {FieldDefinitionTypes.FacetPath.ToLowerInvariant(), name => new FacetType(name, extractorFactory: () => new TermFacetPathExtractor(name)).SetSeparator(",")},
                {FieldDefinitionTypes.AutoSuggest.ToLowerInvariant(), name => new AutoSuggestType(name) { ValueFilter = new HtmlFilter() }},
                {FieldDefinitionTypes.FullText.ToLowerInvariant(), name => new FullTextType(name) { ValueFilter = new HtmlFilter() }},
                {FieldDefinitionTypes.FullTextSortable.ToLowerInvariant(), name => new FullTextType(name, true) { ValueFilter = new HtmlFilter() }}
            };
        } 

        /// <summary>
        /// Converts a DateTime to total number of milliseconds for storage in a numeric field
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static long DateTimeToTicks(DateTime t)
        {
            return t.Ticks;
        }

        /// <summary>
        /// Converts a DateTime to total number of seconds for storage in a numeric field
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static double DateTimeToSeconds(DateTime t)
        {
            return (t - DateTime.MinValue).TotalSeconds;
        }

        /// <summary>
        /// Converts a DateTime to total number of minutes for storage in a numeric field
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double DateTimeToMinutes(DateTime t)
        {
            return (t - DateTime.MinValue).TotalMinutes;
        }

        /// <summary>
        /// Converts a DateTime to total number of hours for storage in a numeric field
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double DateTimeToHours(DateTime t)
        {
            return (t - DateTime.MinValue).TotalHours;
        }

        /// <summary>
        /// Converts a DateTime to total number of days for storage in a numeric field
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double DateTimeToDays(DateTime t)
        {
            return (t - DateTime.MinValue).TotalDays;
        }

        /// <summary>
        /// Converts a number of milliseconds to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromTicks(long ticks)
        {
            return new DateTime(ticks);
        }

        /// <summary>
        /// Converts a number of seconds to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromSeconds(double seconds)
        {
            return DateTime.MinValue.AddSeconds(seconds);
        }

        /// <summary>
        /// Converts a number of minutes to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromMinutes(double minutes)
        {
            return DateTime.MinValue.AddMinutes(minutes);
        }

        /// <summary>
        /// Converts a number of hours to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="hours"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromHours(double hours)
        {
            return DateTime.MinValue.AddHours(hours);
        }

        /// <summary>
        /// Converts a number of days to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromDays(double days)
        {
            return DateTime.MinValue.AddDays(days);
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets the facet configuration.
        /// </summary>
        /// <value>
        /// The facet configuration.
        /// </value>
        public FacetConfiguration FacetConfiguration { get; private set; }

        ///<summary>
        /// This will automatically optimize the index every 'AutomaticCommitThreshold' commits
        ///</summary>
        [Obsolete("No longer used. Background thread handles optimization")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool AutomaticallyOptimize { get; protected set; }

        /// <summary>
        /// The number of commits to wait for before optimizing the index if AutomaticallyOptimize = true
        /// </summary>
        [Obsolete("No longer used. Background thread handles optimization")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OptimizationCommitThreshold { get; protected internal set; }

        /// <summary>
        /// The default analyzer to use when indexing content, analyers can however be specified on a per field basis
        /// </summary>
        public Analyzer IndexingAnalyzer
        {

            get;

            protected set;
        }

        /// <summary>
        /// Used to keep track of how many index commits have been performed.
        /// This is used to determine when index optimization needs to occur.
        /// </summary>
        [Obsolete("This is no longer used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CommitCount { get; protected internal set; }

        /// <summary>
        /// Indicates whether or this system will process the queue items asynchonously. Default is true.
        /// </summary>
        [Obsolete("Items are added synchroniously and commits and reopens are handled asynchroniously")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool RunAsync { get; protected internal set; }

        /// <summary>
        /// The folder that stores the Lucene Index files
        /// </summary>
        public DirectoryInfo LuceneIndexFolder { get; private set; }

        /// <summary>
        /// The base folder that contains the queue and index folder and the indexer executive files
        /// </summary>
        [Obsolete("This is no longer used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DirectoryInfo WorkingFolder { get; private set; }

        /// <summary>
        /// The index set name which references an Examine <see cref="IndexSet"/>
        /// </summary>
        public string IndexSetName { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [index optimizing].
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is no longer used, index optimization is no longer managed with the LuceneIndexer")]
        public event EventHandler IndexOptimizing;

        ///<summary>
        /// Occurs when the index is finished optmizing
        ///</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is no longer used, index optimization is no longer managed with the LuceneIndexer")]
        public event EventHandler IndexOptimized;

        /// <summary>
        /// Fires once an index operation is completed
        /// </summary>
        public event EventHandler IndexOperationComplete;

        /// <summary>
        /// Occurs when [document writing].
        /// </summary>
        public event EventHandler<DocumentWritingEventArgs> DocumentWriting;

        #endregion

        #region Event handlers

        /// <summary>
        /// Called when an indexing error occurs
        /// </summary>
        /// <param name="e"></param>
        /// <param name="resetIndexingFlag">set to true if the IsIndexing flag should be reset (set to false) so future indexing operations can occur</param>
        [Obsolete("This no longer performs any function, the resetIndexingFlag has no affect")]
        protected void OnIndexingError(IndexingErrorEventArgs e, bool resetIndexingFlag)
        {
            OnIndexingError(e);
        }

        /// <summary>
        /// Called when an indexing error occurs
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            base.OnIndexingError(e);

            //TODO: Maybe this exception shouldn't propagate to the user directly.

            var msg = "Indexing Error Occurred: " + e.Message;
            if (e.InnerException != null)
                msg += ". ERROR: " + e.InnerException.Message;
            throw new Exception(msg, e.InnerException);

        }

        protected virtual void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            if (DocumentWriting != null)
                DocumentWriting(this, docArgs);
        }

        [Obsolete("This is no longer used, index optimization is no longer managed with the LuceneIndexer")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnIndexOptimizing(EventArgs e)
        {
            if (IndexOptimizing != null)
                IndexOptimizing(this, e);
        }

        [Obsolete("This is no longer used, index optimization is no longer managed with the LuceneIndexer")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnIndexOptimized(EventArgs e)
        {
            if (IndexOptimized != null)
                IndexOptimized(this, e);
        }

        protected virtual void OnIndexOperationComplete(EventArgs e)
        {
            if (IndexOperationComplete != null)
                IndexOperationComplete(this, e);
        }

        [Obsolete("This is no longer relavent and not used, duplicate fields are allowed")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnDuplicateFieldWarning(int nodeId, string indexSetName, string fieldName) { }

        #endregion

        #region Provider implementation

        public override void IndexItems(params ValueSet[] nodes)
        {
            //check if the index doesn't exist, and if so, create it and reindex everything, this will obviously index this
            //particular node
            if (WasIndexCreated())
            {
                RebuildIndex();
                return;
            }

            foreach (var node in nodes)
            {
                ProcessIndexOperation(new IndexOperation()
                {
                    Operation = IndexOperationType.Add,
                    Item = new IndexItem(node)
                });
            }
        }
        
        /// <summary>
        /// Returns the current SearcherContext
        /// </summary>
        public SearcherContext SearcherContext
        {
            get
            {
                if (_searcherContext == null)
                {
                    throw new InvalidOperationException("The index has not been initialized, not SearcherContext is available");
                }
                return _searcherContext;
            }
        }
        
        /// <summary>
        /// Returns true if the index has just been created.
        /// On later requests it will return false
        /// </summary>
        /// <returns></returns>
        protected bool WasIndexCreated()
        {
            if (_indexIsNew)
            {
                _indexIsNew = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a brand new index, this will override any existing index with an empty one
        /// </summary>
        public void EnsureIndex(bool forceOverwrite)
        {
            LazyInitializer.EnsureInitialized(ref _searcherContext, ref _searcherContextCreated, ref _searchContextCreateLock, () =>
            {
                _indexIsNew = IndexExists();

                //TODO: Test what happens if someone actually wires two indexers to the same index set.
                var searcherContext = SearcherContextCollection.Instance.GetContext(GetLuceneDirectory());
                if (searcherContext == null)
                {
                    SearcherContextCollection.Instance.RegisterContext(
                        searcherContext =
                        new SearcherContext(GetLuceneDirectory(), IndexingAnalyzer, FacetConfiguration));

                    searcherContext.Manager.Tracker = ExamineSession.TrackGeneration;
                }

                InitializeFields(searcherContext);

                return searcherContext;
            });
            

            if (forceOverwrite)
            {
                _searcherContext.Manager.DeleteAll();
            }
        }

        /// <summary>
        /// This initializes all of the defined value types on the searcher context for the fields defined on this indexer
        /// </summary>
        private void InitializeFields(SearcherContext searcherContext)
        {
            //perform the operation for all new field definitions
            foreach (var field in FieldDefinitions)
            {
                Func<string, IIndexValueType> valueType;
                if (!string.IsNullOrWhiteSpace(field.Type) && IndexFieldTypes.TryGetValue(field.Type, out valueType))
                {
                    searcherContext.DefineValueType(
                        valueType(string.IsNullOrWhiteSpace(field.IndexName) ? field.Name : field.IndexName));
                }
                else
                {
                    //Define the default!
                    var fulltext = IndexFieldTypes["fulltext"];
                    searcherContext.DefineValueType(
                        fulltext(string.IsNullOrWhiteSpace(field.IndexName) ? field.Name : field.IndexName));
                }
            }
            
            //perform the operation for all legacy declared fields - We don't have the 'IndexName' 
            // on legacy field types
            foreach (var field in IndexerData.AllFields())
            {
                Func<string, IIndexValueType> valueType;
                if (!string.IsNullOrWhiteSpace(field.Type) && IndexFieldTypes.TryGetValue(field.Type, out valueType))
                {
                    searcherContext.DefineValueType(valueType(field.Name));
                }
                else
                {
                    //Define the default!
                    var fulltext = IndexFieldTypes["fulltext"];
                    searcherContext.DefineValueType(fulltext(field.Name));
                }
            }
        }

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        /// <remarks>This will completely delete the index and recreate it</remarks>
        public override void RebuildIndex()
        {
            EnsureIndex(true);

            //call abstract method
            PerformIndexRebuild();
        }

        /// <summary>
        /// Deletes a node from the index.                
        /// </summary>       
        /// <param name="nodeId">ID of the node to delete</param>
        public override void DeleteFromIndex(string nodeId)
        {
            DeleteFromIndex(new Term(IndexNodeIdFieldName, nodeId), false);
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public override void IndexAll(string type)
        {
            //check if the index doesn't exist, and if so, create it and reindex everything
            if (WasIndexCreated())
            {
                RebuildIndex();
                return;
            }

            //Here we just need to delete all items of a type....
            DeleteFromIndex(new Term(IndexTypeFieldName, type), false);

            //now do the indexing...
            PerformIndexAll(type);
        }

        #endregion

        /// <summary>
        /// This wil optimize the index for searching, this gets executed when this class instance is instantiated.
        /// </summary>
        /// <remarks>
        /// This can be an expensive operation and should only be called when there is no indexing activity
        /// </remarks>
        public void OptimizeIndex()
        {
            EnsureIndex(false);

            SearcherContext.Committer.OptimizeNow();

            //TODO: Hook into searchcontexts comitter thread to optimize
        }

        #region Protected

        /// <summary>
        /// This will add a number of nodes to the index
        /// </summary>        
        /// <param name="nodes"></param>
        /// <param name="type"></param>
        [Obsolete("Do not use this, use the ValueSet override instead")]
        protected void AddNodesToIndex(IEnumerable<XElement> nodes, string type)
        {
            IndexItems(nodes.Select(n => n.ToValueSet(type, n.ExamineNodeTypeAlias())).ToArray());
        }

        /// <summary>
        /// Called to perform the operation to do the actual indexing of an index type after the lucene index has been re-initialized.
        /// </summary>
        /// <param name="category"></param>
        protected abstract void PerformIndexAll(string category);

        /// <summary>
        /// Called to perform the actual rebuild of the indexes once the lucene index has been re-initialized.
        /// </summary>
        protected abstract void PerformIndexRebuild();

        /// <summary>
        /// Overrideable method used to get the indexer data during provider initialization
        /// </summary>
        /// <param name="indexSet"></param>
        protected virtual IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return new IndexCriteria(
                indexSet.IndexAttributeFields.Cast<IIndexField>().ToArray(),
                indexSet.IndexUserFields.Cast<IIndexField>().ToArray(),
                indexSet.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.IndexParentId) { FacetConfiguration = indexSet.FacetConfiguration };
        }

        /// <summary>
        /// Checks if the index is ready to open/write to.
        /// </summary>
        /// <returns></returns>
        protected bool IndexReady()
        {
            return (!IndexWriter.IsLocked(GetLuceneDirectory()));
        }

        /// <summary>
        /// Check if there is an index in the index folder
        /// </summary>
        /// <returns></returns>
        public override bool IndexExists()
        {
            return IndexReader.IndexExists(GetLuceneDirectory());
        }

        /// <summary>
        /// Indicate if the index is new or not
        /// </summary>
        /// <returns></returns>
        public override bool IsIndexNew()
        {
            var baseNew = base.IsIndexNew();
            if (!baseNew)
            {
                var sc = _searcherContext;
                if (sc != null)
                {
                    using (var s = sc.GetSearcher())
                    {
                        return s.Searcher.IndexReader.NumDocs() == 0;
                    }
                }
                else
                {
                    return true;
                }
            }

            return baseNew;
        }

        /// <summary>
        /// Adds single node to index. If the node already exists, a duplicate will probably be created,
        /// To re-index, use the ReIndexNode method.
        /// </summary>
        /// <param name="node">The node to index.</param>
        /// <param name="type">The type to store the node as.</param>
        [Obsolete("Use ValueSets instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void AddSingleNodeToIndex(XElement node, string type)
        {
            AddNodesToIndex(new XElement[] { node }, type);
        }

        /// <summary>
        /// Removes the specified term from the index
        /// </summary>
        /// <param name="indexTerm"></param>
        /// <param name="performCommit">Obsolete. Doesn't have any effect</param>
        /// <returns>Boolean if it successfully deleted the term, or there were on errors</returns>        
        protected bool DeleteFromIndex(Term indexTerm, bool performCommit = true)
        {
            long nodeId = -1;
            if (indexTerm != null && indexTerm.Field == "id")
                long.TryParse(indexTerm.Text, out nodeId);

            try
            {
                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                if (indexTerm == null)
                {
                    SearcherContext.Manager.DeleteAll();
                }
                else
                {
                    SearcherContext.Manager.DeleteDocuments(indexTerm);
                }


                OnIndexDeleted(new DeleteIndexEventArgs(new KeyValuePair<string, string>(indexTerm == null ? "" : indexTerm.Field, indexTerm == null ? "" : indexTerm.Text)));
                return true;
            }
            catch (Exception ee)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error deleting Lucene index", (int)(nodeId > int.MaxValue ? int.MaxValue : nodeId), ee));
                return false;
            }
        }

        /// <summary>
        /// Ensures that the node being indexed is of a correct type 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual bool ValidateDocument(ValueSet node)
        {
            //check if this document is of a correct type of node type alias
            if (IndexerData.IncludeNodeTypes.Any())
                if (!IndexerData.IncludeNodeTypes.Contains(node.ItemType))
                    return false;

            //if this node type is part of our exclusion list, do not validate
            if (IndexerData.ExcludeNodeTypes.Any())
                if (IndexerData.ExcludeNodeTypes.Contains(node.ItemType))
                    return false;

            return true;
        }

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [Obsolete("This method is no longer used and will be removed in future versions, do not call this method")]
        protected virtual bool ValidateDocument(XElement node)
        {
            return ValidateDocument(node.ToValueSet(null, node.ExamineNodeTypeAlias()));
        }

        /// <summary>
        /// Called before the item is put into the index and filters/formats the data
        /// </summary>
        /// <param name="indexItem"></param>
        protected internal virtual void TransformDataToIndex(IndexItem indexItem)
        {
            //copy all original valus here so we can give them to the transform values event
            var originalValues = new Dictionary<string, IEnumerable<object>>(
                indexItem.ValueSet.Values.ToDictionary(pair => pair.Key, pair => (IEnumerable<object>)pair.Value));

            var valueSet = indexItem.ValueSet;

            //if the index critera does not list any fields whatsoever, than we will let
            // all fields be indexed.
            if (IndexerData.AllFields().Any())
            {
                //remove any field that is not declared
                var toRemove = valueSet.Values.Keys.Except(
                    IndexerData.AllFields().Select(x => x.Name))
                    .ToArray();
                foreach (var r in toRemove)
                {
                    valueSet.Values.Remove(r);
                }
            }

            // handle legacy field event for all fields
            foreach (var field in IndexerData.AllFields())
            {
                if (valueSet.Values.ContainsKey(field.Name))
                {
                    HandleLegacyFieldEvent(field, valueSet, true);
                }
            }

            //Now do the legacy stuff.
            //this will call the legacy method to get the returned filtered data to index, we then 
            // need to align the actual field data with the results:
            // * remove any fields that are not contained in the results
            // * update any fields that have changed values
            // * add any fields that don't exist
            var fieldResult = GetDataToIndex(indexItem.DataToIndex, indexItem.IndexType);
            var legacyVals = valueSet.ToLegacyFields();
            foreach (var v in fieldResult)
            {
                string val;
                if (!legacyVals.TryGetValue(v.Key, out val))
                {
                    //if the value doesn't exist in the real values, then add it
                    valueSet.Add(v.Key, v.Value);
                }
                else if (val != v.Value)
                {
                    //if the value has changed, then change it
                    valueSet.Values.Remove(v.Key);
                    valueSet.Add(v.Key, v.Value);
                }
            }

            //Ok, now that the legacy stuff is done, we can emit our new events
            OnTransformingIndexValues(new TransformingIndexDataEventArgs(indexItem.ValueSet, originalValues));
        }

        /// <summary>
        /// Used to deal with the legacy events
        /// </summary>
        /// <param name="field"></param>
        /// <param name="valueSet"></param>
        /// <param name="isStandardField"></param>
        private void HandleLegacyFieldEvent(IIndexField field, ValueSet valueSet, bool isStandardField)
        {
            var fieldVals = valueSet.Values.ContainsKey(field.Name)
                                    ? valueSet.Values[field.Name].ToArray()
                                    : new object[] { };

            //here we need to support the legacy events - which only supports one value so we need to perform some trickery here.                
            int intId;
            try
            {
                intId = Convert.ToInt32(valueSet.Id);
            }
            catch (Exception)
            {
                //if that cannot be converted we'll just skip it
                return;
            }
            //legacy events only support one value so use the first
            var singleVal = fieldVals.Any() ? fieldVals.First().ToString() : "";
            var asXml = valueSet.ToExamineXml();
            var args = new IndexingFieldDataEventArgs(asXml, field.Name, singleVal, isStandardField, intId);
            OnGatheringFieldData(args);
            //update the first field
            if (fieldVals.Any())
            {
                fieldVals[0] = args.FieldValue;
            }
            else if (!string.IsNullOrEmpty(args.FieldValue))
            {
                //it didn't originally exist so add it
                valueSet.Values.Add(field.Name, new List<object> { args.FieldValue });
            }
        }

        /// <summary>
        /// This now just raises the legacy event
        /// </summary>      
        [Obsolete("This should no longer be used, use the TransformDataToIndex method instead with the ValueSet data")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            var values = new Dictionary<string, string>();

            var nodeId = int.Parse(node.Attribute("id").Value);

            //raise the event and assign the value to the returned data from the event
            var indexingNodeDataArgs = new IndexingNodeDataEventArgs(node, nodeId, values, type);

            OnGatheringNodeData(indexingNodeDataArgs);

            values = indexingNodeDataArgs.Fields;

            return values;
        }

        [Obsolete("This is no longer used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual FieldIndexTypes GetPolicy(string fieldName)
        {
            return FieldIndexTypes.ANALYZED;
        }
        
        /// <summary>
        /// Returns the index field names for the source item name
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        protected virtual IEnumerable<string> GetIndexFieldNames(string sourceName)
        {
            LazyInitializer.EnsureInitialized(ref _fieldMappings, ref _fieldMappingCreated, ref _fieldMappingsLock, () =>
            {
                List<string> mappings;

                var fieldMappings = new Dictionary<string, List<string>>();

                //iterate over field definitions
                foreach (var f in FieldDefinitions)
                {
                    //TODO: This here is some zany logic, still trying to figure out what it is doing, the purpose
                    // of resetting the mappings variable with 'out' parmams and how the 'IndexName' get's used.

                    if (!fieldMappings.TryGetValue(f.Name, out mappings))
                    {
                        fieldMappings.Add(f.Name, mappings = new List<string>());
                    }

                    mappings.Add(
                        string.IsNullOrWhiteSpace(f.IndexName)
                            ? f.Name
                            : f.IndexName != f.Name
                                ? f.IndexName
                                : f.Name);
                }

                //iterate over legacy fields - legacy fields do not have an 'IndexName'
                foreach (var f in IndexerData.AllFields())
                {
                    //TODO: This here is some zany logic, still trying to figure out what it is doing, the purpose
                    // of resetting the mappings variable with 'out' parmams

                    if (!fieldMappings.TryGetValue(f.Name, out mappings))
                    {
                        fieldMappings.Add(f.Name, mappings = new List<string>());
                    }
                    mappings.Add(f.Name);
                }

                return fieldMappings;
            });

            List<string> mappings2;
            return _fieldMappings.TryGetValue(sourceName, out mappings2) ? (IEnumerable<string>)mappings2 : new[] { sourceName };
        }

        /// <summary>
        /// The final step to adding a Lucene document
        /// </summary>
        /// <param name="values"></param>
        protected virtual void AddDocument(ValueSet values)
        {
            var args = new IndexingNodeEventArgs(values);
            OnNodeIndexing(args);
            if (args.Cancel)
                return;

            var d = new Document();

            d.Add(new ExternalIdField(values.Id));

            var sc = SearcherContext;
            foreach (var value in values.Values)
            {
                foreach (var mapping in GetIndexFieldNames(value.Key))
                {
                    var type = sc.GetValueType(mapping, true);
                    foreach (var val in value.Value)
                    {
                        type.AddValue(d, val);
                    }
                }
            }

            AddSpecialFieldsToDocument(d, values);

            var docArgs = new DocumentWritingEventArgs(d, values);
            OnDocumentWriting(docArgs);
            if (docArgs.Cancel)
                return;

            SearcherContext.Manager.UpdateDocument(new Term(IndexNodeIdFieldName, values.Id.ToString(CultureInfo.InvariantCulture)), d);

            OnNodeIndexed(new IndexedNodeEventArgs(values.Id));
        }


        /// <summary>
        /// Collects the data for the fields and adds the document which is then committed into Lucene.Net's index
        /// </summary>
        /// <param name="fields">The fields and their associated data.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="type">The type to index the node as.</param>
        /// <remarks>
        /// This will normalize (lowercase) all text before it goes in to the index.
        /// </remarks>
        [Obsolete("Use the ValueSet override instead, this method should not be used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void AddDocument(Dictionary<string, string> fields, int nodeId, string type)
        {
            //we don't have enough info here to create a real ValueSet - missing the item type
            AddDocument(ValueSet.FromLegacyFields(nodeId, type, null, fields));
        }

        /// <summary>
        /// Returns a dictionary of special key/value pairs to store in the lucene index which will be stored by:
        /// - Field.Store.YES
        /// - Field.Index.NOT_ANALYZED_NO_NORMS
        /// - Field.TermVector.NO
        /// </summary>
        /// <param name="allValuesForIndexing">
        /// The dictionary object containing all name/value pairs that are to be put into the index
        /// </param>
        /// <returns></returns>
        [Obsolete("Every field is special in its own way. Don't use this method.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual Dictionary<string, string> GetSpecialFieldsToIndex(Dictionary<string, string> allValuesForIndexing)
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Processes the index operation and validates the item
        /// </summary>
        /// <param name="item"></param>
        protected void ProcessIndexOperation(IndexOperation item)
        {
            switch (item.Operation)
            {
                case IndexOperationType.Add:
                    if (ValidateDocument(item.Item.ValueSet))
                    {
                        var node = ProcessIndexItem(item.Item);
                        OnNodesIndexed(new IndexedNodesEventArgs(IndexerData, new[] { node }));
                    }
                    else
                    {
                        OnIgnoringNode(new IndexingNodeDataEventArgs(item.Item.ValueSet));
                    }
                    break;
                case IndexOperationType.Delete:
                    ProcessDeleteItem(item.Item, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Obsolete("This is no longer used, use ProcessIndexOperation instead")]
        protected void EnqueueIndexOperation(IndexOperation op)
        {
            ProcessIndexOperation(op);
        }

        private Lucene.Net.Store.Directory _directory;

        /// <summary>
        /// Returns the Lucene Directory used to store the index
        /// </summary>
        /// <returns></returns>

        public virtual Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            if (_directory == null)
            {
                lock (_folderLocker)
                {
                    VerifyFolder(LuceneIndexFolder);
                    _directory = new SimpleFSDirectory(LuceneIndexFolder);
                }
            }
            return _directory;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this method, though it will return an IndexWriter, it should never be used directly with the new NrtManager")]
        public virtual IndexWriter GetIndexWriter()
        {
            return SearcherContext.GetWriter();
        }

        #endregion

        #region Private

        /// <summary>
        /// Adds 'special' fields to the Lucene index for use internally.
        /// By default this will add the __IndexType & __NodeId fields to the Lucene Index both specified by:
        /// - Field.Store.YES
        /// - Field.Index.NOT_ANALYZED_NO_NORMS
        /// - Field.TermVector.NO
        /// </summary>
        /// <param name="d"></param>
        /// <param name="values"></param>
        private void AddSpecialFieldsToDocument(Document d, ValueSet values)
        {
            //TODO: These should be added using a value base type? I.E. We shouldn't need the ToLower bits

            d.Add(new Field(IndexNodeIdFieldName, values.Id + "", Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
            d.Add(new Field(IndexTypeFieldName, values.IndexCategory.ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
        }

        private void ProcessDeleteItem(IndexItem item, bool performCommit = true)
        {
            //if type and id are empty remove it all
            if (string.IsNullOrEmpty(item.Id) && string.IsNullOrEmpty(item.IndexType))
            {
                DeleteFromIndex(null, performCommit);
            }
            //if the id is empty then remove the whole type
            else if (string.IsNullOrEmpty(item.Id))
            {
                DeleteFromIndex(new Term(IndexTypeFieldName, item.IndexType), performCommit);
            }
            else
            {
                DeleteFromIndex(new Term(IndexNodeIdFieldName, item.Id), performCommit);
            }
        }

        private IndexedNode ProcessIndexItem(IndexItem item)
        {
            //get the node id
            //TODO: Fix this - it might not be int?
            var nodeId = int.Parse(item.Id);

            //transform the data
            TransformDataToIndex(item);

            var values = item.ValueSet;

            AddDocument(values);

            return new IndexedNode() { NodeId = nodeId, Type = item.IndexType };
        }

        /// <summary>
        /// Creates the folder if it does not exist.
        /// </summary>
        /// <param name="folder"></param>
        private void VerifyFolder(DirectoryInfo folder)
        {

            if (!System.IO.Directory.Exists(folder.FullName))
            {
                folder.Create();
                folder.Refresh();
            }

        }

        private void InitFieldTypes(IEnumerable<KeyValuePair<string, Func<string, IIndexValueType>>> types)
        {
            foreach (var type in types)
            {
                IndexFieldTypes.TryAdd(type.Key, type.Value);
            }
        }

        #endregion

        /// <summary>
        /// Defines the field types such as number, fulltext, etc...
        /// </summary>
        /// <remarks>
        /// Makes concurrent dictionary becaues this is a singleton - though I don't think this collection is ever modified
        /// after construction but we'll leave it like this anyways.
        /// </remarks>
        internal ConcurrentDictionary<string, Func<string, IIndexValueType>> IndexFieldTypes = new ConcurrentDictionary<string, Func<string, IIndexValueType>>(StringComparer.InvariantCultureIgnoreCase);
        
        #region IDisposable Members

        protected bool Disposed;

        /// <summary>
        /// Checks the disposal state of the objects
        /// </summary>
        protected void CheckDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException("LuceneExamine.BaseLuceneExamineIndexer");
        }

        /// <summary>
        /// When the object is disposed, all data should be written
        /// </summary>
        public void Dispose()
        {
            this.CheckDisposed();
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.Disposed = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.CheckDisposed();
            if (disposing)
            {
                //TODO: Should we dispose anything ?
                // the SearcherContext basically contains all elements but what disposes that apart from the Examine Manager? what about tests?

                //_isCancelling = true;
                //this._fileWatcher.Dispose();
            }

        }

        #endregion
    }
}
