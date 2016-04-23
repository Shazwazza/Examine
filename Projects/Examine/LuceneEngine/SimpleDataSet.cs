using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine
{
    [Obsolete("Use ValueSetIndexer with IValueSetService instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
