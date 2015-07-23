using System;
using System.Threading;

namespace Examine
{
    public class LazyIndexOperation : IIndexOperation
    {
        private readonly Lazy<IndexItem> _indexItem; 

        public LazyIndexOperation(Func<IndexItem> getItem, IndexOperationType op)
        {
            _indexItem = new Lazy<IndexItem>(getItem, LazyThreadSafetyMode.None);
            Operation = op;
        }

        /// <summary>
        /// Gets the Index item
        /// </summary>
        public IndexItem Item
        {
            get { return _indexItem.Value; }
        }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public IndexOperationType Operation { get; private set; }
    }
}