
using System;

namespace Examine.SearchCriteria
{
    public interface IBooleanOperation<TBoolOp, out TQuery, out TSearchCriteria> : IBooleanOperation
        where TBoolOp : IBooleanOperation
        where TQuery : IQuery
        where TSearchCriteria: ISearchCriteria
    {
        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        new TQuery And();

        /// <summary>
        /// Adds the nested query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        new TBoolOp And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
        new TQuery Or();

        /// <summary>
        /// Adds the nested query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        new TBoolOp Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
        new TQuery Not();

        /// <summary>
        /// Adds the nested query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp"></param>
        /// <returns></returns>
        new TBoolOp AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>
        new TSearchCriteria Compile();
    }

    /// <summary>
    /// Defines the supported operation for addition of additional clauses in the fluent API
    /// </summary>
    public interface IBooleanOperation
    {
        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        IQuery And();

        /// <summary>
        /// Adds the nested query
        /// </summary>
        /// <param name="inner"></param>
        /// <returns></returns>
        IBooleanOperation And(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
        IQuery Or();

        /// <summary>
        /// Adds the nested query
        /// </summary>
        /// <param name="inner"></param>
        /// <returns></returns>
        IBooleanOperation Or(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);
        
        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
        IQuery Not();


        /// <summary>
        /// Adds the nested query
        /// </summary>
        /// <param name="inner"></param>
        /// <returns></returns>
        IBooleanOperation AndNot(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);


        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>
        ISearchCriteria Compile();

    }
}
