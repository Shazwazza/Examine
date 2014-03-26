using System;
using System.Linq;

namespace Examine.LuceneEngine.Cru
{
    internal static class DisposeUtil
    {
        public static void Dispose(params IDisposable[] disposables)
        {
            PostponeExceptions(disposables.Select(d=>(Action)d.Dispose).ToArray());
        }

        public static void PostponeExceptions(params Action[] disposeActions)
        {
            Exception firstException = null;
            foreach (var d in disposeActions)
            {
                try
                {
                    d();
                }
                catch (Exception ex)
                {
                    firstException = firstException ?? ex;
                }
            }

            if (firstException != null) throw firstException;
        }
    }
}