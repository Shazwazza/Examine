namespace Examine.Search
{
    /// <summary>
    /// Spatial Shape
    /// </summary>
    public interface IExamineSpatialShape
    {
        /// <summary>
        /// Center Point of Shape
        /// </summary>
        IExamineSpatialPoint Center { get; }

        /// <summary>
        /// Whether the Shape is Empty
        /// </summary>
        bool IsEmpty { get; }
    }
}
