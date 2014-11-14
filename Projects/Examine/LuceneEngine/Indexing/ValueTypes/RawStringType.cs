using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing.ValueTypes
{

    /// <summary>
    /// Indexes a raw string value - not analyzed, no normas
    /// </summary>
    public class RawStringType : IndexValueTypeBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="store"></param>
        public RawStringType(string fieldName, bool store = true)
            : base(fieldName, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            var f = value as IFieldable;
            if (f != null)
            {                
                doc.Add(f);
            }
            else
            {
                var ts = value as TokenStream;
                if (ts != null)
                {
                    doc.Add(new Field(FieldName, ts));
                }
                else
                {                    
                    doc.Add(new Field(FieldName, "" + value, 
                        Store ? Field.Store.YES : Field.Store.NO, 
                        Field.Index.NOT_ANALYZED, Field.TermVector.NO));                    
                }
            }
        }
    }

}
