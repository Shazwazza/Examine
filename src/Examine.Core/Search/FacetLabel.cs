using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public readonly struct FacetLabel : IFacetLabel
    {
        private readonly string[] _components;

        public FacetLabel(string[] components)
        {
            _components = components;
        }
        public FacetLabel(string dimension, string[] components)
        {
            _components = new string[1 + components.Length];
            _components[0] = dimension;
            Array.Copy(components, 0, _components, 1, components.Length);
        }

        public string[] Components => _components;

        public int Length => _components.Length;

        // From Lucene.NET
        public int CompareTo(IFacetLabel other)
        {
            int len = Length < other.Length ? Length : other.Length;
            for (int i = 0, j = 0; i < len; i++, j++)
            {
                int cmp = StringComparer.Ordinal.Compare(Components[i], other.Components[j]);
                if (cmp < 0)
                {
                    return -1; // this is 'before'
                }
                if (cmp > 0)
                {
                    return 1; // this is 'after'
                }
            }

            // one is a prefix of the other
            return Length - other.Length;
        }


        /// <inheritdoc/>
        public IFacetLabel Subpath(int length)
        {
            if(Components.Length <= length)
            {
                return new FacetLabel(Components);
            }

            List<string> subpathComponents = new List<string>(length);
            int index = 0;
            while (index < length && index < _components.Length)
            {
                subpathComponents.Add(_components[index]);
                index++;
            }
            return new FacetLabel(subpathComponents.ToArray());
        }
    }
}
