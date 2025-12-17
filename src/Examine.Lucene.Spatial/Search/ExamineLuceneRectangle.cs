using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Spatial Rectangle Shape
    /// </summary>
    public class ExamineLuceneRectangle : ExamineLuceneShape, ISpatialRectangle
    {
        private readonly IRectangle _rectangle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rectangle">Rectangle Shape</param>
        public ExamineLuceneRectangle(IRectangle rectangle) : base(rectangle)
        {
            _rectangle = rectangle;
        }

        /// <inheritdoc/>
        public double MinX => _rectangle.MinX;

        /// <inheritdoc/>
        public double MinY => _rectangle.MinY;

        /// <inheritdoc/>
        public double MaxX => _rectangle.MaxX;

        /// <inheritdoc/>
        public double MaxY => _rectangle.MaxY;
    }
}
