using System;
using System.Collections.Generic;
using System.Linq;

namespace Examine.AzureSearch
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> InGroupsOf<T>(this IEnumerable<T> source, int groupSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (groupSize <= 0)
                throw new ArgumentException("Must be greater than zero.", nameof(groupSize));

            return source
                .Select((x, i) => Tuple.Create<int, T>(i / groupSize, x))
                .GroupBy(t => t.Item1, t => t.Item2);
        }
    }
}