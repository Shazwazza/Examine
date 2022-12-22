using System;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class TaxonomyFacetType : RawStringType
    {
        public TaxonomyFacetType(string fieldName, ILoggerFactory logger, bool store = true) : base(fieldName, logger, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (value is string str)
            {
                doc.Add(new FacetField(FieldName, str));
                    doc.Add(new StringField(
                        FieldName,
                        str,
                        Store ? Field.Store.YES : Field.Store.NO));
            }
            else if (value is string[] arrStr)
            {
                doc.Add(new FacetField(FieldName, arrStr));
                    doc.Add(new StringField(
                        FieldName,
                        string.Join(",", arrStr),
                        Store ? Field.Store.YES : Field.Store.NO));
            }
            throw new NotSupportedException("Value must be of type string[] or string");
        }
    }
}
