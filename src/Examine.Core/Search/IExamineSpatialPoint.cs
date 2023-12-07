namespace Examine.Search
{
    /// <summary>
    /// Spatial Point Shape
    /// </summary>
    public interface IExamineSpatialPoint : IExamineSpatialShape
    {
        /// <summary>
        /// The X coordinate, or Longitude in geospatial contexts.
        /// </summary>
        double X { get; }

        /// <summary>
        ///  The Y coordinate, or Latitude in geospatial contexts.
        /// </summary>
        double Y { get; }
    }
}
