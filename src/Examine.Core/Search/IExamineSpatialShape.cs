using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public interface IExamineSpatialShape
    {
        IExamineSpatialPoint Center { get; }
        bool IsEmpty { get; }
    }
}
