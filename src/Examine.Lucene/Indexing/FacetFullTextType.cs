using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetFullTextType : FullTextType
    {
        private readonly bool _taxonomyIndex;

        public FacetFullTextType(string fieldName, ILoggerFactory logger, Analyzer analyzer = null, bool sortable = false, bool taxonomyIndex = false) : base(fieldName, logger, analyzer, sortable)
        {
            _taxonomyIndex = taxonomyIndex;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (_taxonomyIndex)
            {
                if (TryConvert<string>(value, out var str))
                {
                    doc.Add(new FacetField(FieldName, str));
                }
                else if (TryConvert<string[]>(value, out var strArr))
                {
                    doc.Add(new FacetField(FieldName, strArr));
                }
                return;
            }
            else
            {
                if (!TryConvert<string>(value, out var str))
                {
                    return;
                }
                doc.Add(new SortedSetDocValuesFacetField(FieldName, str));
            }
        }
    }
}
