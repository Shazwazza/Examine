using System;
using System.IO;
using System.Security;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Stream wrapper around IndexInput
    /// </summary>
    
    public class StreamInput : Stream
    {
        public IndexInput Input
        {
            
            get;
            
            set;
        }

        
        public StreamInput(IndexInput input)
        {
            Input = input;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override void Flush() { }

        public override long Length
        {
            
            get { return Input.Length(); }
        }

        public override long Position
        {
            
            get { return Input.FilePointer; }
            
            set { Input.Seek(value); }
        }

        
        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Input.FilePointer;
            //try
            //{
                var len = Input.Length();
                if (count > (len - pos))
                    count = (int)(len - pos);
                Input.ReadBytes(buffer, offset, count);
            //}
            //catch (Exception) { }
            return (int)(Input.FilePointer - pos);
        }

        
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Input.Seek(offset);
                    break;
                case SeekOrigin.Current:
                    Input.Seek(Input.FilePointer + offset);
                    break;
                case SeekOrigin.End:
                    throw new System.NotImplementedException();
            }
            return Input.FilePointer;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        
        public override void Close()
        {
            Input.Close();
            base.Close();
        }
    }
}