using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    public class ExamineLuceneCircle : IExamineSpatialCircle
    {
        private readonly ICircle _circle;

        public ExamineLuceneCircle(ICircle circle)
        {
            _circle = circle;
        }

        public double Radius => _circle.Radius;

        public IExamineSpatialPoint Center => new ExamineLucenePoint(_circle.Center);

        public bool IsEmpty => false;
    }
}
