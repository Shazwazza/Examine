using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine
{
    public class DeleteIndexEventArgs : EventArgs
    {

        public DeleteIndexEventArgs(KeyValuePair<string, string> term)
        {
            DeletedTerm = term;
        }

        public KeyValuePair<string, string> DeletedTerm { get; private set; }

    }
}
