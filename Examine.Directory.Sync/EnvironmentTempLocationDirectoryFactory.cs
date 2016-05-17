using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;

namespace Examine.Directory.Sync
{
    /// <summary>
    /// A directory factory used to create an instance of SyncDirectory that uses the current %temp% environment variable
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync
    /// </remarks>
    public class EnvironmentTempLocationDirectoryFactory : IDirectoryFactory
    {
        public Lucene.Net.Store.Directory CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder)
        {
            var indexFolder = new DirectoryInfo(luceneIndexFolder);
            var tempFolder = GetLocalStorageDirectory(indexFolder);
            var master = new DirectoryInfo(luceneIndexFolder);
            return new SyncDirectory(new SimpleFSDirectory(master), new SimpleFSDirectory(tempFolder));
        }

        public DirectoryInfo GetLocalStorageDirectory(DirectoryInfo indexPath)
        {
            var appDomainHash = ToMd5(HttpRuntime.AppDomainAppId);
            var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "LuceneDir",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash, "App_Data", "TEMP", "ExamineIndexes", indexPath.Name);
            var azureDir = new DirectoryInfo(cachePath);
            if (azureDir.Exists == false)
                azureDir.Create();
            return azureDir;
        }        

        /// <summary>
        /// Converts the string to MD5
        /// </summary>
        /// <param name="stringToConvert">referrs to itself</param>
        /// <returns>the md5 hashed string</returns>
        private static string ToMd5(string stringToConvert)
        {
            //create an instance of the MD5CryptoServiceProvider
            var md5Provider = new MD5CryptoServiceProvider();

            //convert our string into byte array
            var byteArray = Encoding.UTF8.GetBytes(stringToConvert);

            //get the hashed values created by our MD5CryptoServiceProvider
            var hashedByteArray = md5Provider.ComputeHash(byteArray);

            //create a StringBuilder object
            var stringBuilder = new StringBuilder();

            //loop to each each byte
            foreach (var b in hashedByteArray)
            {
                //append it to our StringBuilder
                stringBuilder.Append(b.ToString("x2").ToLower());
            }

            //return the hashed value
            return stringBuilder.ToString();
        }
    }
}