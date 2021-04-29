using System;
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;

namespace Examine.LuceneEngine.Search
{
    public class LateBoundQuery : Query
    {
        private readonly Func<Query> _factory;

        private Query _wrapped;
        public Query Wrapped => _wrapped ?? (_wrapped = _factory());

        public LateBoundQuery(Func<Query> factory)
        {
            _factory = factory;
        }

        public override object Clone()
        {
            return Wrapped.Clone();
        }

        public override Weight CreateWeight(IndexSearcher searcher)
        {
            return Wrapped.CreateWeight(searcher);
        }

        /// <summary>
        /// Expert: adds all terms occuring in this query to the terms set. Only
        ///             works if this query is in its <see cref="M:Lucene.Net.Search.Query.Rewrite(Lucene.Net.Index.IndexReader)">rewritten</see> form.
        /// </summary>
        /// <throws>UnsupportedOperationException if this query is not yet rewritten </throws>
        public override void ExtractTerms(ISet<Term> terms)
        {
            Wrapped.ExtractTerms(terms);
        }

        /// <summary>
        /// Gets or sets the boost for this query clause to <c>b</c>.  Documents
        ///             matching this clause will (in addition to the normal weightings) have
        ///             their score multiplied by <c>b</c>.  The boost is 1.0 by default.
        /// </summary>
        public override float Boost
        {
            get => Wrapped.Boost;
            set => Wrapped.Boost = value;
        }

     

        public override Query Rewrite(IndexReader reader)
        {
            return Wrapped.Rewrite(reader);
        }

  


        public override string ToString(string field)
        {
            return Wrapped.ToString(field);
        }
    }
}