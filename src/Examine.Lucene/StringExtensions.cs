using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Lucene.Net.Analysis.Standard;

namespace Examine
{
    ///<summary>
    /// String extensions
    ///</summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Generates a hash of a string based on the FIPS compliance setting.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GenerateHash(this string str)
        {
            return CryptoConfig.AllowOnlyFipsAlgorithms
                ? str.GenerateSha1Hash()
                : str.GenerateMd5();
        }

        /// <summary>
        /// Generate a SHA1 hash of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GenerateSha1Hash(this string str) => str.GenerateHash("SHA1");

        /// <summary>Generate a MD5 hash of a string
        /// </summary>
        public static string GenerateMd5(this string str) => str.GenerateHash("MD5");

        /// <summary>Generate a MD5 hash of a string
        /// </summary>
        private static string GenerateHash(this string str, string hashType)
        {
            var hasher = HashAlgorithm.Create(hashType) ?? throw new InvalidOperationException("No hashing type found by name " + hashType);

            using (hasher)
            {
                //convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                //get the hashed values created by our SHA1CryptoServiceProvider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                //create a StringBuilder object
                var stringBuilder = new StringBuilder();

                //loop to each each byte
                foreach (var b in hashedByteArray)
                {
                    //append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2").ToLower());
                }

                //return the hashed value
                return stringBuilder.ToString();
            }
        }

        internal static string EnsureEndsWith(this string input, char value)
        {
            if (input.EndsWith(value.ToString(CultureInfo.InvariantCulture)))
            {
                return input;
            }
            else
            {
                return input + value;
            }
        }

        internal static string ReplaceNonAlphanumericChars(this string input, string replacement)
        {
            //any character that is not alphanumeric, convert to a hyphen
            var mName = input;
            foreach (var c in mName.ToCharArray().Where(c => !char.IsLetterOrDigit(c)))
            {
                mName = mName.Replace(c.ToString(CultureInfo.InvariantCulture), replacement);
            }
            return mName;
        }

		//NOTE: The reason this code is in a separate method is because the Code Analysis barks at us with security concerns for medium trust
		// when it is inline in the RemoveStopWords method like it used to be.
		
		private static bool IsStandardAnalyzerStopWord(string stringToCheck)
		{
			if (StandardAnalyzer.STOP_WORDS_SET.Contains(stringToCheck.ToLower()))
			{
				return true;
			}
			return false;
		}

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

	                    foreach (var t in searchParts)
	                    {
							if (!IsStandardAnalyzerStopWord(t))
		                    {
			                    innerBuilder.Append(t);
                                innerBuilder.Append(" ");
		                    }
	                    }
	                    b.Append(innerBuilder.ToString());
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

                    if (carrat > 0)
                    {
                        //add phrase to builder
                        var phraseWithoutQuotes = searchText.Substring(quoteIndex + 1, carrat - quoteIndex - 2);
                        builder.Append("\"" + phraseWithoutQuotes.Trim() + "\" ");
                    }
                    else
                    {
                        //there are not more quotes
                        carrat = quoteIndex + 1;
                    }
                }
                else
                {
                    //move to next quote
                    var nextCarrat = searchText.IndexOf("\"", carrat);
                    if (nextCarrat < 0)
                    {
                        nextCarrat = searchText.Length;
                    }
#pragma warning disable IDE0057 // Use range operator
                    var terms = searchText.Substring(carrat, nextCarrat - carrat).Trim();
#pragma warning restore IDE0057 // Use range operator
                    if (!string.IsNullOrWhiteSpace(terms))
                    {
                        removeWords(terms, builder);    
                    }
                    carrat = nextCarrat;
                }
            }

            return builder.ToString().TrimEnd(' ');
        }
    }
}
