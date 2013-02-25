using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetCounts : IEnumerable<KeyValuePair<FacetKey, int>>
    {
        public static int GrowFactor = 2048;

        public int[] Counts { get; private set; }

        public FacetMap FacetMap { get; private set; }


        public void Reset(FacetMap map)
        {
            FacetMap = map;
            if (Counts == null || Counts.Length < map.Keys.Count)
            {
                Counts = new int[GrowFactor * (1 + map.Keys.Count / GrowFactor)];
            }
            else
            {
                Array.Clear(Counts, 0, Counts.Length);
            }
        }

        public int GetCount(FacetKey key)
        {
            var index = FacetMap.GetIndex(key);
            return index > -1 && index < Counts.Length ? Counts[index] : 0;
        }

        public IEnumerator<KeyValuePair<FacetKey, int>> GetEnumerator()
        {
            var n = Counts.Length;
            foreach( var f in FacetMap)
            {
                if (f.Key < n)
                {
                    yield return new KeyValuePair<FacetKey, int>(f.Value, Counts[f.Key]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}