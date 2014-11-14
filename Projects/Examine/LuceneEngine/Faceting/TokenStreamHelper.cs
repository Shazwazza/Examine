using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    public static class TokenStreamHelper
    {

        public static SingleTokenTokenStream Create(string value)
        {
            return new SingleTokenTokenStream(new Token(value, 0, 0));
        }
        public static SingleTokenTokenStream Create(string value, long num)
        {
            return new SingleTokenTokenStream(new Token(value, 0, 0)
            {
                Payload = new Payload(BitConverter.GetBytes(num))
            });
        }
        public static SingleTokenTokenStream Create(string value, float num)
        {
            return new SingleTokenTokenStream(new Token(value, 0, 0)
            {
                Payload = new Payload(BitConverter.GetBytes(num))
            });
        }

        public static float GetFloatValue(byte[] dataBuffer)
        {
            return BitConverter.ToSingle(dataBuffer, 0);
        }

        public static long GetLongValue(byte[] dataBuffer)
        {
            return BitConverter.ToInt64(dataBuffer, 0);
        }

      
    }
}
