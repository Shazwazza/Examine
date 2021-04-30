using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// The same as the <see cref="StandardAnalyzer"/> but with an additional <see cref="ASCIIFoldingFilter"/>
    /// </summary>
    public sealed class CultureInvariantStandardAnalyzer : Analyzer
    {
        private readonly Lucene.Net.Util.LuceneVersion _matchVersion;
        public readonly CharArraySet STOP_WORDS_SET = StopAnalyzer.ENGLISH_STOP_WORDS_SET;
        private int maxTokenLength = (int) byte.MaxValue;

        public CultureInvariantStandardAnalyzer() : this(LuceneInfo.CurrentVersion)
        {
            
        }

        public CultureInvariantStandardAnalyzer(Lucene.Net.Util.LuceneVersion matchVersion,CharArraySet stopWords)
        {
            _matchVersion = matchVersion;
            STOP_WORDS_SET = stopWords;
        }

        public CultureInvariantStandardAnalyzer(Lucene.Net.Util.LuceneVersion matchVersion) : this(matchVersion, StandardAnalyzer.STOP_WORDS_SET)
        {
        }

        public CultureInvariantStandardAnalyzer(Lucene.Net.Util.LuceneVersion matchVersion, TextReader stopwords) 
        {
        }

      
        
        protected override TokenStreamComponents CreateComponents(
            string fieldName,
            TextReader reader)
        {
            StandardTokenizer src = new StandardTokenizer(LuceneInfo.CurrentVersion, reader);
            src.MaxTokenLength = this.maxTokenLength;
            TokenStream tok = (TokenStream) new StopFilter(LuceneInfo.CurrentVersion,
                (TokenStream) new LowerCaseFilter(LuceneInfo.CurrentVersion, (TokenStream) new StandardFilter(LuceneInfo.CurrentVersion, (TokenStream) src)), STOP_WORDS_SET);
            return (TokenStreamComponents) new CultureInvariantStandardAnalyzer.TokenStreamComponentsAnonymousInnerClassHelper(this, src, tok, reader);
        }
        public int MaxTokenLength
        {
            set
            {
                this.maxTokenLength = value;
            }
            get
            {
                return this.maxTokenLength;
            }
        }
        private class TokenStreamComponentsAnonymousInnerClassHelper : TokenStreamComponents
        {
            private readonly CultureInvariantStandardAnalyzer outerInstance;
            private TextReader reader;
            private readonly StandardTokenizer src;

            public TokenStreamComponentsAnonymousInnerClassHelper(
                CultureInvariantStandardAnalyzer outerInstance,
                StandardTokenizer src,
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
                this.src.MaxTokenLength = this.outerInstance.maxTokenLength;
                base.SetReader(reader);
            }
        }
    }
}