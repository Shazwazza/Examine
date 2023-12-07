using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Filter Chain Operation
    /// </summary>
    public abstract class FilterChainOpBase : IFilterChainStart, IFilterChain
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FilterChainOpBase()
        {
            ChainOps = new Queue<FilterChain> ();
        }

        /// <summary>
        /// Chained Filter Operations
        /// </summary>
        public Queue<FilterChain> ChainOps { get; set; }

        /// <summary>
        /// Build Chained Filter
        /// </summary>
        /// <returns></returns>
        public ChainedFilter Build()
        {
            var count = ChainOps.Count;
            var filters = new Filter[count];
            int[] logicArray = new int[count];
            for (int i = 0; i < count; i++)
            {
                var fc = ChainOps.Dequeue();
                filters[i] = fc.Filter;
                logicArray[i] = fc.Operation;
            }

            var chainedFilter = new ChainedFilter(filters, logicArray);
            return chainedFilter;
        }

        public abstract IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);

        public abstract IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);
    }
}
