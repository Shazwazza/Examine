using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine
{
    public class IndexerExecutiveAssignedEventArgs : EventArgs
    {
        public IndexerExecutiveAssignedEventArgs(string machineName, int clusterNodeCount)
        {
            MachineName = machineName;
            ClusterNodeCount = clusterNodeCount;
        }

        public string MachineName { get; set; }
        public int ClusterNodeCount { get; set; }

    }
}
