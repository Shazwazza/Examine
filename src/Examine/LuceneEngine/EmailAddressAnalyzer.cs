using System.IO;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Used for email addresses
    /// </summary>
    public class EmailAddressAnalyzer : Analyzer
    {

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new LowerCaseFilter(                 //case insensitive
                 new EmailAddressTokenizer(reader));    //email tokenizer
        }

        /// <summary>
        /// Used for email addresses
        /// </summary>
        public class EmailAddressTokenizer : CharTokenizer
        {
            public EmailAddressTokenizer(TextReader input)
                : base(input)
            {
            }

            protected override bool IsTokenChar(char c)
            {
                // Make whitespace characters and the @ symbol be indicators of new words.
                return !(char.IsWhiteSpace(c) || c == '@');
            }
        }
    }

}
