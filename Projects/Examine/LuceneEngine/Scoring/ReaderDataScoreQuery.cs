using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Function;

namespace Examine.LuceneEngine.Scoring
{
    public abstract class ReaderDataScoreQuery : CustomScoreQuery
    {
        private readonly Func<ICriteriaContext> _contextResolver;
        protected ScoreOperation ScoreOperation { get; set; }

        public ReaderDataScoreQuery(Query subQuery, Func<ICriteriaContext> contextResolver, ScoreOperation scoreOperation, ValueSourceQuery[] valSrcQueries)
            : base(subQuery, valSrcQueries)
        {
            if (contextResolver == null) throw new ArgumentNullException("contextResolver");
            if (scoreOperation == null) throw new ArgumentNullException("scoreOperation");

            _contextResolver = contextResolver;
            ScoreOperation = scoreOperation;
        }


        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader)
        {
            var context = _contextResolver();
            return GetCustomScoreProvider(reader, context.GetReaderData(reader), ScoreOperation, context);
        }

        protected abstract CustomScoreProvider GetCustomScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, ICriteriaContext context);
    }
}
