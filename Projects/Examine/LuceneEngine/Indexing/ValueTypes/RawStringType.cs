using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing.ValueTypes
{
    public class RawStringType : IndexValueTypeBase
    {
        public RawStringType(string fieldName, bool store = true)
            : base(fieldName, store)
        {

        }

        protected override void AddSingleValue(Document doc, object value)
        {
            doc.Add(new Field(FieldName, "" + value, Store ? Field.Store.YES : Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
        }
    }

}
