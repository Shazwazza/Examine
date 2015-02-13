using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace Examine.LuceneEngine.Indexing.Analyzers
{
    public sealed class PrefixFilter : TokenFilter
    {
        private readonly ITermAttribute _term;
        private readonly IOffsetAttribute _offset;
        private readonly IPositionIncrementAttribute _position;

        public PrefixFilter(TokenStream input)
            : base(input)
        {
            _term = AddAttribute<ITermAttribute>();
            _offset = AddAttribute<IOffsetAttribute>();
            _position = AddAttribute<IPositionIncrementAttribute>();

        }

        private string[] _termBuffer;
        private int _termBufferIndex = 0;
        private int _yieldLength;
        private int _start;
        public override bool IncrementToken()
        {

            while (true)
            {
                if (_termBuffer == null)
                {
                    if (!input.IncrementToken())
                    {
                        return false;
                    }

                    //_termBuffer =  (char[]) _term.TermBuffer().Clone();
                    var term = new string(_term.TermBuffer(), 0, _term.TermLength());

                    _termBuffer = new[] { term };
                    _termBufferIndex = 0;

                    //_termLength = _term.TermLength();
                    _yieldLength = 0;
                    _start = _offset.StartOffset;
                }

                var t = _termBuffer[_termBufferIndex];

                if (_yieldLength++ <= t.Length)
                {
                    var spelling = _yieldLength == t.Length + 1
                        ? t : t.Substring(0, _yieldLength) + "*";

                    _term.SetTermBuffer(spelling);
                    _offset.SetOffset(_start, _start + _yieldLength);
                    _position.PositionIncrement = _termBufferIndex == 0 && _yieldLength == 1 ? 1 : 0;
                    return true;
                }

                if (++_termBufferIndex >= _termBuffer.Length)
                {
                    _termBuffer = null;
                }
                else
                {
                    var nextTerm = _termBuffer[_termBufferIndex];
                    _yieldLength = 0;
                    //Find first position where the terms differ
                    for (int i = 0, n = Math.Min(t.Length, nextTerm.Length); i < n; i++)
                    {
                        if (t[i] != nextTerm[i]) break;
                        ++_yieldLength;
                    }
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            _termBuffer = null;
        }
    }
}
