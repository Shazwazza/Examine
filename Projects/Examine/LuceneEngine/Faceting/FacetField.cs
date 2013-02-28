using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// A facet value and level that can be added to a Lucene document for indexing
    /// </summary>
    public class FacetField : AbstractField
    {
        public string Value { get; private set; }
        public float Level { get; private set; }

        public FacetField(string name, string value, float level = .5f, bool store = false)            
            : base(name, store ? Field.Store.YES : Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO)
        {
            Value = value;
            Level = level;
            fieldsData = value;
            tokenStream = new PayloadDataTokenStream(Value).SetValue(Level);            
        }

        public override byte[] BinaryValue()
        {
            return null;
        }

        public override TokenStream TokenStreamValue()
        {
            return tokenStream;
        }

        public override TextReader ReaderValue()
        {
            return null;
        }

        public override string StringValue()
        {
            if (fieldsData != null)
                return fieldsData.ToString();

            return null;
        }
    }
}
