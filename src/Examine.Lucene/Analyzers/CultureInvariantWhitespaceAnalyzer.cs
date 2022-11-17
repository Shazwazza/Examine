using System.IO;
using J2N;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Util;

namespace Examine.Lucene.Analyzers
{
    /// <summary>
    /// A whitespace analyzer that can be configured to be culture invariant
    /// </summary>
    /// <remarks>
    /// Includes a LetterOrDigitTokenizer which only includes letters or digits along with 
    /// <see cref="ASCIIFoldingFilter"/> and <see cref="LowerCaseFilter"/>
    /// </remarks>
    public sealed class CultureInvariantWhitespaceAnalyzer : Analyzer
    {
        private readonly bool _caseInsensitive;
        private readonly bool _ignoreLanguageAccents;

        /// <inheritdoc/>
        public CultureInvariantWhitespaceAnalyzer() : this(true, true)
        {
        }

        /// <inheritdoc/>
        public CultureInvariantWhitespaceAnalyzer(bool caseInsensitive, bool ignoreLanguageAccents)
        {
            _caseInsensitive = caseInsensitive;
            _ignoreLanguageAccents = ignoreLanguageAccents;
        }

        /// <inheritdoc/>
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer = new LetterOrDigitTokenizer(reader);

            TokenStream result = null;

            if (_caseInsensitive)
            {
                result = new LowerCaseFilter(LuceneInfo.CurrentVersion, tokenizer);
            }

            if (_ignoreLanguageAccents)
            {
                result = new ASCIIFoldingFilter(result ?? tokenizer);
            }

            return new TokenStreamComponents(tokenizer, result);
        }

        private sealed class LetterOrDigitTokenizer : CharTokenizer
        {
            public LetterOrDigitTokenizer(TextReader tr)
                : base(LuceneInfo.CurrentVersion, tr)
            {
            }

            protected override bool IsTokenChar(int c) => Character.IsLetter(c) || IsNumber(c);

            private bool IsNumber(int c)
            {
                var include = char.IsLetterOrDigit((char)c);
                return include;
            }
        }
    }

}
