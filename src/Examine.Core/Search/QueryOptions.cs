using System;

namespace Examine.Search
{
    public class QueryOptions
    {
        public QueryOptions(int skip, int? take = null)
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
            Take = take;
        }

        public static QueryOptions Default { get; } = new QueryOptions(0, 500);

        public int Skip { get; }
        public int? Take { get; }
    }
}
