using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetCountCollector : IndexReaderDataCollector
    {
        private FacetMap _map;
        private FacetLevel[][] _levels;
        public FacetCounts Counts { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="readerDataCollection"></param>
        /// <param name="inner"></param>
        /// <param name="counts">If a lot of facets are used FacetCounts can be reused from an object pool and passed to this constructor. </param>
        public FacetCountCollector(IndexReaderDataCollection readerDataCollection, Collector inner, FacetCounts counts = null) : base(readerDataCollection, inner)
        {
            _map = readerDataCollection.SearcherContext.Searcher.FacetConfiguration.FacetMap;

            Counts = counts ?? new FacetCounts();
            Counts.Reset(_map);            
        }

        public override void Collect(int doc)
        {            
            base.Collect(doc);

            if (Data != null)
            {
                var docFacets = _levels[doc];
                if( docFacets != null )
                {
                    for( int i = 0, n = docFacets.Length; i < n; i++)
                    {
                        ++Counts.Counts[docFacets[i].FacetId];
                    }
                }

            }
        }

        public override void SetNextReader(Lucene.Net.Index.IndexReader reader, int docBase)
        {
            base.SetNextReader(reader, docBase);

            _levels = Data != null ? Data.FacetLevels : null;
        }
    }
}
