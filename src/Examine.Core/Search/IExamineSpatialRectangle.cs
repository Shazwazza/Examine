namespace Examine.Search
{
    /// <summary>
    /// Spatial Rectangle Shape
    /// </summary>
    public interface IExamineSpatialRectangle : IExamineSpatialShape
    {
        /// <summary>The left edge of the X coordinate.</summary>
        double MinX { get; }

        /// <summary>The bottom edge of the Y coordinate.</summary>
        double MinY { get; }

        /// <summary>The right edge of the X coordinate.</summary>
        double MaxX { get; }

        /// <summary>The top edge of the Y coordinate.</summary>
        double MaxY { get; }
    }
}
