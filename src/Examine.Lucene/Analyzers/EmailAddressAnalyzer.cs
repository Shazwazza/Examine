using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Util;

namespace Examine.Lucene.Analyzers
{
    /// <summary>
    /// Used for email addresses
    /// </summary>
    public class EmailAddressAnalyzer : Analyzer
    {
        /// <inheritdoc/>
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer = new EmailAddressTokenizer(reader);

            //case insensitive
            TokenStream result = new LowerCaseFilter(LuceneInfo.CurrentVersion, tokenizer);

            return new TokenStreamComponents(tokenizer, result);
        }

        /// <summary>
        /// Used for email addresses
        /// </summary>
        private sealed class EmailAddressTokenizer : CharTokenizer
        {
            public EmailAddressTokenizer(TextReader tr)
               : base(LuceneInfo.CurrentVersion, tr)
            {
            }

            protected override bool IsTokenChar(int c)
            {
                var asChar = (char)c;

                // Make whitespace characters and the @ symbol be indicators of new words.
                return !(char.IsWhiteSpace(asChar) || asChar == '@');
            }
        }
    }

}
