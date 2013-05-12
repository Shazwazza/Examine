using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Contrib.Management;
using LuceneManager.Infrastructure;

namespace Examine.Session
{
    public static class ExamineSession
    {
        private static readonly RequestScoped<Dictionary<NrtManager, long>> _currentGeneration =
            new RequestScoped<Dictionary<NrtManager, long>>(()=>new Dictionary<NrtManager, long>());

        public static void TrackGeneration(NrtManager manager, long generation)
        {
            _currentGeneration.Value[manager] = generation;
        }

        public static void WaitForChanges()
        {
            foreach (var manager in _currentGeneration.Value)
            {
                manager.Key.WaitForGeneration(manager.Value);
            }
        }

        public static void WaitForChanges(NrtManager manager)
        {
            long generation;
            if (_currentGeneration.Value.TryGetValue(manager, out generation))
            {
                manager.WaitForGeneration(generation);
            }
        }

        private static readonly RequestScoped<bool> _requireImmediateConsistency = new RequestScoped<bool>(() => false);

        public static bool RequireImmediateConsistency
        {
            get { return _requireImmediateConsistency.Value; }
            set { _requireImmediateConsistency.Value = value; }
        }
    }
}
