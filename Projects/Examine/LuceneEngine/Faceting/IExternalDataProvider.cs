using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    public interface IExternalDataProvider
    {
        object GetData(long id);
    }
}
