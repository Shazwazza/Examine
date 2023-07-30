using System;

namespace Examine.Search
{
    public interface IFilterChainStart
    {
        /// <summary>
        /// Chain Filter
        /// </summary>
        /// <param name="nextFilter">First Filter in the Chain</param>
        /// <returns></returns>
        IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);
    }
}
