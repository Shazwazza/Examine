using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Facet.SortedSet;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetFullTextType : FullTextType
    {
        public FacetFullTextType(string fieldName, ILoggerFactory logger, Analyzer analyzer = null, bool sortable = false) : base(fieldName, logger, analyzer, sortable)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (!TryConvert<string>(value, out var str))
            {
                return;
            }

            doc.Add(new SortedSetDocValuesFacetField(FieldName, str));
        }
    }
}
