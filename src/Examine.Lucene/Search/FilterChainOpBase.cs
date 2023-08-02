using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public abstract class FilterChainOpBase : IFilterChainStart, IFilterChain
    {
        public FilterChainOpBase()
        {
            ChainOps = new Queue<FilterChain> ();
        }
        public Queue<FilterChain> ChainOps { get; set; }

        public ChainedFilter Build()
        {
            Filter[] filters = new Filter[ChainOps.Count];
            int[] logicArray = new int[ChainOps.Count];

            for (int i = 0; i < ChainOps.Count; i++)
            {
                var fc = ChainOps.Dequeue();
                filters[i] = fc.Filter;
                logicArray[i] = fc.Operation;
            }

            ChainedFilter chainedFilter = new ChainedFilter(filters, logicArray);
            return chainedFilter;
        }

        public abstract IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);
        public abstract IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);
    }
}
