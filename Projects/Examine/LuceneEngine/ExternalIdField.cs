using System.IO;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Examine.LuceneEngine
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
            : base(Term.Field, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO)
        {
            ExternalId = externalId;

            tokenStream = TokenStreamHelper.Create(Term.Text, externalId);

        }

        /// <summary>
        /// The TokenStream for this field to be used when indexing, or null.
        /// </summary>
        /// <seealso cref="P:Lucene.Net.Documents.IFieldable.StringValue"/>
        public override TokenStream TokenStreamValue
        {
            get { return tokenStream; }
        }

        /// <summary>
        /// The value of the field as a Reader, which can be used at index time to generate indexed tokens.
        /// </summary>
        /// <seealso cref="P:Lucene.Net.Documents.IFieldable.StringValue"/>
        public override TextReader ReaderValue
        {
            get { return null; }
        }

        /// <summary>
        /// The value of the field as a String, or null.
        ///             <p/>
        ///             For indexing, if isStored()==true, the stringValue() will be used as the stored field value
        ///             unless isBinary()==true, in which case GetBinaryValue() will be used.
        ///             If isIndexed()==true and isTokenized()==false, this String value will be indexed as a single token.
        ///             If isIndexed()==true and isTokenized()==true, then tokenStreamValue() will be used to generate indexed tokens if not null,
        ///             else readerValue() will be used to generate indexed tokens if not null, else stringValue() will be used to generate tokens.
        /// </summary>
        public override string StringValue
        {
            get { return null; }
        }
    }
}
