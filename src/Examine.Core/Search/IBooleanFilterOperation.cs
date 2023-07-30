using System;

namespace Examine.Search
{
    public interface IBooleanFilterOperation : IOrdering
    {
        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        IFilter And();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation And(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
        IFilter Or();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation Or(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
        IFilter Not();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation AndNot(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);
    }
}
