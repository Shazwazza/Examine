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
    public class ExternalDataScoreQuery<TData> : ReaderDataScoreQuery
        where TData : class
    {
        private readonly Func<TData, float> _scorer;

        public ExternalDataScoreQuery(Query subQuery, Func<ISearcherContext> contextResolver, ScoreOperation scoreOperation, Func<TData, float> scorer) 
            : base(subQuery, contextResolver, scoreOperation, null)
        {
            _scorer = scorer;
        }

        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, ISearcherContext context)
        {
            return new ScoreProvider(reader, data, scoreOperation, _scorer);
        }

        class ScoreProvider : CustomScoreProvider
        {
            private readonly object[] _data;
            private readonly ScoreOperation _scoreOperation;
            private readonly Func<TData, float> _scorer;

            public ScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, Func<TData, float> scorer)
                : base(reader)
            {
                _data = data.ExternalData;
                _scoreOperation = scoreOperation;
                _scorer = scorer;
            }
           
            public override float CustomScore(int doc, float subQueryScore, float valSrcScore)
            {
                var score = 0f;
                if( _data != null )
                {
                    var ex = _data[doc] as TData;
                    if( ex != null )
                    {
                        score = _scorer(ex);
                    }
                }

                return _scoreOperation.GetScore(subQueryScore, score);
            }
        }
    }
}
