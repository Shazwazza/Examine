using System.Collections.Generic;

namespace Examine.Search
{
    /// <summary>
    /// A Collection of Shapes
    /// </summary>
    public interface IExamineSpatialShapeCollection : IExamineSpatialShape
    {
        /// <summary>
        /// Shapes in the Shape Collection
        /// </summary>
        IList<IExamineSpatialShape> Shapes { get; }
    }
}
