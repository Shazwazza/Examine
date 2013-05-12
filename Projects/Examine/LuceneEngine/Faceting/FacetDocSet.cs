using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFacetDocSet
    {
        DocIdSet GetDocs(IndexReader reader);
    }

    /// <summary>
    /// 
    /// </summary>
    public class SimpleFacetDocSet : IFacetDocSet
    {
        private readonly DocIdSet _set;

        public SimpleFacetDocSet(DocIdSet set)
        {            
            _set = set;
        }

        public DocIdSet GetDocs(IndexReader reader)
        {
            return _set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class QueryFacetDocSet : IFacetDocSet
    {
        private readonly Query _query;

        public QueryFacetDocSet(Query query)
        {            
            _query = query;
        }

        public DocIdSet GetDocs(IndexReader reader)
        {
            return new QueryWrapperFilter(_query).GetDocIdSet(reader);
        }
    }
}
