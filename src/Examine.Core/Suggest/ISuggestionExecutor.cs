using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Suggest
{
    public interface ISuggestionExecutor
    {
        /// <summary>
        /// Executes the query
        /// </summary>
        ISuggestionResults Execute(string searchText, SuggestionOptions options = null);
    }
}
