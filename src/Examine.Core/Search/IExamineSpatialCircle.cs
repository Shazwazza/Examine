namespace Examine.Search
{
    /// <summary>
    /// Spatial Circle Shape
    /// </summary>
    public interface IExamineSpatialCircle : IExamineSpatialShape
    {
        /// <summary>
        /// Circle Radius
        /// </summary>
        double Radius { get; }
    }
}
