using System.IO;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine
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

        public CultureInvariantWhitespaceAnalyzer() : this(true, true)
        {
            
        }

        public CultureInvariantWhitespaceAnalyzer(bool caseInsensitive, bool ignoreLanguageAccents)
        {
            _caseInsensitive = caseInsensitive;
            _ignoreLanguageAccents = ignoreLanguageAccents;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new LetterOrDigitTokenizer(reader);
            // TODO: This is a bit broken, if both _ignoreLanguageAccents and _caseInsensitve are true it's not working
            if (_ignoreLanguageAccents)
                result = new ASCIIFoldingFilter(result);
            if (_caseInsensitive)
                result = new LowerCaseFilter(result);            
            return result;
        }

        private class LetterOrDigitTokenizer : CharTokenizer
        {
            public LetterOrDigitTokenizer(TextReader tr)
                : base(tr)
            {
            }

            protected override bool IsTokenChar(char p)
            {
                var include = char.IsLetterOrDigit(p);
                return include;
            }
        }

    }

}
