using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Search;
using Microsoft.Extensions.Options;

namespace Examine.Lucene.Providers
{
    ///<summary>
    /// A provider that allows for searching across multiple indexes
    ///</summary>
    public class MultiIndexSearcher : BaseLuceneSearcher
    {
        private readonly Lazy<IEnumerable<ISearcher>> _searchers;
        // Pre-materialized array of searchers — evaluated once on first access and reused for
        // every subsequent GetSearchContext() call. Eliminates two LINQ iterator allocations
        // (OfType<BaseLuceneSearcher>() + Select(s => s.GetSearchContext())) per search query.
        private readonly Lazy<BaseLuceneSearcher[]> _luceneSearcherArray;

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        public MultiIndexSearcher(string name, IOptionsMonitor<LuceneMultiSearcherOptions> options, IEnumerable<IIndex> indexes)
            : base(name, options)
        {
            _searchers = new Lazy<IEnumerable<ISearcher>>(() => indexes.Select(x => x.Searcher));
            _luceneSearcherArray = new Lazy<BaseLuceneSearcher[]>(() => _searchers.Value.OfType<BaseLuceneSearcher>().ToArray());
        }

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        public MultiIndexSearcher(string name, IOptionsMonitor<LuceneMultiSearcherOptions> options, Lazy<IEnumerable<ISearcher>> searchers)
            : base(name, options)
        {
            _searchers = searchers;
            _luceneSearcherArray = new Lazy<BaseLuceneSearcher[]>(() => _searchers.Value.OfType<BaseLuceneSearcher>().ToArray());
        }

        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<BaseLuceneSearcher> Searchers => _luceneSearcherArray.Value;

        /// <summary>
        /// Are the searchers initialized
        /// </summary>
        public bool SearchersInitialized => _searchers.IsValueCreated;

        /// <inheritdoc />
        public override void Dispose()
        {
        }

        /// <inheritdoc/>
        public override ISearchContext GetSearchContext()
        {
            // Use the cached array to avoid re-doing OfType<BaseLuceneSearcher>() on every call.
            // For-loop replaces Select(s => s.GetSearchContext()) to eliminate the LINQ SelectIterator allocation.
            var searchers = _luceneSearcherArray.Value;
            var contexts = new ISearchContext[searchers.Length];
            for (var i = 0; i < searchers.Length; i++)
            {
                contexts[i] = searchers[i].GetSearchContext();
            }

            return new MultiSearchContext(contexts);
        }

    }
}
