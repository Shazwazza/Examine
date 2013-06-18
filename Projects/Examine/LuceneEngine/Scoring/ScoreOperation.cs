namespace Examine.LuceneEngine.Scoring
{
    public abstract class ScoreOperation
    {
        public abstract float GetScore(float inner, float outer);


        public static implicit operator ScoreOperation(double inner)
        {
            return new ScoreAdder(inner);
        }
    }
}