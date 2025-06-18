using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Skeleton.Business.Repositories
{
    public class AzureStorageRepository
    {
        private readonly ILogger<AzureStorageRepository> _logger;
        private readonly AzureStorageOptions _options;
        private readonly BlobServiceClient _blobServiceClient;

        public int SASLifetimeMinutes => _options.SASLifetimeMinutes;

        public AzureStorageRepository(ILogger<AzureStorageRepository> logger, IOptions<AzureStorageOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _blobServiceClient = new BlobServiceClient(_options.ServiceUri, new DefaultAzureCredential());
        }

        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string containerName, string fileName)
        {
            try
            {
                BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

                await blobClient.DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AzureStorageRepository:DeleteAsync failed for container {ContainerName} -> file {FileName}", containerName, fileName);
                return false;
            }
        }

        /// <summary>
        /// Calculates the storage used by a container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>Storage used by a container in bytes</returns>
        public async Task<long> GetStorageSizeAsync(string containerName, CancellationToken cancellationToken = default)
        {
            long containerSize = 0;

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var pagedBlobs = containerClient.GetBlobsAsync(cancellationToken: cancellationToken).AsPages();
            await foreach (Page<BlobItem> blobPage in pagedBlobs)
            {
                long blobPageSize = blobPage.Values.Sum(b => b.Properties.ContentLength.GetValueOrDefault());
                Interlocked.Add(ref containerSize, blobPageSize);
            }

            return containerSize;
        }

        /// <summary>
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>If the operation succeeded</returns>
        public async Task<bool> ProvisionStorageAsync(string containerName)
        {
            try
            {
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AzureStorageRepository:ProvisionStorageAsync failed for container {ContainerName}", containerName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uploads a file to a container, can overwrite existing files by that name
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <param name="overwrite"></param>
        /// <returns>Link to the uploaded file</returns>
        public async Task<string?> UploadAsync(string containerName, string fileName, Stream file, bool overwrite = false)
        {
            try
            {
                BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(file, overwrite);

                return await GetLinkAsync(containerName, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AzureStorageRepository:UploadAsync failed for container {ContainerName} -> file {FileName}", containerName, fileName);
                return null;
            }
        }

        public async Task UpdateMetadataAsync(string containerName, string fileName, Dictionary<string, string> metadata)
        {
            try
            {
                BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

                await blobClient.SetMetadataAsync(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AzureStorageRepository:UpdateMetadataAsync failed for container {ContainerName} -> file {FileName}", containerName, fileName);
            }
        }

        ///// <summary>
        ///// this enumerates all containers, we likely are only interested in calculating a single users size
        ///// Saving this sample code though
        ///// </summary>
        ///// <param name="blobServiceClient"></param>
        ///// <param name="connectionString"></param>
        ///// <param name="prefix"></param>
        ///// <param name="segmentSize"></param>
        ///// <returns></returns>
        //private async Task<ConcurrentDictionary<string, long>> GetContainersSize(BlobServiceClient blobServiceClient,
        //                        string connectionString,
        //                        string prefix,
        //                        int? segmentSize)
        //{
        //    string continuationToken = string.Empty;
        //    var sizes = new ConcurrentDictionary<string, long>();
        //    try
        //    {

        //        do
        //        {
        //            // Call the listing operation and enumerate the result segment.
        //            // When the continuation token is empty, the last segment has been returned
        //            // and execution can exit the loop.
        //            var resultSegment = blobServiceClient.GetBlobContainersAsync(BlobContainerTraits.Metadata, prefix, default).AsPages(continuationToken, segmentSize);
        //            await foreach (Page<BlobContainerItem> containerPage in resultSegment)
        //            {
        //                foreach (BlobContainerItem containerItem in containerPage.Values)
        //                {
        //                    BlobContainerClient container = new(connectionString, containerItem.Name);

        //                    var blobs = container.GetBlobsAsync().AsPages(continuationToken);

        //                    await foreach (var blobPage in blobs)
        //                    {
        //                        var blobPageSize = blobPage.Values.Sum(b => b.Properties.ContentLength.GetValueOrDefault());
        //                        sizes.AddOrUpdate(containerItem.Name, blobPageSize, (key, currentSize) => currentSize + blobPageSize);
        //                    }
        //                }

        //                // Get the continuation token and loop until it is empty.
        //                continuationToken = containerPage.ContinuationToken;
        //            }

        //        } while (continuationToken != string.Empty);

        //        return sizes;
        //    }
        //    catch (RequestFailedException e)
        //    {
        //        Console.WriteLine(e.Message);
        //        Console.ReadLine();
        //        throw;
        //    }
        //}

        public async Task<string> GetLinkAsync(string containerName, string fileName)
        {
            //recommended access method, requires system user to have an rbac role with Microsoft.Storage/storageAccounts/blobServices/generateUserDelegationKey
            UserDelegationKey key = await _blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(_options.SASLifetimeMinutes));
            BlobSasBuilder sasBuilder = new()
            {
                BlobContainerName = containerName,
                BlobName = fileName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_options.SASLifetimeMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return $"{_options.ServiceUri}{containerName}/{fileName}?{sasBuilder.ToSasQueryParameters(key, _options.AccountName)}";
        }

        public async Task<Dictionary<string, string>> GetLinksAsync(string containerName, IEnumerable<string> fileNames)
        {
            //recommended access method, rerquires system user to have an rbac role with Microsoft.Storage/storageAccounts/blobServices/generateUserDelegationKey
            UserDelegationKey key = await _blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(_options.SASLifetimeMinutes));
            BlobSasBuilder sasBuilder = new()
            {
                BlobContainerName = containerName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_options.SASLifetimeMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            Dictionary<string, string> downloadUrls = [];

            foreach (string fileName in fileNames)
            {
                sasBuilder.BlobName = fileName;

                string downloadUrl = $"{_options.ServiceUri}{containerName}/{fileName}?{sasBuilder.ToSasQueryParameters(key, _options.AccountName)}";

                downloadUrls.Add(fileName, downloadUrl);
            }

            return downloadUrls;
        }
    }
}
