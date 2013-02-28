using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Function;

namespace Examine.LuceneEngine.Scoring
{
    public abstract class ReaderDataScoreQuery : CustomScoreQuery
    {
        private readonly Func<ISearcherContext> _contextResolver;
        protected ScoreOperation ScoreOperation { get; set; }

        public ReaderDataScoreQuery(Query subQuery, Func<ISearcherContext> contextResolver, ScoreOperation scoreOperation, ValueSourceQuery[] valSrcQueries)
            : base(subQuery, valSrcQueries)
        {          
            _contextResolver = contextResolver;
            ScoreOperation = scoreOperation;
        }


        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader)
        {
            var context = _contextResolver();
            return GetCustomScoreProvider(reader, context.ReaderData[reader], ScoreOperation, context);
        }

        protected abstract CustomScoreProvider GetCustomScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, ISearcherContext context);
    }
}
