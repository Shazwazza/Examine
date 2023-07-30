using System;

namespace Examine.Search
{
    public interface IFilterChain
    {
        /// <summary>
        /// Chain Filter
        /// </summary>
        /// <param name="nextFilter">Next Filter in the Chain</param>
        /// <param name="operation">Operation between the filter in the chain</param>
        /// <returns></returns>
        IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);
    }
}
