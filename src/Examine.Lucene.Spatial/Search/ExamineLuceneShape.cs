using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    public class ExamineLuceneShape : IExamineSpatialShape
    {
        public ExamineLuceneShape(IShape shape)
        {
            Shape = shape;
        }
        public IExamineSpatialPoint Center => new ExamineLucenePoint(Shape.Center);

        public virtual bool IsEmpty => Shape.IsEmpty;

        public IShape Shape { get; }
    }
}
