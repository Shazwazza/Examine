using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LuceneManager.Infrastructure.DataStructures;

namespace Examine.LuceneEngine.Facets
{
    public class FacetCounts : IEnumerable<KeyValuePair<FacetKey, int>>
    {
        public static int GrowFactor = 2048;
        
        public LittleBigArray Counts { get; private set; }

        public FacetMap FacetMap { get; private set; }


        public void Reset(FacetMap map)
        {
            FacetMap = map;            
            if (Counts == null || Counts.Length < map.Keys.Count)
            {
                Counts = new LittleBigArray(GrowFactor * (1 + map.Keys.Count / GrowFactor));
            }
            else
            {
                Counts.Reset();
            }
        }

        public int this[FacetKey key]
        {
            get { return GetCount(key); }
        }

        public int GetCount(FacetKey key)
        {
            var index = FacetMap.GetIndex(key);
            return index > -1 && index < Counts.Length ? Counts[index] : 0;
        }

        public IEnumerable<KeyValuePair<FacetKey, int>> GetTopFacets(int count, params string[] fieldNames)
        {
            var facets = fieldNames.IsNullOrEmpty() ? GetNonEmpty()
                : FacetMap.GetByFieldNames(fieldNames).Select(f => new KeyValuePair<FacetKey, int>(f.Value, Counts[f.Key]));

            return facets.GetTopItems(count, 
                
                new LambdaComparer<KeyValuePair<FacetKey, int>>((x, y) =>
                    {
                        var c = y.Value.CompareTo(x.Value);
                        return c == 0 ? x.Key.CompareTo(y.Key) : c;
                    }));
        } 

        public IEnumerable<KeyValuePair<FacetKey, int>> GetNonEmpty()
        {
            return Counts.Select(f => new KeyValuePair<FacetKey, int>(FacetMap.Keys[f.Key], f.Value));
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