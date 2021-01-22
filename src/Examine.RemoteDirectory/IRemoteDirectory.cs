using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public interface IRemoteDirectory
    {
        bool FileExists(string filename);
        void EnsureContainer(string containerName);
        void SyncFile(Lucene.Net.Store.Directory directory, string fileName, bool CompressBlobs);
        long FileLength(string filename, long lenghtFallback);
        IEnumerable<string> GetAllRemoteFileNames();
        void DeleteFile(string name);
        long FileModified(string name);
        bool Upload(IndexInput stream, string name, long originalLength, bool CompressBlobs, string lastModified = null);
        bool TryGetBlobFile(string name);
        bool Upload(MemoryStream stream, string fileName);
        Tuple<long, DateTime> GetFileProperties(string filename);
    }
}