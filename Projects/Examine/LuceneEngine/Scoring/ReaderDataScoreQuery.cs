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
        private readonly ICriteriaContext _context;
        protected ScoreOperation ScoreOperation { get; set; }

        public ReaderDataScoreQuery(Query subQuery, ICriteriaContext context, ScoreOperation scoreOperation, ValueSourceQuery[] valSrcQueries)
            : base(subQuery, valSrcQueries)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (scoreOperation == null) throw new ArgumentNullException("scoreOperation");

            _context = context;
            ScoreOperation = scoreOperation;
        }


        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader)
        {
            return GetCustomScoreProvider(reader, _context.GetReaderData(reader), ScoreOperation, _context);
        }

        protected abstract CustomScoreProvider GetCustomScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, ICriteriaContext context);
    }
}
