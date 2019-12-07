using Examine.LuceneEngine.Directories;

namespace Examine.S3Directory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for user on the server that only reads from the index
    /// </summary>
    public class ReadOnlyS3DirectoryFactory : S3DirectoryFactory
    {
        public ReadOnlyS3DirectoryFactory() : base(isReadOnly:true)
        {   
        }
    }
}