using System;

namespace Examine.Search
{
    /// <summary>
    /// Represents options for querying
    /// </summary>
    public class QueryOptions
    {
        public const int AbsoluteMaxResults = 10000;

        /// <summary>
        /// The default maximum ammount of results
        /// </summary>
        public const int DefaultMaxResults = 100;

        /// <summary>
        /// Creates a <see cref="QueryOptions"/> with the specified parameters
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public static QueryOptions SkipTake(int skip, int? take = null) => new QueryOptions(skip, take ?? DefaultMaxResults);

        /// <summary>
        /// Creates a default <see cref="QueryOptions"/>
        /// </summary>
        public static QueryOptions Default { get; } = new QueryOptions(0, DefaultMaxResults);

        /// <inheritdoc/>
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
            Take = take ?? DefaultMaxResults;
        }

        /// <summary>
        /// The number of documents to skip in the result set.
        /// </summary>
        public int Skip { get; }

        /// <summary>
        /// The number of documents to take in the result set.
        /// </summary>
        public int Take { get; }
    }
}
