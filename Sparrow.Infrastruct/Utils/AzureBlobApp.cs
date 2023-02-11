using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace SparrowPlatform.Infrastruct.Utils
{

    public static class AzureBlobApp
    {

        public static AzureResponse CreateFolderInDataLake(string tenantName)
        {
            AzureResponse azureResponse = new AzureResponse();
            if (!string.IsNullOrEmpty(tenantName))
            {
                try
                {
                    string containerName = "analytics";

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AzureADAppSetup.connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                    string newDirPath = $"ingress/TripDetails/00_raw/{tenantName}/init-file";
                    CloudBlockBlob blob = container.GetBlockBlobReference(newDirPath);
                    CloudBlockBlob GPSTracksBlob = container.GetBlockBlobReference($"ingress/GPSTracks/00_raw/{tenantName}/init-file");
                    CloudBlockBlob TripWarningsBlob = container.GetBlockBlobReference($"ingress/TripWarnings/00_raw/{tenantName}/init-file");
                    if (!blob.ExistsAsync().Result)
                    {
                        // in fact, there is no directory concept on Azure Blob Storage, so we just create a placeholder file
                        blob.UploadTextAsync("__placeholder__").Wait();

                        GPSTracksBlob.UploadTextAsync("__placeholder__").Wait();
                        TripWarningsBlob.UploadTextAsync("__placeholder__").Wait();

                        azureResponse.suc = true;
                        azureResponse.msg = "Creating a successful";
                    }
                    else
                    {
                        azureResponse.msg = "There is a folder with the same name already exists.";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    azureResponse.msg = e.Message;
                }
            }

            return azureResponse;

        }
    }
    public class AzureResponse
    {
        public bool suc { get; set; } = false;
        public string msg { get; set; } = "azure server is wrong!";
    }
    public class CallBackResponse
    {
        public bool suc { get; set; } = false;
        public string msg { get; set; } = "Apply callback failed!";
    }
}
