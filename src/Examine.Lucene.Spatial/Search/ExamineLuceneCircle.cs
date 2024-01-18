using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Spatial Circle Shape
    /// </summary>
    public class ExamineLuceneCircle : ExamineLuceneShape, ISpatialCircle
    {
        private readonly ICircle _circle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="circle">Circle</param>
        public ExamineLuceneCircle(ICircle circle) : base(circle)
        {
            _circle = circle;
        }

        /// <inheritdoc/>
        public double Radius => _circle.Radius;

    }
}
