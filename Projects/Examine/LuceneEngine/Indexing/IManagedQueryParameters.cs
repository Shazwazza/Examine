using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// Interface for parameters for managed queries (i.e. queries provided by IIndexValueType)
    /// </summary>
    public interface IManagedQueryParameters
    {
        object GetParameter(string name);
    }
}
