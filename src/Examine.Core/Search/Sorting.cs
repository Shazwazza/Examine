using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public struct Sorting
    {
        public SortableField Field { get; }
        public SortDirection Direction { get; }

        public Sorting(SortableField field, SortDirection direction) {
            Field = field;
            Direction = direction;
        }
    }
}
