using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Contrib.Management;

namespace Examine.Session
{

    //TODO: I'm not sure this class is thread safe ???

    internal static class DisposableCollector
    {
        private static readonly RequestScoped<Queue<HashSet<IDisposable>>> Disposables =
            new RequestScoped<Queue<HashSet<IDisposable>>>(() =>
                {
                    var q = new Queue<HashSet<IDisposable>>();
                    q.Enqueue(new HashSet<IDisposable>());
                    return q;
                });



        public static void Track(IDisposable disposable)
        {
            Disposables.Value.Peek().Add(disposable);
        }

        public static void Untrack(IDisposable disposable)
        {
            Disposables.Value.Peek().Remove(disposable);
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
            var disposables = Disposables.Value;
            while (disposables.Count > 0)
            {
                CleanScope(disposables.Dequeue());
            }
            Disposables.Reset();

        }

        public static IDisposable OpenScope()
        {
            var scope = new HashSet<IDisposable>();
            Disposables.Value.Enqueue(scope);

            return new DisposableScope(() => CleanScope(Disposables.Value.Dequeue()));
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