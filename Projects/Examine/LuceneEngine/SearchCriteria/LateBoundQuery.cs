using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class LateBoundQuery : Query
    {
        private readonly Func<Query> _factory;

        private Query _wrapped;
        public Query Wrapped
        {
            get { return _wrapped ?? (_wrapped = _factory()); }
        }

        public LateBoundQuery(Func<Query> factory)
        {
            _factory = factory;
        }

        public override object Clone()
        {
            return Wrapped.Clone();
        }

        public override Weight CreateWeight(Searcher searcher)
        {
            return Wrapped.CreateWeight(searcher);
        }

        public override void ExtractTerms(System.Collections.Hashtable terms)
        {
            Wrapped.ExtractTerms(terms);
        }

        public override float GetBoost()
        {
            return Wrapped.GetBoost();
        }

        public override Lucene.Net.Search.Similarity GetSimilarity(Searcher searcher)
        {
            return Wrapped.GetSimilarity(searcher);
        }

        public override Query Rewrite(Lucene.Net.Index.IndexReader reader)
        {
            return Wrapped.Rewrite(reader);
        }

        public override void SetBoost(float b)
        {
            Wrapped.SetBoost(b);            
        }

        public override Weight Weight(Searcher searcher)
        {
            return Wrapped.Weight(searcher);
        }        

        public override Query Combine(Query[] queries)
        {
            return Wrapped.Combine(queries);
        }

        public override string ToString(string field)
        {
            return Wrapped.ToString(field);
        }
    }
}
