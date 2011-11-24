using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// A Lucene searcher that uses RAMDirectory
    /// </summary>
    public class LuceneMemorySearcher : LuceneSearcher
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LuceneMemorySearcher()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        public LuceneMemorySearcher(DirectoryInfo workingFolder, Analyzer analyzer)
            : base(workingFolder, analyzer)
        {
        }

        protected override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            return new RAMDirectory(new SimpleFSDirectory(LuceneIndexFolder));
        }

        private IndexSearcher _searcher;

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
        public override Searcher GetSearcher()
        {
            if (_searcher == null)
            {
                 _searcher = new IndexSearcher(GetLuceneDirectory(), true);
            }
            //ensure scoring is turned on for sorting
            _searcher.SetDefaultFieldSortScoring(true, true);
            return _searcher;
        }
    }
}