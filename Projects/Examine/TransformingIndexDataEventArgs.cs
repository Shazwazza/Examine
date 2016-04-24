using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Examine.LuceneEngine.Indexing;

namespace Examine
{
    /// <summary>
    /// Aguments for the TransformIndexValues event
    /// </summary>
    public class TransformingIndexDataEventArgs : CancelEventArgs
    {
        public IndexItem IndexItem { get; private set; }
        public IReadOnlyDictionary<string, IEnumerable<object>> OriginalValues { get; private set; }

        public TransformingIndexDataEventArgs(IndexItem indexItem, IReadOnlyDictionary<string, IEnumerable<object>> originalValues)
        {
            IndexItem = indexItem;
            OriginalValues = originalValues;
        }
    }
}