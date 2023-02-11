using Examine.Search;

namespace Examine.Lucene.Search
{
    public class ExamineLuceneEmptyShape : ExamineLuceneShape, IExamineSpatialEmptyShape
    {
        public ExamineLuceneEmptyShape() : base(null)
        {
        }
        public override bool IsEmpty => true;
    }
}
