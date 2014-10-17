using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine.Indexing.Analyzers
{
    public sealed class PrefixAnalyzer : Analyzer
    {
        private readonly Analyzer _inner;
        
        
        public PrefixAnalyzer(Analyzer inner)
        {
            _inner = inner;            
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new PrefixFilter(_inner.TokenStream(fieldName, reader));
        }
    }
}
