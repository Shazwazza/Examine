using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Examine.LuceneEngine.Cru;

namespace Examine.Session
{
    /// <summary>
    /// Used by Examine classes to track the current generation of the NrtManager
    /// so that the Examine classes can specifically wait for outstanding operations to complete
    /// if necessary given the current generation.
    /// </summary>
    /// <remarks>
    /// This will only work when then NrtManager.Tracker property is set to use the TrackGeneration method, otherwise
    /// the ExamineSession instance performs no function.
    /// </remarks>
    public static class ExamineSession
    {
        private static readonly RequestScoped<Dictionary<NrtManager, long>> CurrentGeneration =
            new RequestScoped<Dictionary<NrtManager, long>>(()=>new Dictionary<NrtManager, long>());

        /// <summary>
        /// This gets called everytime the NrtManager updates/deletes a document
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="generation"></param>
        /// <remarks>
        /// This is called only when NrtManager.Tracker property is set to use this delegate
        /// </remarks>
        internal static void TrackGeneration(NrtManager manager, long generation)
        {
            CurrentGeneration.Instance[manager] = generation;
        }

        /// <summary>
        /// Waits for any outstanding operations to finish for the currently scoped NrtManager
        /// </summary>
        public static void WaitForChanges()
        {
            foreach (var manager in CurrentGeneration.Instance)
            {
                manager.Key.WaitForGeneration(manager.Value);
            }
        }

        /// <summary>
        /// Waits for any outstanding operations to finish for the specified NrtManager
        /// </summary>
        /// <param name="manager"></param>
        internal static void WaitForChanges(NrtManager manager)
        {
            long generation;
            if (CurrentGeneration.Instance.TryGetValue(manager, out generation))
            {
                manager.WaitForGeneration(generation);
            }
        }

        /// <summary>
        /// A RequestScoped boolean to track 
        /// </summary>
        private static readonly RequestScoped<bool> RequireImmediateConsistencyInternal = new RequestScoped<bool>(() => false);

        /// <summary>
        /// A current thread scoped flag for whether or not an indexer or searcher requires
        /// immediate results and must wait for any async indexing operations to complete 
        /// before continuing.
        /// </summary>
        /// <remarks>
        /// You would use this for example if you were creating some items in the underlying data store, and then indexed these items and wanted 
        /// to be able to search on this data in the index immediately in the same thread.
        /// </remarks>
        public static bool RequireImmediateConsistency
        {
            get { return RequireImmediateConsistencyInternal.Instance; }
            set { RequireImmediateConsistencyInternal.Instance = value; }
        }
    }
}
