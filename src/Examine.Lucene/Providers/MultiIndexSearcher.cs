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
        // Pre-materialized array of LuceneSearchers — evaluated once on first access and reused for
        // every subsequent GetSearchContext() call. Eliminates two LINQ iterator allocations
        // (OfType<LuceneSearcher>() + Select(s => s.GetSearchContext())) per search query.
        private readonly Lazy<LuceneSearcher[]> _luceneSearcherArray;


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
            _luceneSearcherArray = new Lazy<LuceneSearcher[]>(() => _searchers.Value.OfType<LuceneSearcher>().ToArray());
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
            _luceneSearcherArray = new Lazy<LuceneSearcher[]>(() => _searchers.Value.OfType<LuceneSearcher>().ToArray());
        }

        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<LuceneSearcher> Searchers => _luceneSearcherArray.Value;

        // for tests
        public bool SearchersInitialized => _searchers.IsValueCreated;

        public override ISearchContext GetSearchContext()
        {
            // Use the cached array to avoid re-doing OfType<LuceneSearcher>() on every call.
            // For-loop replaces Select(s => s.GetSearchContext()) to eliminate the LINQ SelectIterator allocation.
            var searchers = _luceneSearcherArray.Value;
            var contexts = new ISearchContext[searchers.Length];
            for (var i = 0; i < searchers.Length; i++)
                contexts[i] = searchers[i].GetSearchContext();
            return new MultiSearchContext(contexts);
        }

    }
}
