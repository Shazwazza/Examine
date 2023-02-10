using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    public class ExamineLucenePoint : IExamineSpatialPoint
    {
        private readonly IPoint _point;

        public ExamineLucenePoint(IPoint point)
        {
            _point = point;
        }

        public double X => _point.X;

        public double Y => _point.Y;
    }
}
