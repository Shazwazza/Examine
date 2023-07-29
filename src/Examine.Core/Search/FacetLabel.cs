using System;
using System.Collections.Generic;

namespace Examine.Search
{
    /// <summary>
    /// Holds a sequence of string components, specifying the hierarchical name of a category.
    /// </summary>
    public readonly struct FacetLabel : IFacetLabel
    {
        private readonly string[] _components;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="components">The components of this FacetLabel</param>
        public FacetLabel(string[] components)
        {
            _components = components;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dimension">The name of the dimension that stores this FacetLabel</param>
        /// <param name="components">>The components of this FacetLabel</param>
        public FacetLabel(string dimension, string[] components)
        {
            _components = new string[1 + components.Length];
            _components[0] = dimension;
            Array.Copy(components, 0, _components, 1, components.Length);
        }

        /// <inheritdoc/>
        public string[] Components => _components;

        /// <inheritdoc/>
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
