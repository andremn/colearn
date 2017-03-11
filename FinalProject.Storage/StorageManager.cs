using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace FinalProject.Storage
{
    public class StorageManager
    {
        public async Task UploadBlockAsync(string containerName, string blobName, string blockId, Stream blockData)
        {
            try
            {
                var connectionString = Configuration.StorageConnectionString;
                var account = CloudStorageAccount.Parse(connectionString);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference(containerName);

                container.CreateIfNotExists();

                var blob = container.GetBlockBlobReference(blobName);

                await blob.PutBlockAsync(blockId, blockData, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}