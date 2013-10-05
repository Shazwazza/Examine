using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Contrib.Management;
using LuceneManager.Infrastructure;

namespace Examine.Session
{
    //TODO : I'm not sure if this is thread safe ??

    /// <summary>
    /// 
    /// </summary>
    public static class ExamineSession
    {
        private static readonly RequestScoped<Dictionary<NrtManager, long>> CurrentGeneration =
            new RequestScoped<Dictionary<NrtManager, long>>(()=>new Dictionary<NrtManager, long>());

        internal static void TrackGeneration(NrtManager manager, long generation)
        {
            CurrentGeneration.Value[manager] = generation;
        }

        public static void WaitForChanges()
        {
            foreach (var manager in CurrentGeneration.Value)
            {
                manager.Key.WaitForGeneration(manager.Value);
            }
        }

        public static void WaitForChanges(NrtManager manager)
        {
            long generation;
            if (CurrentGeneration.Value.TryGetValue(manager, out generation))
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
