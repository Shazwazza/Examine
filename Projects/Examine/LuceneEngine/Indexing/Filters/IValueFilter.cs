using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Indexing.Filters
{
    public interface IValueFilter
    {
        object Filter(object value);
    }
}
