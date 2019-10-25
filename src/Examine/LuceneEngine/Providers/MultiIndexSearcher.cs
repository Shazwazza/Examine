using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// A provider that allows for searching across multiple indexes
    ///</summary>
    public class MultiIndexSearcher : BaseLuceneSearcher, IDisposable
    {
        private readonly Lazy<IEnumerable<ISearcher>> _searchers;

        #region Constructors

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="indexes"></param>
        /// <param name="analyzer"></param>
        public MultiIndexSearcher(string name, IEnumerable<IIndex> indexes, Analyzer analyzer = null)
            : base(name, analyzer ?? new StandardAnalyzer(Util.Version))
        {
            _searchers = new Lazy<IEnumerable<ISearcher>>(() => indexes.Select(x => x.GetSearcher()));
            _disposer = new DisposableSearcher(this);
        }

        /// <summary>
        /// Constructor to allow for creating a searcher at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchers"></param>
        /// <param name="analyzer"></param>
        public MultiIndexSearcher(string name, Lazy<IEnumerable<ISearcher>> searchers, Analyzer analyzer = null)
            : base(name, analyzer ?? new StandardAnalyzer(Util.Version))
        {
            _searchers = searchers;
            _disposer = new DisposableSearcher(this);
        }

        #endregion


        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<LuceneSearcher> Searchers => _searchers.Value.OfType<LuceneSearcher>();

        /// <summary>
        /// Returns a list of fields to search on based on all distinct fields found in the sub searchers
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllIndexedFields()
        {
            var searchableFields = new List<string>();
            foreach (var searcher in Searchers)
            {
                searchableFields.AddRange(searcher.GetAllIndexedFields());
            }
            return searchableFields.Distinct().ToArray();
        }

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
		
        public override Searcher GetLuceneSearcher()
        {
	        var searchables = new List<Searchable>();
			//NOTE: Do not convert this to Linq as it will fail the Code Analysis because Linq screws with it.
			foreach(var s in Searchers)
			{
			    var searcher = s.GetLuceneSearcher();
                if (searcher != null)
                    searchables.Add(searcher);
			}
			return new MultiSearcher(searchables.ToArray());
        }

        public override ISearchContext GetSearchContext()
        {
            return new MultiSearchContext(GetLuceneSearcher(), Searchers.Select(s => s.GetSearchContext()).ToArray());
        }


        #region IDisposable Members

        private readonly DisposableSearcher _disposer;

        private class DisposableSearcher : DisposableObjectSlim
        {
            private readonly MultiIndexSearcher _searcher;

            public DisposableSearcher(MultiIndexSearcher searcher)
            {
                _searcher = searcher;
            }

            /// <summary>
            /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
            /// </summary>
            
            protected override void DisposeResources()
            {
                foreach (var searcher in _searcher.Searchers)
                {
                    searcher.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposer.Dispose();
        }

        #endregion
    }
}
