using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Facets
{
    public class FacetFilter : Filter
    {
        private readonly FacetsLoader _facetsLoader;
        private readonly FacetKey _key;

        
        public FacetFilter(FacetsLoader facetsLoader, FacetKey key)
        {
            _facetsLoader = facetsLoader;
            _key = key;
        }


        public override DocIdSet GetDocIdSet(IndexReader reader)
        {

            var readerData = _facetsLoader.GetReaderData(reader);

            if (readerData != null)
            {
                var config = _facetsLoader.Configuration;
                if (config != null)
                {
                    var facet = config.FacetMap.GetIndex(_key);
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
