using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetFilter : Filter
    {
        private readonly Func<ISearcherContext> _contextResolver;
        private readonly FacetKey _key;

        /// <summary>
        /// The context is returned by a lambda to allow late binding from IQuery.
        /// </summary>
        /// <param name="contextResolver"></param>
        /// <param name="key">The facet key </param>
        public FacetFilter(Func<ISearcherContext> contextResolver, FacetKey key)
        {
            _contextResolver = contextResolver;
            _key = key;
        }


        public override DocIdSet GetDocIdSet(Lucene.Net.Index.IndexReader reader)
        {
            var context = _contextResolver();
            
            var readerData = context.ReaderData[reader];

            if (readerData != null)
            {
                var facet = context.Searcher.FacetConfiguration.FacetMap.GetIndex(_key);
                if (facet > -1)
                {
                    DocIdSet set;
                    if (readerData.FacetFilters.TryGetValue(facet, out set))
                    {
                        return set;
                    }
                }
            }

            return DocIdSet.EMPTY_DOCIDSET;
        }
    }

}
