using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Custom IndexOutput that does nothing which is used for readonly instances
    /// </summary>
    public class NoopIndexOutput : IndexOutput
    {
        protected override void Dispose(bool disposing)
        {
        }

        public override void Flush()
        {
        }

        public override long Length => 0;

        public override long Checksum => 0;

        public override void Seek(long pos)
        {
        }

        public override void WriteByte(byte b)
        {
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
        }

        public override long GetFilePointer() => throw new System.NotImplementedException();
    }
}
