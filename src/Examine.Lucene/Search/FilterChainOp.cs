using System;
using System.Collections.Generic;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Filter Chain Operation
    /// </summary>
    public class FilterChainOp : FilterChainOpBase, IFilterChainStart, IFilterChain
    {
        private readonly LuceneSearchFilteringOperation _luceneFilter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="luceneFilter"></param>
        public FilterChainOp(LuceneSearchFilteringOperation luceneFilter)
        {
            ChainOps = new Queue<FilterChain>();
            _luceneFilter = luceneFilter;
        }

        /// <inheritdoc/>
        public override IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter)
        {
            var filterOp = new LuceneSearchFilteringOperation(_luceneFilter.LuceneSearchQuery);
            var bo = new LuceneBooleanOperation(_luceneFilter.LuceneSearchQuery);
            var fbo = new LuceneFilteringBooleanOperation(filterOp);
            var filter = fbo.GetNestedFilterOp(nextFilter, BooleanOperation.And);
            // Start a chain
            ChainOps.Enqueue(new FilterChain(filter, (int)ChainOperation.AND));
            return this;
        }

        /// <inheritdoc/>
        public override IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter)
        {
            var filterOp = new LuceneSearchFilteringOperation(_luceneFilter.LuceneSearchQuery);
            var bo = new LuceneBooleanOperation(_luceneFilter.LuceneSearchQuery);
            var fbo = new LuceneFilteringBooleanOperation(filterOp);
            var filter = fbo.GetNestedFilterOp(nextFilter, BooleanOperation.And);
            // Continue a chain
            ChainOps.Enqueue(new FilterChain(filter, (int)operation));
            return this;
        }
    }
}
