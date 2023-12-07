using System;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// A set of helpers for working with Lucene.Net in Examine
    /// </summary>
    public static class LuceneSearchExtensions
    {

        /// <summary>
        /// Converts an Examine boolean operation to a Lucene representation
        /// </summary>
        /// <param name="o">The operation.</param>
        /// <returns>The translated Boolean operation</returns>        
        public static Occur ToLuceneOccurrence(this BooleanOperation o)
        {
            switch (o)
            {
                case BooleanOperation.And:
                    return Occur.MUST;
                case BooleanOperation.Not:
                    return Occur.MUST_NOT;
                case BooleanOperation.Or:
                default:
                    return Occur.SHOULD;
            }
        }

        /// <summary>
        /// Converts a Lucene boolean occurrence to an Examine representation
        /// </summary>
        /// <param name="o">The occurrence to translate.</param>
        /// <returns>The translated boolean occurrence</returns>

        public static BooleanOperation ToBooleanOperation(this Occur o)
        {
            if (o == Occur.MUST)
            {
                return BooleanOperation.And;
            }
            else if (o == Occur.MUST_NOT)
            {
                return BooleanOperation.Not;
            }
            else
            {
                return BooleanOperation.Or;
            }
        }
        /// <summary>
        /// Executes the query
        /// </summary>
        public static ILuceneSearchResults ExecuteWithLucene(this IQueryExecutor queryExecutor, QueryOptions? options = null)
        {
            var results = queryExecutor.Execute(options);
            if (results is ILuceneSearchResults luceneSearchResults)
            {
                return luceneSearchResults;
            }
            throw new NotSupportedException("QueryExecutor is not Lucene.NET");
        }
    }
}
