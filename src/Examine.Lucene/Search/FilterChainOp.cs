using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    internal class FilterChainOp : FilterChainOpBase, IFilterChainStart, IFilterChain
    {
        private readonly LuceneSearchFilteringOperation _luceneFilter;

        public FilterChainOp(LuceneSearchFilteringOperation luceneFilter)
        {
            ChainOps = new Queue<FilterChain>();
            _luceneFilter = luceneFilter;
        }

        public override IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter)
        {
            throw new NotImplementedException();
            // Start a chain
            //ChainOps.Enqueue(new FilterChain(nextFilter, (int)ChainOperation.AND));
            return this;
        }

        public override IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter)
        {
            throw new NotImplementedException();
            // Continue a chain
            //ChainOps.Enqueue(new FilterChain(nextFilter, (int)operation));
            return this;
        }

    }

    public struct FilterChain
    {
        public Filter Filter { get; }

        public FilterChain(Filter filter, int operation)
        {
            Filter = filter;
            Operation = operation;
        }

        public int Operation { get; }

    }
}
