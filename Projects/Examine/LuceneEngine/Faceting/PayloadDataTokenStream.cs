using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    public class PayloadDataTokenStream : TokenStream
    {
        private readonly Token _token;
        private bool _returnToken = false;


        public PayloadDataTokenStream(string value)
        {
            _token = new Token(value, 0, 0);
        }

        public PayloadDataTokenStream SetValue(long sid)
        {
            _token.SetPayload(new Payload(BitConverter.GetBytes(sid)));
            _returnToken = true;

            return this;
        }

        public PayloadDataTokenStream SetValue(float value)
        {
            _token.SetPayload(new Payload(BitConverter.GetBytes(value)));
            _returnToken = true;

            return this;
        }

        public static float GetFloatValue(byte[] dataBuffer)
        {
            return BitConverter.ToSingle(dataBuffer, 0);
        }

        public static long GetLongValue(byte[] dataBuffer)
        {
            return BitConverter.ToInt64(dataBuffer, 0);
        }

        public override Token Next()
        {
            if (_returnToken)
            {
                _returnToken = false;
                return _token;
            }

            return null;
        }
    }
}
