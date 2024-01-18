using System;

namespace Examine.Search
{
    public interface INestedBooleanFilterOperation
    {
        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        INestedFilter And();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation And(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
        INestedFilter Or();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation Or(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
        INestedFilter Not();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        INestedBooleanFilterOperation AndNot(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);
    }
}
