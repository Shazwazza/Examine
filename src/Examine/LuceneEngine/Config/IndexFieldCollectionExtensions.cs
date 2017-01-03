using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.LuceneEngine.Config
{
    public static class IndexFieldCollectionExtensions
    {
        public static List<IndexField> ToList(this IndexFieldCollection indexes)
        {
            List<IndexField> fields = new List<IndexField>();
            foreach (IndexField field in indexes)
                fields.Add(field);
            return fields;
        }
    }
}
