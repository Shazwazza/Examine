using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine
{
    public interface ISimpleDataService
    {
        /// <summary>
        /// Returns a dictionary list of:
        /// - The IndexedNode definition (NodeId / Type)
        /// - The fields for the data row/item
        /// </summary>
        /// <param name="indexType"></param>
        /// <returns></returns>
        IEnumerable<SimpleDataSet> GetAllData(string indexType);

        
    }
}