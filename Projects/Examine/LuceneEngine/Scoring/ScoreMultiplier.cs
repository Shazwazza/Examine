using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Scoring
{
    public class ScoreMultiplier : ScoreOperation
    {
        public override float GetScore(float inner, float outer)
        {
            return inner*outer;
        }
    }
}
