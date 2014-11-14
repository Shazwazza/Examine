using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Cru;
using Lucene.Net.Analysis;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Config
{
    /// <summary>
    /// Manages a list of SearcherContext objects
    /// </summary>
    public class SearcherContextCollection : IDisposable
    {
        internal delegate object DirectoryKeyAdapter(Directory dir);

        private static readonly SearcherContextCollection _instance = new SearcherContextCollection();
        
        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static SearcherContextCollection Instance { get { return _instance; } }

        internal IEnumerable<DirectoryKeyAdapter> KeyAdapters { get; private set; }

        private readonly ConcurrentDictionary<object, SearcherContext> _contexts;

        /// <summary>
        /// ctor
        /// </summary>
        public SearcherContextCollection()
        {
            KeyAdapters = new List<DirectoryKeyAdapter>
                {
                    dir=> { var fsDir = dir as FSDirectory;
                              return fsDir != null ? fsDir.Directory.FullName : null;
                    },
                    dir=> dir as RAMDirectory
                };

            _contexts = new ConcurrentDictionary<object, SearcherContext>();
        }

        /// <summary>
        /// Registers a context
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public SearcherContext RegisterContext(SearcherContext ctx)
        {
            if (!_contexts.TryAdd(GetKey(ctx.Directory), ctx))
            {
                throw new InvalidOperationException("Context with the same directory is already added");
            }

            return ctx;
        }

        private readonly object _createLock = new object();

        /// <summary>
        /// Removes a context
        /// </summary>
        /// <param name="ctx"></param>
        public void RemoveContext(SearcherContext ctx)
        {
            lock (_createLock)
            {
                var key = GetKey(ctx.Directory);
                if (_contexts.TryRemove(key, out ctx))
                {
                    ctx.Dispose();
                    return;
                }
            }

            throw new KeyNotFoundException("No context is created for the directory");
        }

        /// <summary>
        /// Returns a context based on a Directory instance
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public SearcherContext GetContext(Directory dir)
        {
            SearcherContext ctx;
            return _contexts.TryGetValue(GetKey(dir), out ctx) ? ctx : null;
        }
        
        private object GetKey(Directory dir)
        {
            var key = KeyAdapters.Select(a => a(dir)).FirstOrDefault(k => k != null);
            if (key == null)
            {
                throw new NotSupportedException(string.Format("Directory class {0} is not supported. Register a DirectoryKeyAdapater for the type.", dir.GetType().Name));
            }
            return key;
        }


        public void Dispose()
        {
            DisposeUtil.PostponeExceptions(_contexts.Values.Select(v => (Action)v.Dispose).ToArray());
        }
    }
    
}
