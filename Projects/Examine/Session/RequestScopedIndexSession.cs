using System;
using System.Web;
using Examine.LuceneEngine.Cru;

namespace Examine.Session
{
    /// <summary>
    /// Scopes the index session to the http context
    /// </summary>
    public class RequestScopedIndexSession : IDisposable
    {
        private readonly HttpContextBase _httpContext;
        
        public RequestScopedIndexSession(HttpContextBase httpContext, params SearcherContext[] searcherContexts)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            _httpContext = httpContext;
            if (_httpContext.Items[this] == null)
            {
                _httpContext.Items[this] = new ThreadScopedIndexSession(searcherContexts);
            }
        }

        public void WaitForChanges()
        {
            var session = (ThreadScopedIndexSession)_httpContext.Items[this];
            session.WaitForChanges();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            var session = (ThreadScopedIndexSession)_httpContext.Items[this];
            session.Dispose();
        }
    }
}