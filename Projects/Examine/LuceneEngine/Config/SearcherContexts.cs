using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Store;
using LuceneManager.Infrastructure;

namespace Examine.LuceneEngine.Config
{
    public class SearcherContexts : IDisposable
    {
        private static readonly SearcherContexts _instance = new SearcherContexts();

        public static SearcherContexts Instance { get { return _instance; } }


        public List<DirectoryKeyAdapter> KeyAdapters { get; private set; }

        private readonly ConcurrentDictionary<object, SearcherContext> _contexts;

        public SearcherContexts()
        {
            KeyAdapters = new List<DirectoryKeyAdapter>
                {
                    dir=> { var fsDir = dir as FSDirectory;
                              return fsDir != null ? fsDir.GetDirectory().FullName : null;
                    },
                    dir=> dir as RAMDirectory
                };

            _contexts = new ConcurrentDictionary<object, SearcherContext>();
        }

        public SearcherContext RegisterContext(SearcherContext ctx)
        {
            if (!_contexts.TryAdd(ctx.Directory, ctx))
            {
                throw new InvalidOperationException("Context with the same directory is already added");
            }

            return ctx;
        }

        private object _createLock = new object();
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

        public SearcherContext GetContext(Directory dir)
        {
            SearcherContext ctx;
            return _contexts.TryGetValue(GetKey(dir), out ctx) ? ctx : null;
        }



        object GetKey(Directory dir)
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

    public delegate object DirectoryKeyAdapter(Directory dir);
}
