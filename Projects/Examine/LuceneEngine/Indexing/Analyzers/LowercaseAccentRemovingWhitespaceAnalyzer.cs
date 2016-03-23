using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine.Indexing.Analyzers
{
    public sealed class LowercaseAccentRemovingWhitespaceAnalyzer : Analyzer
    {
        
        public override TokenStream TokenStream(string fieldName, TextReader tr)
        {
            tr = new StringReader(RemoveAccents(tr.ReadToEnd()));
            return new LowerCaseFilter(new Tokenizer(tr));
        }

        private class Tokenizer : WhitespaceTokenizer
        {
            public Tokenizer(TextReader tr)
                : base(tr)
            {
            }

            protected override bool IsTokenChar(char p)
            {
                return char.IsLetterOrDigit(p);
            }
        }


        public static string RemoveAccents(string s)
        {
            bool hasAny = false;
            var sb = new StringBuilder();
            foreach (char c in s.Normalize(NormalizationForm.FormD))

                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        hasAny = true;
                        //do nothing
                        break;
                    default:
                        sb.Append(c);
                        break;
                }

            return hasAny ? sb.ToString().Normalize(NormalizationForm.FormC) : s;
        }
    }
    
}
