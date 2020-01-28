using System;
using System.ComponentModel;
using System.Web.Hosting;
using Examine.LuceneEngine;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers for ASP.Net Framework applications
    ///</summary>
    public class AspExamineManager : ExamineManager, IRegisteredObject
    {
        //tracks if the ExamineManager should register itself with the HostingEnvironment
        private static volatile bool _defaultRegisteration = true;

        /// <summary>
        /// By default the <see cref="ExamineManager"/> will use itself to to register against the HostingEnvironment for tracking
        /// app domain shutdown. In some cases a library may wish to manage this shutdown themselves in which case this can be called
        /// on startup to disable the default registration.
        /// </summary>        
        /// <returns></returns>
        public static void DisableDefaultHostingEnvironmentRegistration()
        {
            if (!_defaultRegisteration) return;
            _defaultRegisteration = false;

            var instance = Instance;
            if (instance is AspExamineManager e) HostingEnvironment.UnregisterObject(e);
        }

        private AspExamineManager()
        {
            if (!_defaultRegisteration) return;
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => Dispose();
            HostingEnvironment.RegisterObject(this);
        }

        /// <summary>
        /// Returns true if this singleton has been initialized
        /// </summary>
        public static bool InstanceInitialized { get; private set; }

        /// <summary>
        /// Singleton instance - but it's much more preferable to use IExamineManager
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IExamineManager Instance
        {
            get
            {
                InstanceInitialized = true;
                return Manager;
            }
        }

        private static readonly ExamineManager Manager = new ExamineManager();

        /// <summary>
        /// Requests a registered object to unregister on app domain unload in a web project
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should unregister from the hosting environment before returning; otherwise, false.</param>
        public override void Stop(bool immediate)
        {
            base.Stop(immediate);
            if (immediate)
            {   
                try
                {                    
                    OpenReaderTracker.Current.CloseAllReaders();
                }
                catch
                {
                    // we don't want to kill the app or anything, even though it is terminating, best to just ensure that 
                    // no strange lucene background thread stuff causes issues here.
                }
                finally
                {
                    //unregister if the default registration was used
                    if (_defaultRegisteration)
                        HostingEnvironment.UnregisterObject(this);
                }
            }
        }
    }
}