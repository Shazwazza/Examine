using System;
using System.Collections.Generic;
using Examine.LuceneEngine.Cru;

namespace Examine.Session
{
    /// <summary>
    /// Used to isolate a session for indexing on the current thread in order to wait for index changes
    /// </summary>
    /// <remarks>
    /// Generally used for single threaded index operations such as unit tests
    /// </remarks>
    public class ThreadScopedIndexSession : IDisposable
    {
        private readonly SearcherContext[] _searcherContexts;
        private readonly Dictionary<NrtManager, long> _currentGeneration = new Dictionary<NrtManager, long>();
        private readonly Dictionary<NrtManager, Action<NrtManager, long>> _originalTrackers = new Dictionary<NrtManager, Action<NrtManager, long>>();
     
        public ThreadScopedIndexSession(params SearcherContext[] searcherContexts)
        {
            _searcherContexts = searcherContexts;
            Init();
        }

        /// <summary>
        /// Save the original tracking method for each NrtManager and set it's new on to use this object's
        /// </summary>
        private void Init()
        {
            foreach (var searcherContext in _searcherContexts)
            {
                _originalTrackers[searcherContext.Manager] = searcherContext.Manager.Tracker;
                searcherContext.Manager.Tracker = TrackGeneration;
            }
        }

        /// <summary>
        /// This gets called everytime the NrtManager updates/deletes a document
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="generation"></param>        
        private void TrackGeneration(NrtManager manager, long generation)
        {
            _currentGeneration[manager] = generation;
        }

        public void WaitForChanges()
        {
            foreach (var manager in _currentGeneration)
            {
                manager.Key.WaitForGeneration(manager.Value);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            WaitForChanges();
            foreach (var searcherContext in _searcherContexts)
            {
                //put back original
                searcherContext.Manager.Tracker = _originalTrackers[searcherContext.Manager];
            }
        }
    }
}