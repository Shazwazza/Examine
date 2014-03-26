using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Cru;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    internal class ValueTypeWarmer : ISearcherWarmer
    {
        private readonly SearcherContext _searcherContext;

        public ValueTypeWarmer(SearcherContext searcherContext)
        {
            _searcherContext = searcherContext;
        }

        public void Warm(IndexSearcher s)
        {
            foreach (var reader in s.GetIndexReader().GetAllSubReaders())
            {
                foreach (var type in _searcherContext.ValueTypes )
                {
                    type.Value.AnalyzeReader(_searcherContext.FacetsLoader.GetReaderData(reader));
                }
            }            
        }
    }
}
