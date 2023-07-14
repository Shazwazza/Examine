using Lucene.Net.Search;

namespace Examine.Lucene.Scoring
{
    public interface ILuceneRelevanceScorerFunctionDefintion
    {
        Query GetScoreQuery(Query inner);
    }
}
