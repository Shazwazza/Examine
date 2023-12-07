using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Lucene.Net Shape
    /// </summary>
    public class ExamineLuceneShape : IExamineSpatialShape
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shape"></param>
        public ExamineLuceneShape(IShape shape)
        {
            Shape = shape;
        }

        ///<inheritdoc/>
        public IExamineSpatialPoint Center => new ExamineLucenePoint(Shape.Center);

        ///<inheritdoc/>
        public virtual bool IsEmpty => Shape.IsEmpty;

        /// <summary>
        /// Shape
        /// </summary>
        public IShape Shape { get; }
    }
}
