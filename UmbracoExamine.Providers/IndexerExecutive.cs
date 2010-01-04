using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Providers.Config;
using System.IO;

namespace UmbracoExamine.Providers
{

    /// <summary>
    /// Manages the delegation of authority over which machine in a load balanced environment will perform the indexing.
    /// This is done by an IO race on initialization of the LuceneExamineIndexer.
    /// If a server's app pool is recycled at a seperate time than the rest of the servers in the cluster, it will generally
    /// take over the executive role (this is dependant on the time that the last latest server's app pool was restarted).
    /// The Executive is determined by file lock (.lck) file, theoretically there should only be one of these.
    /// If there is only 1 server in the cluster, then obviously it is the Executive.
    /// </summary>
    public sealed class IndexerExecutive
    {

        public IndexerExecutive(DirectoryInfo d)
        {
            ExamineDirectory = d;
            ExaFile = new FileInfo(Path.Combine(ExamineDirectory.FullName, Environment.MachineName + ".exa"));
            LckFile = new FileInfo(Path.Combine(ExamineDirectory.FullName, Environment.MachineName + ".lck"));
        }

        public DirectoryInfo ExamineDirectory { get; private set; }

        private FileInfo ExaFile;
        private FileInfo LckFile;

        public void Initialize()
        {
            CreateAnnounceFile();
            //only clear lock files older than 1 hour as there may already be an active Executive
            ClearOldLockFiles(DateTime.Now.AddHours(-1));
            RaceForMasterIndexer();
        }

        /// <summary>
        /// Ensures there is an elected Executive, otherwise starts the race. 
        /// Returns a bool as to whether or not this is the Executive machine.
        /// </summary>
        public bool IsExecutiveMachine
        {
            get
            {
                int count = LockFileCount();
                if (count != 1)
                {
                    //since we know there's no Executive, clear all lock files and start the race.
                    ClearOldLockFiles(DateTime.Now);
                    RaceForMasterIndexer();                
                }

                //if the lck file exists with this machine name, then it is executive.
                return LckFile.Exists;
            }
        }

        /// <summary>
        /// Creates an xml file to declare that this machine is taking part in the index writing.
        /// This is used to determine the master indexer if this app exists in a load balanced environment.
        /// </summary>
        private void CreateAnnounceFile()
        {

            var d = new SerializableDictionary<string, string>();
            d.Add("Name", Environment.MachineName);
            d.Add("Created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            d.Add("Updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            d.Add("IsMaster", false.ToString());
            d.SaveToDisk(ExaFile);
        }

        private void ClearOldLockFiles(DateTime cutoffTime)
        {
            //delete all old lck files (any that are more than cutoffTime old)
            ExamineDirectory
                .GetFiles("*.lck")
                .Where(x => x.CreationTime < cutoffTime)
                .ToList()
                .ForEach(x => x.Delete());

        }
        /// <summary>
        /// Get all lck files that are not named by this machines name. If there are any, this means that another machine
        /// has won the race and created the lck file for itself. If there is a lck file with the current machines name, then this
        /// must mean it was previously the master indexer and the apppool has recycled in less than the hour.
        /// </summary>
        /// <returns></returns>
        private List<FileInfo> GetLockFiles()
        {
            return ExamineDirectory
                .GetFiles("*.lck")
                .Where(x => !x.Name.StartsWith(Environment.MachineName))
                .ToList();
        }

        private int LockFileCount()
        {
            return ExamineDirectory
               .GetFiles("*.lck")
               .Count();                
        }

        /// <summary>
        /// This will check for any lock files, not created by the current machine. If there are any, then this machine will flag it's
        /// exa file as not being the master indexer, otherwise, it will try to create it's own lock file to let others know it is the race
        /// winner and therefore the master indexer. If this succeeds, it will update it's exa file to flag it as the master indexer.
        /// </summary>
        private void RaceForMasterIndexer()
        {
            //get this machine's exa file
            var dExa = new SerializableDictionary<string, string>();
            dExa.ReadFromDisk(ExaFile);

            if (GetLockFiles().Count == 0)
            {
                dExa["IsMaster"] = true.ToString();

                var lckFileName = Environment.MachineName + ".lck";
                var fLck = new FileInfo(Path.Combine(ExamineDirectory.FullName, lckFileName));
                var dLck = new SerializableDictionary<string, string>();
                dLck.Add("Name", Environment.MachineName);
                dLck.Add("Created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                //check one more time
                if (GetLockFiles().Count == 0)
                {
                    dLck.SaveToDisk(fLck);
                }
                else
                {
                    //if there is an lck file at this stage, the race was won by a hair by another machine, so this machine
                    //will back down.
                    dExa["IsMaster"] = false.ToString();
                }
            }
            else
            {
                dExa["IsMaster"] = false.ToString();
            }
            dExa["Updated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            dExa.SaveToDisk(ExaFile);

        }



    }
}
