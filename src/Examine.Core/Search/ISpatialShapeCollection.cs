using System.Collections.Generic;

namespace Examine.Search
{
    /// <summary>
    /// A Collection of Shapes
    /// </summary>
    public interface ISpatialShapeCollection : ISpatialShape
    {
        /// <summary>
        /// Shapes in the Shape Collection
        /// </summary>
        IList<ISpatialShape> Shapes { get; }
    }
}
