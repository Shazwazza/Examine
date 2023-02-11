using Examine.Search;

namespace Examine.Lucene.Spatial.Search
{
    public class ExamineLuceneEmptyShape : ExamineLuceneShape, IExamineSpatialEmptyShape
    {
        public ExamineLuceneEmptyShape() : base(null)
        {
        }
        public override bool IsEmpty => true;
    }
}
