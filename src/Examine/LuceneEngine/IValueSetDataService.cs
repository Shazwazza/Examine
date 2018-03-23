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
        /// <param name="category"></param>
        /// <returns></returns>
        IEnumerable<ValueSet> GetAllData(string category);

        
    }
}