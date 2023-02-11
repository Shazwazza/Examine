using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    public class ExamineLuceneLineString : ExamineLuceneShape, IExamineSpatialLineString
    {
        private readonly IShape _lineString;

        public ExamineLuceneLineString(IShape lineString) : base(lineString)
        {
            _lineString = lineString;
        }

    }
}
