using System.Collections.Generic;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.DeletePolicies
{
    public class NoDeletionPolicy : IndexDeletionPolicy
    {
        public void OnInit<T>(IList<T> commits) where T : IndexCommit
        {
            
        }
        private NoDeletionPolicy()
        {
            // keep private to avoid instantiation
        }
        public void OnCommit<T>(IList<T> commits) where T : IndexCommit
        {
        }
        private static readonly IndexDeletionPolicy _instance = new NoDeletionPolicy();
        public static IndexDeletionPolicy INSTANCE => _instance;
    }
}