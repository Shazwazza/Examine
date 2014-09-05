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
    public class FacetLevelScoreQuery : ReaderDataScoreQuery
    {
        private readonly IFacetLevel[] _levels;

        public FacetLevelScoreQuery(Query subQuery, Func<ICriteriaContext> contextResolver, ScoreOperation op, params IFacetLevel[] levels)
            : base(subQuery, contextResolver, op, null)
        {
            _levels = levels ?? new IFacetLevel[0];
        }

        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, ICriteriaContext context)
        {
            //TODO: if the FacetMap is null or empty this will ysod! Need to do something about that.

            return new ScoreProvider(reader, data, scoreOperation, _levels.Select(l => l.ToFacetLevel(context.FacetsLoader.FacetMap)).ToArray());
        }

        class ScoreProvider : CustomScoreProvider
        {
            private readonly FacetLevel[][] _data;
            private readonly ScoreOperation _scoreOperation;
            private readonly Dictionary<int, float> _levels;

            public ScoreProvider(IndexReader reader, ReaderData data, ScoreOperation scoreOperation, FacetLevel[] levels)
                : base(reader)
            {
                if (data == null) throw new ArgumentNullException("data");
                if (scoreOperation == null) throw new ArgumentNullException("scoreOperation");
                if (levels == null) throw new ArgumentNullException("levels");

                _data = data.FacetLevels;
                _scoreOperation = scoreOperation;
                _levels = levels.ToDictionary(l => l.FacetId, l => l.Level);
            }

            public override float CustomScore(int doc, float subQueryScore, float[] valSrcScores)
            {
                float level, score = 0;
                foreach (var docLevel in _data[doc])
                {
                    if (_levels.TryGetValue(docLevel.FacetId, out level))
                    {
                        score += docLevel.Level * level;
                    }
                }

                return _scoreOperation.GetScore(subQueryScore, score);
            }
        }
    }
}