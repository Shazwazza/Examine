using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.DataStructures
{
    public static class DataHelpers
    {
        public static IEnumerable<int> GetDocIds(this DocIdSet set)
        {
            var it = set.Iterator();
            int doc;
            while ((doc = it.NextDoc()) != DocIdSetIterator.NO_MORE_DOCS)
            {
                yield return doc;
            }
        }

        public static bool IsNullOrEmpty(this IEnumerable e)
        {
            if( e != null )
            {
                foreach (var el in e) return true;
            }
            return true;
        }


        /// <summary>
        /// This method is typically faster than sorting all elements in the array. It uses a priority queue to only sort the count first elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="count"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetTopItems<T>(this IEnumerable<T> items, int count, IComparer<T> comparer)
        {
            var set = new SortedSet<T>(comparer);
            T max = default(T);
            foreach (var item in items)
            {
                if (set.Count < count)
                {
                    if (set.Count == 0 || comparer.Compare(item, max) > 0)
                {
                        max = item;
                    }
                    set.Add(item);
                }
                else if (comparer.Compare(item, max) < 0)
                {
                    set.Remove(max);
                    set.Add(item);
                    max = set.Max;
                }
            }

            return set;
        }
    }
}
