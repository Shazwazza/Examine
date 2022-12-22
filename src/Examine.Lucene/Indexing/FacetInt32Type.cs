using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetInt32Type : Int32Type
    {
        private readonly bool _taxonomyIndex;

        public FacetInt32Type(string fieldName, ILoggerFactory logger, bool store = true,bool taxonomyIndex = false) : base(fieldName, logger, store)
        {
            _taxonomyIndex = taxonomyIndex;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (!TryConvert(value, out int parsedVal))
                return;
            if (_taxonomyIndex)
            {
                doc.Add(new FacetField(FieldName, parsedVal.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, parsedVal));
            }
            else
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, parsedVal.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, parsedVal));
            }
        }
    }
}
