namespace Examine.Search
{
    /// <summary>
    /// Spatial Shape
    /// </summary>
    public interface ISpatialShape
    {
        /// <summary>
        /// Center Point of Shape
        /// </summary>
        ISpatialPoint Center { get; }

        /// <summary>
        /// Whether the Shape is Empty
        /// </summary>
        bool IsEmpty { get; }
    }
}
