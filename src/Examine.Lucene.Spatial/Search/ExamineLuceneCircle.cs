using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    public class ExamineLuceneCircle : ExamineLuceneShape, IExamineSpatialCircle
    {
        private readonly ICircle _circle;

        public ExamineLuceneCircle(ICircle circle) : base(circle)
        {
            _circle = circle;
        }

        public double Radius => _circle.Radius;

    }
}
