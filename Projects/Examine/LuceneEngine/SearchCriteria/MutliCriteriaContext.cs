using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class MutliCriteriaContext : ICriteriaContext
    {
        private readonly MultiSearcher _searcher;
        private readonly ICriteriaContext[] _inner;

        public MutliCriteriaContext(MultiSearcher searcher, ICriteriaContext[] inner)
        {            
            _searcher = searcher;
            _inner = inner;
            //We can do this because all facet configurations share a single static facetmap.
            FacetsLoader = inner.Select(cc => cc.FacetsLoader).FirstOrDefault(fm => fm != null);
            FacetMap = FacetsLoader != null ? FacetsLoader.FacetMap : null;

            ManagedQueries = new List<KeyValuePair<IIndexValueType, Query>>();
        }

        public Searcher Searcher { get { return _searcher; } }

        public FacetsLoader FacetsLoader { get; private set; }

        public FacetMap FacetMap { get; private set; }

        public ReaderData GetReaderData(IndexReader reader)
        {
            for (int i = 0, n = _inner.Length; i < n; i++)
            {
                var data = _inner[i].GetReaderData(reader);
                if (data != null)
                {
                    return data;
                }
            }

            return null;
        }

        public DocumentData GetDocumentData(int doc)
        {
            var index = _searcher.SubSearcher(doc);
            doc = _searcher.SubDoc(doc);

            return _inner[index].GetDocumentData(doc);
        }

        public List<KeyValuePair<IIndexValueType, Query>> ManagedQueries { get; private set; }


        public IEnumerable<IIndexValueType> ValueTypes
        {
            get { return _inner.SelectMany(cc=>cc.ValueTypes); }
        }

        public IIndexValueType GetValueType(string fieldName)
        {
            return _inner.Select(cc => cc.GetValueType(fieldName)).FirstOrDefault(type => type != null);
        }
    }
}
