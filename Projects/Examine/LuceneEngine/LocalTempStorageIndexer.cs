using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = System.IO.Directory;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Utility to copy/backup/restore an index (WIP, it works but need to think about locking, etc.. and messaging)
    /// </summary>
    internal class LuceneIndexCopy
    {
        private readonly SnapshotDeletionPolicy _snapshotter;

        [SecuritySafeCritical]
        public LuceneIndexCopy()
        {
            IndexDeletionPolicy policy = new KeepOnlyLastCommitDeletionPolicy();
            _snapshotter = new SnapshotDeletionPolicy(policy);
        }

        [SecuritySafeCritical]
        public bool Copy(SimpleFSDirectory sourceLuceneDirectory, Analyzer analyzer, DirectoryInfo targetPath)
        {
            if (targetPath.Exists == false)
            {
                Directory.CreateDirectory(targetPath.FullName);
            }

            //copy index if it exists, don't do anything if it's not there
            if (IndexReader.IndexExists(sourceLuceneDirectory) == false) return true;

            using (new IndexWriter(
                //read from the underlying/default directory, not the temp codegen dir
                sourceLuceneDirectory,
                analyzer,
                _snapshotter,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                try
                {
                    //var basePath = IOHelper.MapPath(configuredPath);

                    var allSnapshotFiles = new HashSet<string>();
                    var commit = _snapshotter.Snapshot();
                    foreach (var fileName in commit.GetFileNames())
                    {
                        allSnapshotFiles.Add(fileName);
                    }
                    allSnapshotFiles.Add(commit.GetSegmentsFileName());
                    //we need to manually include the segments.gen file
                    allSnapshotFiles.Add("segments.gen");

                    //Get all files in the temp storage that don't exist in the snapshot collection, we want to remove these
                    var toRemove = targetPath.GetFiles()
                        .Select(x => x.Name)
                        .Except(allSnapshotFiles);

                    //using (var tempDirectory = new SimpleFSDirectory(tempDir))
                    //{
                    //TODO: We're ignoring if it is locked right now, it shouldn't be unless for some strange reason the 
                    // last process hasn't fully shut down, in that case we're not going to worry about it.

                    //if (IndexWriter.IsLocked(tempDirectory) == false)
                    //{
                    foreach (var file in toRemove)
                    {
                        try
                        {
                            File.Delete(Path.Combine(targetPath.FullName, file));
                        }
                        catch (IOException ex)
                        {
                            //TODO: we're ignoring this, as old files shouldn't affect the index/search operations, lucene files are 'write once'
                            //TODO: Return some info?

                            //quit here
                            //return false;
                        }
                    }
                    //}
                    //else
                    //{
                    //    LogHelper.Warn<LocalTempStorageIndexer>("Cannot sync index files from main storage, the index is currently locked");
                    //    //quit here
                    //    return false;
                    //}

                    foreach (var fileName in allSnapshotFiles.Where(f => string.IsNullOrWhiteSpace(f) == false))
                    {
                        var destination = Path.Combine(targetPath.FullName, Path.GetFileName(fileName));

                        //don't copy if it's already there, lucene is 'write once' so this file is meant to be there already
                        if (File.Exists(destination)) continue;

                        try
                        {
                            File.Copy(
                                Path.Combine(sourceLuceneDirectory.GetDirectory().FullName, "Index", fileName),
                                destination);
                        }
                        catch (IOException ex)
                        {
                            //TODO: Return some info?
                            //quit here
                            return false;
                        }
                    }

                    //}



                }
                finally
                {
                    _snapshotter.Release();
                }


            }

            return true;

        }
    }
}
