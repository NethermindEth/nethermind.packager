using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Nethermind.Packager.Core.Services.Storage.Azure
{
    public class AzureStorageUploader : IStorageUploader
    {
        private readonly IOptions<StorageOptions> _storageOptions;

        public AzureStorageUploader(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions;
        }
        
        public async Task UploadAsync(byte[] bytes, string name)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageOptions.Value.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_storageOptions.Value.Directory);
            var blob = container.GetBlockBlobReference(name);
            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length, null,
                new BlobRequestOptions {StoreBlobContentMD5 = true}, null);
        }
        
        private static string CreateHash(byte[] bytes)
        {
            using (var md5 = MD5.Create())
            {
                var md5Hash = md5.ComputeHash(bytes);
                return Convert.ToBase64String(md5Hash);
            }
        }
    }
}