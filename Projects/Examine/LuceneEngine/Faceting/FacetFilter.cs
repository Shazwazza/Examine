using System;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetFilter : Filter
    {
        private readonly Func<ICriteriaContext> _criteriaContext;
        private readonly FacetKey _key;

        
        public FacetFilter(Func<ICriteriaContext> criteriaContext, FacetKey key)
        {
            _criteriaContext = criteriaContext;
            _key = key;
        }


        public override DocIdSet GetDocIdSet(IndexReader reader)
        {
            var context = _criteriaContext();

            var readerData = context.GetReaderData(reader);

            if (readerData != null)
            {                
                if (context != null && context.FacetMap != null)
                {
                    var facet = context.FacetMap.GetIndex(_key);
                    if (facet > -1)
                    {
                        Filter set;
                        if (readerData.FacetFilters.TryGetValue(facet, out set))
                        {
                            return set.GetDocIdSet(reader);
                        }
                    }
                }
            }

            return DocIdSet.EMPTY_DOCIDSET;
        }
    }

}
