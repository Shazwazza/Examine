using System.Collections.Generic;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    public interface IFacetExtractor
    {
        IEnumerable<DocumentFacet> GetDocumentFacets(IndexReader reader, FacetConfiguration data);
    }

    public class TermFacetExtractor : IFacetExtractor
    {
        public string FieldName { get; private set; }

        public TermFacetExtractor(string fieldName)
        {
            FieldName = fieldName;
        }

        public IEnumerable<DocumentFacet> GetDocumentFacets(IndexReader reader, FacetConfiguration data)
        {
            var terms = reader.Terms(new Term(FieldName));
            var tp = reader.TermPositions();
            try
            {
                var dataBuffer = new byte[4];
                do
                {
                    var t = terms.Term();
                    if (t == null || t.Field() != FieldName)
                    {
                        break;
                    }
                    tp.Seek(terms);
                    while (tp.Next())
                    {
                        var docId = tp.Doc();
                        tp.NextPosition();
                        float level = 1f;
                        if (tp.IsPayloadAvailable())
                        {
                            tp.GetPayload(dataBuffer, 0);
                            level = PayloadDataTokenStream.GetFloatValue(dataBuffer);
                        }

                        foreach (var df in ExpandTerm(docId, FieldName, t.Text(), level))
                        {
                            yield return df;
                        }
                    }
                } while (terms.Next());
            }
            finally
            {
                terms.Close();
                tp.Close();
            }
        }

        protected virtual IEnumerable<DocumentFacet> ExpandTerm(int docId, string fieldName, string termValue, float level)
        {
            yield return new DocumentFacet
            {
                DocumentId = docId,
                Key = new FacetKey(fieldName, termValue),                
                Level = level
            };
        }
    }
}