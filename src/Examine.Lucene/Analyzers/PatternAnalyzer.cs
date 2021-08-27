using System.IO;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Pattern;
using Lucene.Net.Analysis.Util;

namespace Examine.Lucene.Analyzers
{
    /// <summary>
    /// Analyzer that uses regex to parse out tokens
    /// </summary>
    public class PatternAnalyzer : Analyzer
    {
        private readonly int _regexGroup;
        private readonly bool _lowercase;
        private readonly CharArraySet _stopWords;
        private readonly Regex _pattern;

        /// <summary>
        /// Creates a new <see cref="PatternAnalyzer"/>
        /// </summary>
        /// <param name="format">The regex pattern</param>
        /// <param name="regexGroup">The regex group number to match. -1 to use as a split.</param>
        /// <param name="lowercase">Whether to lower case the tokens</param>
        /// <param name="stopWords">Any stop words that should be included</param>
        public PatternAnalyzer(string format, int regexGroup, bool lowercase = false, CharArraySet stopWords = null)
        {
            _regexGroup = regexGroup;
            _lowercase = lowercase;
            _stopWords = stopWords;
            _pattern = new Regex(format);
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer = new PatternTokenizer(reader, _pattern, _regexGroup);
            TokenStream stream = tokenizer;

            if (_lowercase)
            {
                stream = new LowerCaseFilter(LuceneInfo.CurrentVersion, stream);
            }

            if (_stopWords != null)
            {
                stream = new StopFilter(LuceneInfo.CurrentVersion, stream, _stopWords);
            }

            return new TokenStreamComponents(tokenizer, stream);
        }
    }
}
