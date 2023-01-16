using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;

namespace Examine.Suggest
{
    public interface ISuggestionOrdering : ISuggestionExecutor
    {
        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fields">The field names.</param>
        /// <returns></returns>
        ISuggestionSelectFields OrderBy(params SortableField[] fields);

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fields">The field names.</param>
        /// <returns></returns>
        ISuggestionSelectFields OrderByDescending(params SortableField[] fields);
    }
}
