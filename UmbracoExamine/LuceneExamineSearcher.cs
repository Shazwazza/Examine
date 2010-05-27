using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using UmbracoExamine.Config;
using UmbracoExamine.SearchCriteria;

namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
    public class LuceneExamineSearcher : BaseSearchProvider
    {

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public LuceneExamineSearcher(): base(){}
        
        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        public LuceneExamineSearcher(DirectoryInfo indexPath)
            : base()
        {           
            //set up our folder
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(indexPath.FullName, "Index"));
        }

        #endregion

        /// <summary>
        /// Used as a singleton instance
        /// </summary>
        private static IndexSearcher m_Searcher;
        private static readonly object m_Locker = new object();

        /// <summary>
        /// Initializes the provider.
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

            //need to check if the index set is specified, if it's not, we'll see if we can find one by convension
            //if the folder is not null and the index set is null, we'll assume that this has been created at runtime.
            if (config["indexSet"] == null && LuceneIndexFolder == null)
            {
                //if we don't have either, then we'll try to set the index set by naming convensions
                var found = false;
                if (name.EndsWith("Searcher"))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf("Searcher")) + "IndexSet";
                    //check if we can assign the index set by naming convension
                    var set = ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>()
                        .Where(x => x.SetName == setNameByConvension)
                        .SingleOrDefault();

                    if (set != null)
                    {
                        //we've found an index set by naming convensions :)
                        IndexSetName = set.SetName;
                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration");

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
            }
            else if (config["indexSet"] != null)
            {
                if (ExamineLuceneIndexes.Instance.Sets[config["indexSet"]] == null)
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

                IndexSetName = config["indexSet"];

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
            }            



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
            
        }

        /// <summary>
        /// Directory where the Lucene.NET Index resides
        /// </summary>
        public DirectoryInfo LuceneIndexFolder
        {
            get
            {
                if (m_IndexFolder == null)
                {
                    return null;
                }

                //ensure's that it always up to date!
                m_IndexFolder.Refresh();
                return m_IndexFolder;
            }
            protected internal set
            {
                m_IndexFolder = value;
            }
        }
        
        /// <summary>
        /// Do not access this object directly. The public property ensures that the folder state is always up to date
        /// </summary>
        private DirectoryInfo m_IndexFolder;

        /// <summary>
        /// The analyzer to use when searching content, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer IndexingAnalyzer { get; protected internal set; }

        /// <summary>
        /// Name of the Lucene.NET index set
        /// </summary>
        public string IndexSetName { get; protected internal set; }

        /// <summary>
        /// Simple search method which defaults to searching content nodes
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public override ISearchResults Search(string searchText, bool useWildcards)
        {
            var sc = this.CreateSearchCriteria(IndexType.Content);

            if (useWildcards)
            {
                sc = sc.GroupedOr(GetSearchFields(), searchText.MultipleCharacterWildcard().Value).Compile();
            }
            else
            {
                sc = sc.GroupedOr(GetSearchFields(), searchText).Compile();
            }

            return Search(sc);
        }

        /// <summary>
        /// Searches the data source using the Examine Fluent API
        /// </summary>
        /// <param name="searchParams">The fluent API search.</param>
        /// <returns></returns>
        public override ISearchResults Search(ISearchCriteria searchParams)
        {
            ValidateSearcher(false);

            Enforcer.ArgumentNotNull(searchParams, "searchParams");

            var luceneParams = searchParams as LuceneSearchCriteria;
            if (luceneParams == null)
                throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

            if (!LuceneIndexFolder.Exists)
                throw new DirectoryNotFoundException("No index found at the location specified. Ensure that an index has been created");

            var pagesResults = new SearchResults(luceneParams.query, luceneParams.sortFields, m_Searcher);
            return pagesResults;
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <returns>A blank SearchCriteria</returns>
        public override ISearchCriteria CreateSearchCriteria(IndexType type)
        {
            return this.CreateSearchCriteria(type, BooleanOperation.And);
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        public override ISearchCriteria CreateSearchCriteria(IndexType type, BooleanOperation defaultOperation)
        {
            return new LuceneSearchCriteria(type, this.IndexingAnalyzer, this.GetSearchFields(), defaultOperation);
        }

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
        public IndexSearcher GetSearcher()
        {
            ValidateSearcher(false);
            return m_Searcher;
        }

        #region Private

        private string[] GetSearchFields()
        {
            var searcher = GetSearcher();
            return GetSearchFields(searcher.GetIndexReader());
        }

        private static string[] GetSearchFields(IndexReader reader)
        {
            var fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
            //exclude the special index fields
            var searchFields = fields
                .Where(x => x != LuceneExamineIndexer.IndexNodeIdFieldName && x != LuceneExamineIndexer.IndexTypeFieldName)
                .ToArray();
            return searchFields;
        }

        private enum ReaderStatus { Current, Closed, NotCurrent }

        /// <summary>
        /// Checks if the reader is current, closed or not up to date
        /// </summary>
        /// <returns>The reader status</returns>
        /// <remarks>
        /// Performs error checking as the reader may be closed
        /// </remarks>
        private ReaderStatus GetReaderStatus()
        {
            ReaderStatus status = ReaderStatus.NotCurrent;
            try
            {
                status = m_Searcher.GetIndexReader().IsCurrent() ? ReaderStatus.Current : ReaderStatus.NotCurrent;
            }
            catch (AlreadyClosedException)
            {
                status = ReaderStatus.Closed;
            }
            return status;
        }

        /// <summary>
        /// This checks if the singleton IndexSearcher is initialized and up to date.
        /// </summary>
        /// <param name="forceReopen"></param>
        internal void ValidateSearcher(bool forceReopen)
        {
            if (!forceReopen)
            {
                if (m_Searcher == null)
                {
                    lock (m_Locker)
                    {
                        //double check
                        if (m_Searcher == null)
                        {

                            try
                            {
                                m_Searcher = new IndexSearcher(new SimpleFSDirectory(LuceneIndexFolder), true);
                            }
                            catch (IOException ex)
                            {
                                throw new ApplicationException("There is no Lucene index in the folder: " + LuceneIndexFolder.FullName + " to create an IndexSearcher on", ex);
                            }
                        }
                    }
                }
                else
                {
                    if (GetReaderStatus() != ReaderStatus.Current)
                    {
                        lock (m_Locker)
                        {
                            //double check, now, we need to find out if it's closed or just not current
                            switch (GetReaderStatus())
                            {
                                case ReaderStatus.Current:
                                    break;
                                case ReaderStatus.Closed:
                                    m_Searcher = new IndexSearcher(new SimpleFSDirectory(LuceneIndexFolder), true);
                                    break;
                                case ReaderStatus.NotCurrent:

                                    //yes, this is actually the way the Lucene wants you to work...
                                    //normally, i would have thought just calling Reopen() on the underlying reader would suffice... but it doesn't.
                                    //here's references: 
                                    // http://stackoverflow.com/questions/1323779/lucene-indexreader-reopen-doesnt-seem-to-work-correctly
                                    // http://gist.github.com/173978 

                                    var oldReader = m_Searcher.GetIndexReader();
                                    var newReader = oldReader.Reopen(true);
                                    if (newReader != oldReader)
                                    {
                                        m_Searcher.Close();
                                        oldReader.Close();
                                        m_Searcher = new IndexSearcher(newReader);
                                    }

                                    break;
                            }                
                        }
                    }
                }
            }
            else
            {            
                //need to close the searcher and force a re-open

                if (m_Searcher != null)
                {
                    lock (m_Locker)
                    {
                        //double check
                        if (m_Searcher != null)
                        {
                            try
                            {
                                m_Searcher.Close();
                            }
                            catch (IOException)
                            {
                                //this will happen if it's already closed ( i think )
                            }
                            finally
                            {
                                //set to null in case another call to this method has passed the first lock and is checking for null
                                m_Searcher = null;
                            }

                            m_Searcher = new IndexSearcher(new SimpleFSDirectory(LuceneIndexFolder), true); 
                        }
                    }
                }
            }
            
        }

        #endregion
    }
}
