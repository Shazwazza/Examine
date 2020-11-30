using System.Collections;
using System.Collections.Generic;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.DeletePolicies
{
    public class NoDeletionPolicy : IndexDeletionPolicy
    {
        
        private NoDeletionPolicy()
        {
            // keep private to avoid instantiation
        }
        
        public static readonly IndexDeletionPolicy INSTANCE = new NoDeletionPolicy();
        public void OnInit(IList commits)
        {
        }

        public void OnCommit(IList commits)
        {
            throw new System.NotImplementedException()
        }
    }
}