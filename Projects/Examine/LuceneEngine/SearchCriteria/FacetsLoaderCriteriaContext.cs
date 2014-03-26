using System.Collections.Generic;
using Examine.LuceneEngine.Cru;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System.Linq;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.SearchCriteria
{
    internal class FacetsLoaderCriteriaContext : ICriteriaContext
    {
        private readonly IndexSearcher _searcher;
        private readonly SearcherContext _searcherContext;
        
        public FacetsLoaderCriteriaContext(IndexSearcher searcher, SearcherContext searcherContext)
        {
            _searcher = searcher;
            _searcherContext = searcherContext;

            ManagedQueries = new List<KeyValuePair<IIndexValueType, Query>>();
        }

        public Searcher Searcher { get { return _searcher; } }
        public FacetMap FacetMap { get { return _searcherContext.FacetsLoader.FacetMap; } }

        public FacetsLoader FacetsLoader { get { return _searcherContext.FacetsLoader; } }

        public ReaderData GetReaderData(IndexReader reader)
        {
            return _searcherContext.FacetsLoader.GetReaderData(reader);
        }

        public IEnumerable<IIndexValueType> ValueTypes
        {
            get { return _searcherContext.RegisteredValueTypes; }
        }

        public IIndexValueType GetValueType(string fieldName)
        {
            return _searcherContext.GetValueType(fieldName, false);            
        }

        private KeyValuePair<int, IndexReader>[] _docStarts;

        public DocumentData GetDocumentData(int doc)
        {
            if (_docStarts == null)
            {
                _docStarts = _searcher.GetIndexReader().GetAllSubReaders().Select(r => 
                    new KeyValuePair<int, IndexReader>(r.MaxDoc(), r)).ToArray();
            }

            foreach (var r in _docStarts)
            {                
                if (doc < r.Key)
                {
                    var readerData = GetReaderData(r.Value);
                    return readerData == null ? null : new DocumentData(readerData, doc);
                }
                doc -= r.Key;
            }

            return null;   
        }

        public List<KeyValuePair<IIndexValueType, Query>> ManagedQueries { get; private set; }
    }
}