//    License: Microsoft Public License (Ms-PL) 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lucene.Net.Store;

namespace Lucene.Net.Store.Azure
{
    /// <summary>
    /// Stream wrapper around an IndexOutput
    /// </summary>
    public class StreamOutput : Stream
    {
        public IndexOutput Output { get;set;}

        public StreamOutput(IndexOutput output)
        {
            Output = output;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            Output.Flush();
        }

        public override long Length
        {
            get { return Output.Length(); }
        }

        public override long Position
        {
            get
            {
                return Output.GetFilePointer();
            }
            set
            {
                Output.Seek(value);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Output.Seek(offset);
                    break;
                case SeekOrigin.Current:
                    Output.Seek(Output.GetFilePointer() + offset);
                    break;
                case SeekOrigin.End:
                    throw new System.NotImplementedException();
            }
            return Output.GetFilePointer();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Output.WriteBytes(buffer, offset, count);
        }

        public override void Close()
        {
            Output.Flush();
            Output.Close();
            base.Close();
        }
    }
}
