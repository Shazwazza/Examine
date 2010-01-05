using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine.Core
{
    public class DeleteIndexEventArgs : EventArgs
    {

        public DeleteIndexEventArgs(KeyValuePair<string, string> term, int numDeleted)
        {
            DeletedTerm = term;
            DeletionCount = numDeleted;
        }

        public KeyValuePair<string, string> DeletedTerm { get; private set; }
        public int DeletionCount { get; private set; }

    }
}
