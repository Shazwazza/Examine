using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Scoring
{
    public class ScoreAdder : ScoreOperation
    {
        private readonly float _originalWeight;

        public ScoreAdder(double originalWeight)
        {
            _originalWeight = (float) originalWeight;
        }

        public override float GetScore(float inner, float outer)
        {
            return _originalWeight*inner + (1 - _originalWeight)*outer;
        }
    }
}
