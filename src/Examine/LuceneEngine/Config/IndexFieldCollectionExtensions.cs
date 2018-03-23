using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.LuceneEngine.Config
{
    public static class IndexFieldCollectionExtensions
    {
        public static List<ConfigIndexField> ToList(this IndexFieldCollection indexes)
        {
            List<ConfigIndexField> fields = new List<ConfigIndexField>();
            foreach (ConfigIndexField field in indexes)
                fields.Add(field);
            return fields;
        }
    }
}
