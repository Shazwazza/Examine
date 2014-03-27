using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Used for the ISimpleDataService
    /// </summary>
    public class SimpleDataSet
    {
        /// <summary>
        /// The definition of the node: NodeId & Node type
        /// </summary>
        public IndexedNode NodeDefinition { get; set; }

        /// <summary>
        /// The data contained in the rows for the item
        /// </summary>
        public Dictionary<string, string> RowData { get; set; }
    }
}
