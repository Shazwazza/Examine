using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Spatial Line String Shape
    /// </summary>
    public class ExamineLuceneLineString : ExamineLuceneShape, ISpatialLineString
    {
        private readonly IShape _lineString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lineString">Line String Shape</param>
        public ExamineLuceneLineString(IShape lineString) : base(lineString)
        {
            _lineString = lineString;
        }
    }
}
