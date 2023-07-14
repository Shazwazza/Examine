using System;

namespace Examine.Search
{
    /// <summary>
    /// Defines the supported operation for addition of additional clauses in the fluent API
    /// </summary>
    public interface IScoreQuery : IOrdering
    {
        IScoreQuery ScoreWith(params string[] scorers);
    }
}
