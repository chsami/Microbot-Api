using System.Web;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace MicrobotApi.Services;

public class AzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;
    private const string BlobContainer = "microbot";
    
    public AzureStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _configuration = configuration;
    }
    
    public static Uri GetSasUri(BlobBaseClient blobClient)
    {
        if (!blobClient.CanGenerateSasUri)
            return null;

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b"
        };

        sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5);
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri;
    }

    public Task<Response<BlobDownloadInfo>> DownloadFile(string storagePath)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainer);
        var blobClient = containerClient.GetBlobClient(HttpUtility.UrlDecode(storagePath));
        
        var blobData =  blobClient.DownloadAsync();

        return blobData;
    }

    public async Task<Uri> GetDownloadUrl(string storagePath)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainer);
        
        BlobClient blobClient = containerClient.GetBlobClient(HttpUtility.UrlDecode(storagePath));
        Uri sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        return sasUri;
    }
    
    public async Task<List<string>> GetFileNames(string storagePath, string? fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainer);

        var fileNames = new List<string>();

        // List blobs in the container
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: storagePath))
        {
            if (string.IsNullOrWhiteSpace(fileName) || blobItem.Name.Contains(fileName))
            {
                fileNames.Add(blobItem.Name);
            }
        }

        return fileNames;
    }
}