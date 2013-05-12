using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Contrib.Management;

namespace Examine.Session
{
    public static class DisposableCollector
    {
        private static RequestScoped<Queue<HashSet<IDisposable>>> _disposables =
            new RequestScoped<Queue<HashSet<IDisposable>>>(() =>
                {
                    var q = new Queue<HashSet<IDisposable>>();
                    q.Enqueue(new HashSet<IDisposable>());
                    return q;
                });



        public static void Track(IDisposable disposable)
        {
            _disposables.Value.Peek().Add(disposable);
        }

        public static void Untrack(IDisposable disposable)
        {
            _disposables.Value.Peek().Remove(disposable);
        }

        private static void CleanScope(HashSet<IDisposable> scope)
        {
            lock (scope)
            {
                DisposeUtil.Dispose(scope.ToArray());
                scope.Clear();
            }
        }

        public static void Clean()
        {
            var disposables = _disposables.Value;
            while (disposables.Count > 0)
            {
                CleanScope(disposables.Dequeue());
            }
            _disposables.Reset();

        }

        public static IDisposable OpenScope()
        {
            var scope = new HashSet<IDisposable>();
            _disposables.Value.Enqueue(scope);

            return new DisposableScope(() => CleanScope(_disposables.Value.Dequeue()));
        }

        private class DisposableScope : IDisposable
        {
            private readonly Action _disposeAction;

            public DisposableScope(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            private bool _disposed = false;
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                _disposeAction();
            }
        }
    }
}