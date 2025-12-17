namespace Examine.Search
{
    /// <summary>
    /// Spatial Circle Shape
    /// </summary>
    public interface ISpatialCircle : ISpatialShape
    {
        /// <summary>
        /// Circle Radius
        /// </summary>
        double Radius { get; }
    }
}
