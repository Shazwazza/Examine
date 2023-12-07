using System;

namespace Examine.Search
{
    /// <summary>
    /// Holds a sequence of string components, specifying the hierarchical name of a category.
    /// </summary>
    public interface IFacetLabel : IComparable<IFacetLabel>
    {
        /// <summary>
        /// The components of this IFacetLabel.
        /// </summary>
        string[] Components { get; }

        /// <summary>
        /// The number of components of this IFacetLabel.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Returns a sub-path of this path up to length components.
        /// </summary>
        IFacetLabel Subpath(int length);
    }
}
