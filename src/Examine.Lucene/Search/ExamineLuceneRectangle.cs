using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    public class ExamineLuceneRectangle : IExamineSpatialRectangle
    {
        private readonly IRectangle _rectangle;

        public ExamineLuceneRectangle(IRectangle rectangle)
        {
            _rectangle = rectangle;
        }
        public IExamineSpatialPoint Center => new ExamineLucenePoint(_rectangle.Center);

        public bool IsEmpty => false;

        public double MinX => _rectangle.MinX;

        public double MinY => _rectangle.MinY;

        public double MaxX => _rectangle.MaxX;

        public double MaxY => _rectangle.MaxY;
    }
}
