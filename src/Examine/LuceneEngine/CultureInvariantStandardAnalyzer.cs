using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// The same as the <see cref="StandardAnalyzer"/> but with an additional <see cref="ASCIIFoldingFilter"/>
    /// </summary>
    public sealed class CultureInvariantStandardAnalyzer : StandardAnalyzer
    {
        private readonly Version _matchVersion;
        private readonly ISet<string> _stopWords;
        private readonly bool _enableStopPositionIncrements;

        public CultureInvariantStandardAnalyzer() : this(Version.LUCENE_30)
        {
            
        }

        public CultureInvariantStandardAnalyzer(Version matchVersion, ISet<string> stopWords) : base(matchVersion, stopWords)
        {
            _matchVersion = matchVersion;
            _stopWords = stopWords;
            _enableStopPositionIncrements = StopFilter.GetEnablePositionIncrementsVersionDefault(matchVersion);
        }

        public CultureInvariantStandardAnalyzer(Version matchVersion) : this(matchVersion, StandardAnalyzer.STOP_WORDS_SET)
        {
        }

        public CultureInvariantStandardAnalyzer(Version matchVersion, TextReader stopwords) : base(matchVersion, stopwords)
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new StopFilter(_enableStopPositionIncrements,
                new ASCIIFoldingFilter(
                    new LowerCaseFilter(
                        new StandardFilter(
                            new StandardTokenizer(_matchVersion, reader)
                            {
                                MaxTokenLength = MaxTokenLength
                            }))), _stopWords);
        }

        public override TokenStream ReusableTokenStream(string fieldName, TextReader reader)
        {
            var savedStreams = (SavedStreams)PreviousTokenStream;
            if (savedStreams == null)
            {
                savedStreams = new SavedStreams();
                this.PreviousTokenStream = savedStreams;
                savedStreams.TokenStream = new StandardTokenizer(_matchVersion, reader);
                savedStreams.FilteredTokenStream = new StandardFilter(savedStreams.TokenStream);
                savedStreams.FilteredTokenStream = new LowerCaseFilter(savedStreams.FilteredTokenStream);
                savedStreams.FilteredTokenStream = new ASCIIFoldingFilter(savedStreams.FilteredTokenStream);
                savedStreams.FilteredTokenStream = new StopFilter(_enableStopPositionIncrements, savedStreams.FilteredTokenStream, _stopWords);
            }
            else
                savedStreams.TokenStream.Reset(reader);
            savedStreams.TokenStream.MaxTokenLength = MaxTokenLength;
            return savedStreams.FilteredTokenStream;
        }

        private sealed class SavedStreams
        {
            internal StandardTokenizer TokenStream;
            internal TokenStream FilteredTokenStream;
        }
    }
}