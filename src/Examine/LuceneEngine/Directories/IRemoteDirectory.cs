using System.Collections.Generic;

namespace Examine.RemoteDirectory
{
    public interface IRemoteDirectory
    {
        bool FileExists(string filename);
        void EnsureContainer(string filename);
        void SyncFile(Lucene.Net.Store.Directory directory, string fileName, bool CompressBlobs);
        long FileLength(string filename, long lenghtFallback);
        IEnumerable<string> GetAllRemoteFileNames();
        void DeleteFile(string name);
    }
}