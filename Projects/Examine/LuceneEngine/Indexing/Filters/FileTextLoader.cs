using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPocalipse.IFilter;

namespace Examine.LuceneEngine.Indexing.Filters
{
    public class FileTextFilter : IValueFilter
    {
        private readonly Func<string, string> _pathMapper;

        public FileTextFilter(Func<string, string> pathMapper)
        {
            _pathMapper = pathMapper;
        }

        public object Filter(object value)
        {
            var s = value as string;
            if (!string.IsNullOrWhiteSpace(s))
            {
                var path = _pathMapper(s);

                try
                {
                    using (var r = new FilterReader(path))
                    {
                        return r.ReadToEnd();
                    }
                }
                catch
                {
                    
                }
            }

            return null;
        }
    }
}
