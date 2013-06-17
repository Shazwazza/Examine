using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace Examine.LuceneEngine.Indexing.Filters
{
    public class HtmlFilter : IValueFilter
    {
        public object Filter(object value)
        {
            var s = value as string;

            if (!string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(s);
                    if (doc.DocumentNode != null)
                    {
                        return doc.DocumentNode.InnerText;
                    }
                }
                catch { }                
            }

            return value;
        }
    }
}
