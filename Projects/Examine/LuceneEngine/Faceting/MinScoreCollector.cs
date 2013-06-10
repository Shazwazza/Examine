using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class MinScoreCollector : Collector
    {
        private readonly Collector _inner;
        private readonly float _minScore;

        public MinScoreCollector(Collector inner, float minScore = 0f)
        {
            _inner = inner;
            _minScore = minScore;
        }

        private Scorer _scorer;
        public override void SetScorer(Scorer scorer)
        {
            _scorer = scorer;
            _inner.SetScorer(scorer);
        }

        public override void Collect(int doc)
        {
            if (_scorer.Score() > _minScore)
            {
                _inner.Collect(doc);
            }
        }

        public override void SetNextReader(IndexReader reader, int docBase)
        {
            _inner.SetNextReader(reader, docBase);
        }

        public override bool AcceptsDocsOutOfOrder()
        {
            return _inner.AcceptsDocsOutOfOrder();
        }
    }
}
