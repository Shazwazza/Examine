using Examine.Search;

namespace Examine
{
    /// <summary>
    /// Spatial Index Field Value Type Shape Factory
    /// </summary>
    public interface ISpatialIndexFieldValueTypeShapesBase
    {
        /// <summary>
        /// Gets the Shape Factory for the fields spatial strategy
        /// </summary>
        IExamineSpatialShapeFactory ExamineSpatialShapeFactory { get; }
    }
}
