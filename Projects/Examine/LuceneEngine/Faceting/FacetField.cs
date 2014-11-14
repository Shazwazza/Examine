using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// A facet value and level that can be added to a Lucene document for indexing
    /// </summary>
    public class FacetValue
    {
        private readonly SingleTokenTokenStream _stream;

        public string Value { get; private set; }
        public float Level { get; private set; }

        public TokenStream TokenStream
        {
            get { return _stream; }
        }

        public FacetValue(string value, float level = .5f)
        {
            _stream = TokenStreamHelper.Create(value, level);

            Value = value;
            Level = level;
        }      
    }
}
