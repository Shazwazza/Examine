using System;
using System.IO;
using System.Security;
using Lucene.Net.Store;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Stream wrapper around an IndexOutput
    /// </summary>
    
    public class StreamOutput : Stream
    {
        public IndexOutput Output
        {
            
            get;
            
            set;
        }

        
        public StreamOutput(IndexOutput output)
        {
            Output = output;
        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        
        public override void Flush()
        {
            Output.Flush();
        }

        public override long Length => Output.Length;

        public override long Position
        {   
            get => Output.GetFilePointer();
            set => Output.Seek(value);
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
                    throw new NotImplementedException();
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
            Output.Dispose();
            base.Close();
        }
    }
}