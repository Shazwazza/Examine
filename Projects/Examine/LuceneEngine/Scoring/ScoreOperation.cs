namespace Examine.LuceneEngine.Scoring
{
    public abstract class ScoreOperation
    {
        public abstract float GetScore(float inner, float outer);
    }
}