using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    public class ExamineLuceneRectangle : ExamineLuceneShape, IExamineSpatialRectangle
    {
        private readonly IRectangle _rectangle;

        public ExamineLuceneRectangle(IRectangle rectangle) : base(rectangle) 
        {
            _rectangle = rectangle;
        }

        public double MinX => _rectangle.MinX;

        public double MinY => _rectangle.MinY;

        public double MaxX => _rectangle.MaxX;

        public double MaxY => _rectangle.MaxY;
    }
}
