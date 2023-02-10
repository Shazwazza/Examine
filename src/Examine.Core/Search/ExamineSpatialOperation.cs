using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public enum ExamineSpatialOperation
    {
        Intersects = 0,
        Overlaps = 1,
        IsWithin = 2,
        BoundingBoxIntersects = 3,
        BoundingBoxWithin = 4,
        Contains = 5,
        IsDisjointTo = 6,
        IsEqualTo = 7
    }
}
