using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Cru
{
    public class CompositeSearcherWarmer : ISearcherWarmer, IDisposable
    {
        private readonly IEnumerable<ISearcherWarmer> _warmers;

        public CompositeSearcherWarmer(IEnumerable<ISearcherWarmer> warmers)
        {
            _warmers = warmers;
        }

        public void Warm(IndexSearcher s)
        {
            foreach (var w in _warmers)
            {
                w.Warm(s);
            }
        }

        public void Dispose()
        {
            foreach (var w in _warmers)
            {
                var d = w as IDisposable;
                if (d != null)
                {
                    d.Dispose();
                }
            }
        }
    }
}
