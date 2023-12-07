using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Filter Chain
    /// </summary>
    public struct FilterChain
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="operation"></param>
        public FilterChain(Filter filter, int operation)
        {
            Filter = filter;
            Operation = operation;
        }

        /// <summary>
        /// Filter
        /// </summary>
        public Filter Filter { get; }

        /// <summary>
        /// Filter Chain Operation
        /// </summary>
        public int Operation { get; }

    }
}
