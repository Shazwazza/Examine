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
    /// Stream wrapper around IndexInput
    /// </summary>
    public class StreamInput : Stream
    {
        public IndexInput Input { get; set; }

        public StreamInput(IndexInput input)
        {
            Input = input;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true;; } }
        public override bool CanWrite { get { return false; } }
        public override void Flush() { }
        public override long Length { get { return Input.Length(); } }

        public override long Position
        {
            get { return Input.GetFilePointer(); }
            set { Input.Seek(value); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long pos = Input.GetFilePointer();
            try
            {
                long len = Input.Length();
                if (count > (len - pos))
                    count = (int)(len - pos);
                Input.ReadBytes(buffer, offset, count);
            }
            catch (Exception) { }
            return (int)(Input.GetFilePointer() - pos);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    Input.Seek(offset); 
                    break;
                case SeekOrigin.Current:
                    Input.Seek(Input.GetFilePointer()+offset);
                    break;
                case SeekOrigin.End:
                    throw new System.NotImplementedException();
             }
            return Input.GetFilePointer();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            Input.Close();
            base.Close();
        }
    }
}
