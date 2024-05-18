using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace MicrobotApi.Services;

public class AzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string BlobContainer = "microbot";
    
    public AzureStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }
    
    public static Uri GetSasUri(BlobBaseClient blobClient, string storedPolicyName = null)
    {
        if (!blobClient.CanGenerateSasUri)
            return null;

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b"
        };

        if (storedPolicyName == null)
        {
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(2);
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
        }
        else
        {
            sasBuilder.Identifier = storedPolicyName;
        }

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri;
    }

    public Task<Response<BlobDownloadInfo>> DownloadFile(string storagePath)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainer);
        var blobClient = containerClient.GetBlobClient(storagePath);

        var blobData =  blobClient.DownloadAsync();

        return blobData;
    }
}