using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Spatial Point Shape
    /// </summary>
    public class ExamineLucenePoint : ExamineLuceneShape, ISpatialPoint
    {
        private readonly IPoint _point;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="point">Point Shape</param>
        public ExamineLucenePoint(IPoint point) : base(point)
        {
            _point = point;
        }

        /// <summary>
        /// The X coordinate, or Longitude in geospatial contexts.
        /// </summary>
        /// <returns></returns>
        public double X => _point.X;

        /// <summary>
        /// The Y coordinate, or Latitude in geospatial contexts.
        /// </summary>
        /// <returns></returns>
        public double Y => _point.Y;
    }
}
