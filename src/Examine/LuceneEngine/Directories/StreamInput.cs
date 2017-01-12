using System;
using System.IO;
using System.Security;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Stream wrapper around IndexInput
    /// </summary>
    [SecuritySafeCritical]
    public class StreamInput : Stream
    {
        public IndexInput Input
        {
            [SecuritySafeCritical]
            get;
            [SecuritySafeCritical]
            set;
        }

        [SecuritySafeCritical]
        public StreamInput(IndexInput input)
        {
            Input = input;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; ; } }
        public override bool CanWrite { get { return false; } }
        public override void Flush() { }

        public override long Length
        {
            [SecuritySafeCritical]
            get { return Input.Length(); }
        }

        public override long Position
        {
            [SecuritySafeCritical]
            get { return Input.GetFilePointer(); }
            [SecuritySafeCritical]
            set { Input.Seek(value); }
        }

        [SecuritySafeCritical]
        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Input.GetFilePointer();
            try
            {
                var len = Input.Length();
                if (count > (len - pos))
                    count = (int)(len - pos);
                Input.ReadBytes(buffer, offset, count);
            }
            catch (Exception) { }
            return (int)(Input.GetFilePointer() - pos);
        }

        [SecuritySafeCritical]
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Input.Seek(offset);
                    break;
                case SeekOrigin.Current:
                    Input.Seek(Input.GetFilePointer() + offset);
                    break;
                case SeekOrigin.End:
                    throw new System.NotImplementedException();
            }
            return Input.GetFilePointer();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        [SecuritySafeCritical]
        public override void Close()
        {
            Input.Close();
            base.Close();
        }
    }
}