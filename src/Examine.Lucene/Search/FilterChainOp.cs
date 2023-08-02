using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    internal class FilterChainOp : IFilterChainStart, IFilterChain
    {
        private readonly LuceneSearchFilteringOperation _luceneFilter;

        public Queue<FilterChain> ChainOps { get; set; }

        public FilterChainOp(LuceneSearchFilteringOperation luceneFilter)
        {
            ChainOps = new Queue<FilterChain>();
            _luceneFilter = luceneFilter;
        }

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

        public IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter)
        {
            var nestedOp = nextFilter.Invoke(_luceneFilter);
            throw new NotImplementedException();
        }

        public IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter)
        {
            throw new NotImplementedException();
        }
    }

    internal class FilterChain
    {
        public Filter Filter { get; set; }

        public int Operation { get; set; }
    }
}
