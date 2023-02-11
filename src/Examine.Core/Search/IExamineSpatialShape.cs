using System.Collections;
using System.Collections.Generic;

namespace Examine.Search
{
    public interface IExamineSpatialShape
    {
        IExamineSpatialPoint Center { get; }
        bool IsEmpty { get; }
    }
}
