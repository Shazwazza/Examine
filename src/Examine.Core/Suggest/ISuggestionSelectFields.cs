using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Suggest
{
    public interface ISuggestionSelectFields : ISuggestionExecutor
    {
        /// <summary>
        /// Return only the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        ISuggestionSelectFields SelectFields(ISet<string> fieldNames);
    }
}
