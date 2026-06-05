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

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        public MultiIndexSearcher(string name, IOptionsMonitor<LuceneMultiSearcherOptions> options, IEnumerable<IIndex> indexes)
            : base(name, options)
        {
            _searchers = new Lazy<IEnumerable<ISearcher>>(() => indexes.Select(x => x.Searcher));
        }

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        public MultiIndexSearcher(string name, IOptionsMonitor<LuceneMultiSearcherOptions> options, Lazy<IEnumerable<ISearcher>> searchers)
            : base(name, options)
        {
            _searchers = searchers;
        }

        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<BaseLuceneSearcher> Searchers => _searchers.Value.OfType<BaseLuceneSearcher>();

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
            => new MultiSearchContext(Searchers.Select(s => s.GetSearchContext()).ToArray());

    }
}
