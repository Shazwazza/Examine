using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Indexing;

namespace Examine.LuceneEngine
{
    public interface IValueSetService
    {
        IEnumerable<ValueSet> GetAllData(string indexCategory);
    }
}
