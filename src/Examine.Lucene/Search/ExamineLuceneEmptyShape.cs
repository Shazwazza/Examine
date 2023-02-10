using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    public class ExamineLuceneEmptyShape : IExamineSpatialEmptyShape
    {
        public IExamineSpatialPoint Center => null;

        public bool IsEmpty => true;
    }
}
