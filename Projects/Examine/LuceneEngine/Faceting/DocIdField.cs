using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// An external ID of the object indexed in a Lucene document stored for fast retrieval by reading from payload data.
    /// http://invertedindex.blogspot.dk/2009/04/lucene-dociduid-mapping-and-payload.html
    /// </summary>
    public class ExternalIdField : AbstractField
    {
        public long ExternalId { get; private set; }
        public static readonly Term Term = new Term("__ExternalId", "ExternalId");

        public ExternalIdField(long externalId)
            : base(Term.Field(), Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO)
        {
            ExternalId = externalId;
            tokenStream = new PayloadDataTokenStream(Term.Text()).SetValue(externalId);

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
            return null;
        }
    }
}
