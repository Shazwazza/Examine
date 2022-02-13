using System;

namespace Examine.Search
{
    public class QueryOptions
    {
        public const int DefaultMaxResults = 500;
        public static QueryOptions SkipTake(int skip, int? take = null) => new QueryOptions(skip, take ?? DefaultMaxResults,null);
        public static QueryOptions Default { get; } = new QueryOptions(0, DefaultMaxResults,null);

        public QueryOptions(int skip, int? take = null, TimeSpan? timeLimit = null)
        {
            if (skip < 0)
            {
                throw new ArgumentException("Skip cannot be negative");
            }

            if (take.HasValue && take < 0)
            {
                throw new ArgumentException("Take cannot be negative");
            }

            Skip = skip;
            Take = take ?? DefaultMaxResults;
        }

        public int Skip { get; }
        public int Take { get; }

        public TimeSpan? TimeLimit { get; set; }
    }
}
