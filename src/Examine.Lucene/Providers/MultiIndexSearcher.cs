using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace Examine.Lucene.Providers
{
    ///<summary>
    /// A provider that allows for searching across multiple indexes
    ///</summary>
    public class MultiIndexSearcher : BaseLuceneSearcher
    {
        private readonly Lazy<IEnumerable<ISearcher>> _searchers;


        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="indexes"></param>
        /// <param name="analyzer"></param>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public MultiIndexSearcher(string name, IEnumerable<IIndex> indexes, Analyzer analyzer = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            : base(name, analyzer ?? new StandardAnalyzer(LuceneInfo.CurrentVersion))
        {
            _searchers = new Lazy<IEnumerable<ISearcher>>(() => indexes.Select(x => x.Searcher));
        }

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchers"></param>
        /// <param name="analyzer"></param>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public MultiIndexSearcher(string name, Lazy<IEnumerable<ISearcher>> searchers, Analyzer analyzer = null)
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            : base(name, analyzer ?? new StandardAnalyzer(LuceneInfo.CurrentVersion))
        {
            _searchers = searchers;
        }

        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<LuceneSearcher> Searchers => _searchers.Value.OfType<LuceneSearcher>();

        // for tests
        public bool SearchersInitialized => _searchers.IsValueCreated;

        public override ISearchContext GetSearchContext()
            => new MultiSearchContext(Searchers.Select(s => s.GetSearchContext()).ToArray());

    }
}
