using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using System.IO;
using System.Net;

namespace Lucene.Net.Store.Azure
{
    public static class LeaseBlobExtensions
    {
        public static string AcquireLease(this CloudBlob blob)
        {
            var creds = blob.ServiceClient.Credentials;
            var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
            var req = BlobRequest.Lease(transformedUri,
                60, // timeout (in seconds)
                LeaseAction.Acquire, // as opposed to "break" "release" or "renew"
                null); // name of the existing lease, if any
            blob.ServiceClient.Credentials.SignRequest(req);
            using (var response = req.GetResponse())
            {
                return response.Headers["x-ms-lease-id"];
            }
        }

        private static void DoLeaseOperation(CloudBlob blob, string leaseId, LeaseAction action)
        {
            var creds = blob.ServiceClient.Credentials;
            var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
            var req = BlobRequest.Lease(transformedUri, 60, action, leaseId);
            creds.SignRequest(req);
            req.GetResponse().Close();
        }

        public static void ReleaseLease(this CloudBlob blob, string leaseId)
        {
            DoLeaseOperation(blob, leaseId, LeaseAction.Release);
        }

        public static void RenewLease(this CloudBlob blob, string leaseId)
        {
            DoLeaseOperation(blob, leaseId, LeaseAction.Renew);
        }

        public static void BreakLease(this CloudBlob blob)
        {
            DoLeaseOperation(blob, null, LeaseAction.Break);
        }

        // NOTE: This method doesn't do everything that the regular UploadText does.
        // Notably, it doesn't update the BlobProperties of the blob (with the new
        // ETag and LastModifiedTimeUtc). It also, like all the methods in this file,
        // doesn't apply any retry logic. Use this at your own risk!
        public static void UploadText(this CloudBlob blob, string text, string leaseId)
        {
            string url = blob.Uri.ToString();
            if (blob.ServiceClient.Credentials.NeedsTransformUri)
            {
                url = blob.ServiceClient.Credentials.TransformUri(url);
            }
            var req = BlobRequest.Put(new Uri(url), 60, new BlobProperties(), BlobType.BlockBlob, leaseId, 0);
            using (var writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write(text);
            }
            blob.ServiceClient.Credentials.SignRequest(req);
            req.GetResponse().Close();
        }
    }
}