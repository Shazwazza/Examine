using System;
using System.Collections.Generic;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    public class TermFacetExtractor : IFacetExtractor
    {
        public string FieldName { get; private set; }

        public bool ValuesAreReferences { get; private set; }

        public TermFacetExtractor(string fieldName, bool valuesAreReferences = false)
        {
            FieldName = fieldName;
            ValuesAreReferences = valuesAreReferences;
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
                        float level = .5f;
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
            if (ValuesAreReferences)
            {
                long longId;
                if (!long.TryParse(termValue, out longId))
                {
                    throw new InvalidOperationException("Cannot use term value as a reference, the value must be convertable to a long data type");
                }

                yield return new DocumentFacet(docId, true,
                    new FacetReferenceKey(fieldName, long.Parse(termValue)),
                    level);
            }
            
            yield return new DocumentFacet(docId, true,
                new FacetKey(fieldName, termValue),
                level);
        }
    }
}