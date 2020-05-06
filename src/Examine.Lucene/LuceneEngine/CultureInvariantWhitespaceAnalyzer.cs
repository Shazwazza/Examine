using System.Globalization;
using System.IO;
using J2N;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Support;
using Lucene.Net.Util;

namespace Examine.LuceneEngine
{

    /// <summary>
    /// A whitespace analyzer that can be configured to be culture invariant
    /// </summary>
    public sealed class CultureInvariantWhitespaceAnalyzer : Analyzer
    {
        private readonly bool _caseInsensitive;
        private readonly bool _ignoreLanguageAccents;

        public CultureInvariantWhitespaceAnalyzer() : this(true, true)
        {

        }

        public CultureInvariantWhitespaceAnalyzer(bool caseInsensitive, bool ignoreLanguageAccents)
        {
            _caseInsensitive = caseInsensitive;
            _ignoreLanguageAccents = ignoreLanguageAccents;
        }



        private sealed class LetterOrDigitTokenizer : CharTokenizer
        {
            public LetterOrDigitTokenizer(TextReader tr)
                : base(Util.Version, tr)
            {
            }


            protected override bool IsTokenChar(int c)
            {
                return Character.IsLetter(c) || IsNumber(c);
            }

            private bool IsNumber(int c)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory((char) c);
                switch (unicodeCategory)
                {
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.OtherNumber:
                    case UnicodeCategory.LetterNumber:
                        return true;
                    default:
                        return false;
                }
            }
        }
            protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
            {
                LetterOrDigitTokenizer src = new LetterOrDigitTokenizer(reader);

                TokenStream tok = (TokenStream) new LowerCaseFilter(Util.Version, //case insensitive
                    src);
                if (_ignoreLanguageAccents)
                    tok = new ASCIIFoldingFilter(tok);
                if (_caseInsensitive)
                    tok = new LowerCaseFilter(Util.Version, tok);
                return new TokenStreamComponentsAnonymousInnerClassHelper(this, src, tok, reader);
            }

            private class TokenStreamComponentsAnonymousInnerClassHelper : TokenStreamComponents
            {
                private readonly CultureInvariantWhitespaceAnalyzer outerInstance;
                private TextReader reader;
                private readonly LetterOrDigitTokenizer src;

                public TokenStreamComponentsAnonymousInnerClassHelper(
                    CultureInvariantWhitespaceAnalyzer outerInstance,
                    LetterOrDigitTokenizer src,
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
