using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine
{
    public interface IValueSetDataService
    {
        /// <summary>
        /// Returns a collection of <see cref="ValueSet"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<ValueSet> GetAllData();
    }
}