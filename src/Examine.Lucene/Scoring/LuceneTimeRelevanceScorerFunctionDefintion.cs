using System;
using Examine.Lucene.Scoring;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Scoring {
    public class LuceneTimeRelevanceScorerFunctionDefintion : TimeRelevanceScorerFunctionDefintion, ILuceneRelevanceScorerFunctionDefintion
    {
        public LuceneTimeRelevanceScorerFunctionDefintion(string fieldName, float boost, TimeSpan boostTimeRange) : base(fieldName, boost, boostTimeRange)
        {
        }

        public Query GetScoreQuery(Query inner) => new FreshnessScoreQuery(inner, FieldName, BoostTimeRange, Boost);
    }

    public class FreshnessScoreQuery : CustomScoreQuery
    {
        private readonly string _fieldName;
        private readonly TimeSpan _duration;
        private readonly float _boost;

        public FreshnessScoreQuery(Query subQuery, string fieldName, TimeSpan duration, float boost) : base(subQuery)
        {
            _fieldName = fieldName;
            _duration = duration;
            _boost = boost;
        }

        protected override CustomScoreProvider GetCustomScoreProvider(AtomicReaderContext context) => new FreshnessScoreProvider(context, _fieldName, _duration, _boost);

        private class FreshnessScoreProvider : CustomScoreProvider
        {
            private readonly string _fieldName;
            private readonly TimeSpan _duration;
            private readonly float _boost;

            public FreshnessScoreProvider(AtomicReaderContext context, string fieldName, TimeSpan duration, float boost) : base(context)
            {
                _fieldName = fieldName;
                _duration = duration;
                _boost = boost;
            }

            public override float CustomScore(int doc, float subQueryScore, float valSrcScore)
            {
                var date = GetDocumentDate(doc);

                var score = subQueryScore;

                if (date != null)
                {
                    var end = DateTime.Now;
                    var start = end.Subtract(_duration);

                    if ((date > start && date < end) || (date < start && date > end))
                    {
                        score *= _boost;
                    }
                }

                return score;
            }

            private DateTime? GetDocumentDate(int doc)
            {
                var document = m_context.Reader.Document(doc);

                var field = document.GetField(_fieldName);

                if (field != null && field.NumericType == NumericFieldType.INT64)
                {
                    var timestamp = field.GetInt64Value() ?? 0;

                    var date = new DateTime(timestamp);

                    return date;
                }

                return null;
            }

        }
    }
}
