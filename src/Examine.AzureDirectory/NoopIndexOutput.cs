using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Custom IndexOutput that does nothing which is used for readonly instances
    /// </summary>
    public class NoopIndexOutput : IndexOutput
    {
        public override void Close()
        {
        }

        public override void Flush()
        {
        }

        public override long GetFilePointer() => 0;

        public override long Length() => 0;

        public override void Seek(long pos)
        {
        }

        public override void WriteByte(byte b)
        {
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
        }
    }
}
