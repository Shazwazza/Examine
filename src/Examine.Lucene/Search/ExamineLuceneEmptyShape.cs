using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Spatial4n.Shapes;

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
