using System;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;
using Lucene.Net.Analysis.TokenAttributes;

namespace Examine.Test.Index
{
    [TestFixture]
    [Ignore("This is just here to confirm that Standard Analyzer no longer strips apostrophe's")]
    public class AnalyzerTests
    {
        [Test]
        public void Underscores()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            var ts = analyzer.GetTokenStream("myField", "This is Warren's book");
            ts.Reset();
            while (ts.IncrementToken())
            {
                var termAtt = ts.GetAttribute<ICharTermAttribute>();
                Console.WriteLine(termAtt);
            }
        }
    }
}
