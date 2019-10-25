using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Support;
using Lucene.Net.Util;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Used for email addresses
    /// </summary>
    public class EmailAddressAnalyzer : Analyzer
    {

     

        /// <summary>
        /// Used for email addresses
        /// </summary>
        public class EmailAddressTokenizer : CharTokenizer
        {
            public EmailAddressTokenizer(TextReader input)
                : base(LuceneVersion.LUCENE_48,input)
            {
            }
            

            protected override bool IsTokenChar(int c)
            {
                return   Character.IsLetter(c);
            }
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            EmailAddressTokenizer src = new EmailAddressTokenizer( reader);
            TokenStream tok = (TokenStream)   new LowerCaseFilter(LuceneVersion.LUCENE_48,                 //case insensitive
                src);
          
            return new TokenStreamComponentsAnonymousInnerClassHelper(this, src, tok, reader);
        }
        private class TokenStreamComponentsAnonymousInnerClassHelper : TokenStreamComponents
        {
            private readonly EmailAddressAnalyzer outerInstance;
            private TextReader reader;
            private readonly EmailAddressTokenizer src;

            public TokenStreamComponentsAnonymousInnerClassHelper(
                EmailAddressAnalyzer outerInstance,
                EmailAddressTokenizer src,
                TokenStream tok,
                TextReader reader)
                : base((Tokenizer) src, tok)
            {
                this.outerInstance = outerInstance;
                this.reader = reader;
                this.src = src;
            }

            protected override void SetReader(TextReader reader)
            {
                base.SetReader(reader);
            }
        }
    }

}
