using System;

namespace Examine.Search
{
    public interface IBooleanFilterOperation
    {
        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        IFilter AndFilter();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation AndFilter(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
        IFilter OrFilter();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation OrFilter(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
        IFilter NotFilter();

        /// <summary>
        /// Adds the nested filter
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        IBooleanFilterOperation AndNotFilter(Func<INestedFilter, INestedBooleanFilterOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);
    }
}
