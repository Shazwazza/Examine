using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.Standard;

namespace Examine
{
    ///<summary>
    /// String extensions
    ///</summary>
    public static class StringExtensions
    {
        ///<summary>
        /// Removes stop words from the text if not contained within a phrase
        ///</summary>
        ///<param name="searchText"></param>
        ///<returns></returns>
        public static string RemoveStopWords(this string searchText)
        {
            Action<string, StringBuilder> removeWords = (str, b) =>
                    {
                        //remove stop words prior to search
                        var innerBuilder = new StringBuilder();
                        var searchParts = str.Split(' ');

                        foreach (var t in searchParts.Where(t => !StandardAnalyzer.STOP_WORDS_SET.Contains(t.ToLower())))
                        {
                            innerBuilder.Append(t);
                            innerBuilder.Append(" ");
                        }
                        b.Append(innerBuilder.ToString().TrimEnd(' '));
                    };

            var builder = new StringBuilder();
            var carrat = 0;
            while(carrat < searchText.Length)
            {
                var quoteIndex = searchText.IndexOf("\"", carrat);
                if (quoteIndex >= 0 && carrat == quoteIndex)
                {
                    //move to next quote
                    carrat = searchText.IndexOf("\"", quoteIndex + 1) + 1;
                    //add phrase to builder
                    builder.Append(searchText.Substring(quoteIndex, carrat - quoteIndex));
                    //builder.Append(' ');
                }
                else
                {
                    //move to next quote
                    var nextCarrat = searchText.IndexOf("\"", carrat);
                    if (nextCarrat < 0)
                    {
                        nextCarrat = searchText.Length;
                    }
                    var terms = searchText.Substring(carrat, nextCarrat - carrat);
                    removeWords(terms, builder);
                    if (terms.EndsWith(" ")) builder.Append(' ');
                    carrat = nextCarrat;
                }
            }

            return builder.ToString().TrimEnd(' ');
        }
    }
}