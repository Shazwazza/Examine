using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace Examine.Lucene.Analyzers
{
    /// <summary>
    /// The same as the <see cref="StandardAnalyzer"/> but with an additional <see cref="ASCIIFoldingFilter"/>
    /// </summary>
    public sealed class CultureInvariantStandardAnalyzer : Analyzer
    {
        private readonly CharArraySet _stopWordsSet;
        private readonly bool _caseInsensitive;
        private readonly bool _ignoreLanguageAccents;

        /// <inheritdoc/>
        public CultureInvariantStandardAnalyzer(CharArraySet stopWords)
            : this(stopWords, true, true)
        {
            
        }

        /// <inheritdoc/>
        public CultureInvariantStandardAnalyzer()
            : this(StandardAnalyzer.STOP_WORDS_SET)
        {
        }

        /// <inheritdoc/>
        public CultureInvariantStandardAnalyzer(CharArraySet stopWords, bool caseInsensitive, bool ignoreLanguageAccents)
        {
            _stopWordsSet = stopWords;
            _caseInsensitive = caseInsensitive;
            _ignoreLanguageAccents = ignoreLanguageAccents;
        }

        /// <inheritdoc/>
        protected override TokenStreamComponents CreateComponents(
            string fieldName,
            TextReader reader)
        {
            var tokenizer = new StandardTokenizer(LuceneInfo.CurrentVersion, reader)
            {
                MaxTokenLength = MaxTokenLength
            };

            TokenStream result = new StandardFilter(LuceneInfo.CurrentVersion, tokenizer);

            if (_caseInsensitive)
            {
                result = new LowerCaseFilter(LuceneInfo.CurrentVersion, result);
            }

            if (_ignoreLanguageAccents)
            {
                result = new ASCIIFoldingFilter(result ?? tokenizer);
            }

            result = new StopFilter(LuceneInfo.CurrentVersion, result, _stopWordsSet);

            return new TokenStreamComponents(tokenizer, result);
        }

        /// <summary>
        /// Set the max allowed token length. Any token longer than this is skipped
        /// </summary>
        public int MaxTokenLength { set; get; } = byte.MaxValue;

    }
}
