using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Scoring
{
    public class ScoreAdder : ScoreOperation
    {
        private readonly float _innerWeight;

        public ScoreAdder(double innerWeight)
        {
            _innerWeight = (float) innerWeight;
        }

        public override float GetScore(float inner, float outer)
        {
            return _innerWeight*inner + (1 - _innerWeight)*outer;
        }
    }
}
