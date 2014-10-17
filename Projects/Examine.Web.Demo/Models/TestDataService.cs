using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Indexing;

namespace Examine.Web.Demo.Models
{
    public class TestDataService : IValueSetService
    {
        public IEnumerable<ValueSet> GetAllData(string indexCategory)
        {
            switch (indexCategory)
            {
                case "Type1":
                    return Enumerable.Range(1, 100).Select(x => new ValueSet(x, indexCategory, "test", new
                    {
                        Name = x.ToString(CultureInfo.InvariantCulture),
                        Email = "test" + x.ToString(CultureInfo.InvariantCulture) + "@example.com"
                    }));
                case "Type2":
                    return Enumerable.Range(101, 200).Select(x => new ValueSet(x, indexCategory, "test", new
                    {
                        Name = x.ToString(CultureInfo.InvariantCulture),
                        Email = "test" + x.ToString(CultureInfo.InvariantCulture) + "@example.com"
                    }));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}