//    License: Microsoft Public License (Ms-PL) 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.StorageClient;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Lucene.Net.Store.Azure
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory via a blob lease
    /// </summary>
    public class AzureLock : Lock
    {
        private string _lockFile;
        private AzureDirectory _azureDirectory;
        private string _leaseid;

        public AzureLock(string lockFile, AzureDirectory directory)
        {
            _lockFile = lockFile;
            _azureDirectory = directory;
        }

        #region Lock methods
        override public bool IsLocked()
        {
            var blob = _azureDirectory.BlobContainer.GetBlobReference(_lockFile);
            try
            {
                Debug.Print("IsLockeD() : {0}", _leaseid);
                if (String.IsNullOrEmpty(_leaseid))
                {
                    var tempLease = blob.AcquireLease();
                    if (String.IsNullOrEmpty(tempLease))
                    {
                        Debug.Print("IsLocked() : TRUE");
                        return true;
                    }
                    blob.ReleaseLease(tempLease);
                }
                Debug.Print("IsLocked() : {0}", _leaseid);
                return String.IsNullOrEmpty(_leaseid);
            }
            catch (WebException webErr)
            {
                if (_handleWebException(blob, webErr))
                    return IsLocked();
            }
            catch (StorageClientException err)
            {
                if (_handleStorageClientException(blob, err))
                    return IsLocked();
            }
            _leaseid = null;
            return false;
        }

        public override bool Obtain()
        {
            var blob = _azureDirectory.BlobContainer.GetBlobReference(_lockFile);
            try
            {
                Debug.Print("AzureLock:Obtain({0}) : {1}", _lockFile, _leaseid);
                if (String.IsNullOrEmpty(_leaseid))
                {
                    _leaseid = blob.AcquireLease();
                    Debug.Print("AzureLock:Obtain({0}): AcquireLease : {1}", _lockFile, _leaseid);
                    
                    // keep the lease alive by renewing every 30 seconds
                    long interval = (long)TimeSpan.FromSeconds(30).TotalMilliseconds;
                    _renewTimer = new Timer((obj) => 
                        {
                            try
                            {
                                AzureLock al = (AzureLock)obj;
                                al.Renew();
                            }
                            catch (Exception err) { Debug.Print(err.ToString()); } 
                        }, this, interval, interval);
                }
                return !String.IsNullOrEmpty(_leaseid);
            }
            catch (WebException webErr)
            {
                if (_handleWebException(blob, webErr))
                    return Obtain();
            }
            catch (StorageClientException err)
            {
                if (_handleStorageClientException(blob, err))
                    return Obtain();
            }
            return false;
        }

        private Timer _renewTimer;

        public void Renew()
        {
            if (!String.IsNullOrEmpty(_leaseid))
            {
                Debug.Print("AzureLock:Renew({0} : {1}", _lockFile, _leaseid);
                var blob = _azureDirectory.BlobContainer.GetBlobReference(_lockFile);
                blob.RenewLease(_leaseid);
            }
        }

        public override void Release()
        {
            Debug.Print("AzureLock:Release({0}) {1}", _lockFile, _leaseid);
            if (!String.IsNullOrEmpty(_leaseid))
            {
                var blob = _azureDirectory.BlobContainer.GetBlobReference(_lockFile);
                blob.ReleaseLease(_leaseid);
                if (_renewTimer != null)
                {
                    _renewTimer.Dispose();
                    _renewTimer = null;
                }
                _leaseid = null;
            }
        }
        #endregion

        public void BreakLock()
        {
            Debug.Print("AzureLock:BreakLock({0}) {1}", _lockFile, _leaseid);
            var blob = _azureDirectory.BlobContainer.GetBlobReference(_lockFile);
            try
            {
                blob.BreakLease();
            }
            catch (Exception err)
            {
            }
            _leaseid = null;
        }

        public override System.String ToString()
        {
            return String.Format("AzureLock@{0}.{1}", _lockFile, _leaseid);
        }

        private bool _handleWebException(CloudBlob blob, WebException err)
        {
            var response = (HttpWebResponse)err.Response;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _azureDirectory.CreateContainer();
                blob.UploadText(_lockFile);
                return true;
            }
            return false;
        }

        private bool _handleStorageClientException(CloudBlob blob, StorageClientException err)
        {
            switch (err.ErrorCode)
            {
                case StorageErrorCode.ResourceNotFound:
                    blob.UploadText(_lockFile);
                    return true;

                case StorageErrorCode.ContainerNotFound:
                    // container is missing, we should create it.
                    _azureDirectory.BlobContainer.Delete();
                    _azureDirectory.CreateContainer();
                    return true;

                default:
                    return false;
            }
        }

    }

}
