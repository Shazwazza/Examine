using System.Security;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    [SecuritySafeCritical]
    public class FacetCountCollector : IndexReaderDataCollector
    {
        private FacetMap _map;
        private FacetLevel[][] _levels;
        public FacetCounts Counts { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readerDataCollection"></param>
        /// <param name="_criteriaContext"></param>
        /// <param name="inner"></param>
        /// <param name="counts">If a lot of facets are used FacetCounts can be reused from an object pool and passed to this constructor. </param>
        public FacetCountCollector(ICriteriaContext _criteriaContext, Collector inner, FacetCounts counts = null) : base(_criteriaContext, inner)
        {
            _map = _criteriaContext.FacetsLoader.FacetMap;

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
