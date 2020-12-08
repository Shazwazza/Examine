using System.Collections.Generic;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.DeletePolicies
{
    public class NoDeletionPolicy : IndexDeletionPolicy
    {
        public void OnInit<T>(IList<T> commits) where T : IndexCommit
        {
            
        }
        public NoDeletionPolicy()
        {
            // keep private to avoid instantiation
        }
        public void OnCommit<T>(IList<T> commits) where T : IndexCommit
        {
        }
    }
}