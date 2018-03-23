using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Examine
{
    public class DeleteIndexEventArgs : EventArgs
    {

        public DeleteIndexEventArgs(IIndexer indexer, KeyValuePair<string, string> term)
        {
            Indexer = indexer;
            DeletedTerm = term;
        }

        public IIndexer Indexer { get; }
        public KeyValuePair<string, string> DeletedTerm { get; }

    }
}
