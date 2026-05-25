using System;
using System.Collections.Generic;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Holds a sequence of string components, specifying the hierarchical name of a category.
    /// </summary>
    internal readonly struct FacetLabel : IFacetLabel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="components">The components of this FacetLabel</param>
        public FacetLabel(string[] components)
        {
            Components = components;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dimension">The name of the dimension that stores this FacetLabel</param>
        /// <param name="components">The components of this FacetLabel</param>
        public FacetLabel(string dimension, string[] components)
        {
            Components = new string[1 + components.Length];
            Components[0] = dimension;
            Array.Copy(components, 0, Components, 1, components.Length);
        }

        /// <inheritdoc/>
        public string[] Components { get; }

        /// <inheritdoc/>
        public int Length => Components.Length;

        /// <inheritdoc/>
        public int CompareTo(IFacetLabel? other)
        {
            if (other == null)
            {
                return 1;
            }

            int len = Length < other.Length ? Length : other.Length;
            for (int i = 0, j = 0; i < len; i++, j++)
            {
                int cmp = StringComparer.Ordinal.Compare(Components[i], other.Components[j]);
                if (cmp < 0)
                {
                    return -1;
                }
                if (cmp > 0)
                {
                    return 1;
                }
            }

            return Length - other.Length;
        }

        /// <inheritdoc/>
        public IFacetLabel Subpath(int length)
        {
            if (Components.Length <= length)
            {
                return new FacetLabel(Components);
            }

            var subpathComponents = new List<string>(length);
            int index = 0;
            while (index < length && index < Components.Length)
            {
                subpathComponents.Add(Components[index]);
                index++;
            }
            return new FacetLabel(subpathComponents.ToArray());
        }
    }
}