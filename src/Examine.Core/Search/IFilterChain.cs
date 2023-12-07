using System;

namespace Examine.Search
{
    /// <summary>
    /// Filter Chaining
    /// </summary>
    public interface IFilterChain
    {
        /// <summary>
        /// Chain Filter AND
        /// </summary>
        /// <param name="nextFilter">First Filter in the Chain</param>
        /// <returns></returns>
        IFilterChain Chain(Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);

        /// <summary>
        /// Chain Filter
        /// </summary>
        /// <param name="nextFilter">Next Filter in the Chain</param>
        /// <param name="operation">Operation between the filter in the chain</param>
        /// <returns></returns>
        IFilterChain Chain(ChainOperation operation, Func<INestedFilter, INestedBooleanFilterOperation> nextFilter);
    }
}
