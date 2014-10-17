using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Scoring
{
    //TODO: Figure out what this does? and when we want to use it since its not in use
    internal class ScoreMultiplier : ScoreOperation
    {
        public override float GetScore(float inner, float outer)
        {
            return inner*outer;
        }
    }
}
