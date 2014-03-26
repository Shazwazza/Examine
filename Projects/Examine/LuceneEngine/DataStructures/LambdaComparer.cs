using System;
using System.Collections.Generic;

namespace Examine.LuceneEngine.DataStructures
{
    internal class LambdaComparer<T> : IComparer<T>
    {
        private Func<T, T, int> _compareFn;

        public LambdaComparer(Func<T, T, int> fn)
        {
            _compareFn = fn;
        }

        public int Compare(T x, T y)
        {
            return _compareFn(x, y);
        }    
    }
}
