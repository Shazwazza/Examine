using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;

namespace Examine.Lucene.Indexing
{
    public static class DocumentExtensions
    {
        // TODO: Make this happen

        public static void AddOrSetValue<TField>(
            this Document doc,
            string fieldName,
            object value)
            where TField : Field
        {
            if (doc.GetField(fieldName) is TextField existing)
            {
                existing.SetStringValue((string)value);
            }
            else
            {
                doc.Add(new TextField(fieldName, value, Field.Store.YES));
            }
        }
    }
}
