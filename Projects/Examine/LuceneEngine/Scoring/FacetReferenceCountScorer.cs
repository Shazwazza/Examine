using System;
using System.Collections.Generic;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Function;
using System.Linq;

namespace Examine.LuceneEngine.Scoring
{
    //TODO: Figure out what this does? and when we want to use it since its not in use
    internal class FacetReferenceCountScorer : ReaderDataScoreQuery
    {
        private readonly FacetCounts _counts;

        public FacetReferenceCountScorer(Query subQuery, ICriteriaContext context, ScoreOperation op, FacetCounts counts)
            : base(subQuery, context, op, null)
        {
            _counts = counts;
        }

        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, ICriteriaContext context)
        {
            return new ScoreProvider(reader, data, scoreOperation,_counts);
        }

        class ScoreProvider : CustomScoreProvider
        {
            private readonly ReaderData _data;
            private readonly ScoreOperation _scoreOperation;
            private readonly FacetCounts _counts;
            private readonly int _maxCount;
            private readonly FacetMap _map;

            public ScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, FacetCounts counts)
                : base(reader)
            {
                _data = data;
                _scoreOperation = scoreOperation;
                _counts = counts;

                _map = counts.FacetMap;
                _maxCount = counts.Counts.Select(count => count.Value).Concat(new[] {0}).Max();
            }

            public override float CustomScore(int doc, float subQueryScore, float[] valSrcScores)
            {
                var score = 0f;

                if (_maxCount > 0)
                {

                    var id = _data.ExternalIds[doc];
                    if (id != 0)
                    {
                        FacetReferenceInfo[] _infos;
                        
                        if (_map.TryGetReferenceInfo(id, out _infos))
                        {
                            foreach (var info in _infos)
                            {
                                score += _counts.Counts[info.Id];
                            }
                        }
                    }

                }

                return _scoreOperation.GetScore(subQueryScore, score);
            }
        }
    }
}