using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Queues;

namespace AzureSQLDBConnectionAPI.Controllers;


[Route("api/Uploads")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private BlobServiceClient _blobServiceClient;
    private readonly string _containerName; // Name of your Azure Blob Storage container
    private readonly QueueServiceClient queueServiceClient;
    private readonly SecretClient secretClient;

    public FileUploadController(IConfiguration configuration, QueueServiceClient queueServiceClient, SecretClient secretClient)
    {


        //try
        //{
        //    // Use Azure Managed Identity to authenticate and retrieve secrets from Key Vault
        //    var credential = new DefaultAzureCredential();
        //    var keyVaultUrl = "https://rlkvalut.vault.azure.net/"; // Replace with your Key Vault URL

        //    var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

        //    // Retrieve the Azure Blob Storage connection string from Key Vault
        //    var secretName = "StorageConnectionString"; // Replace with your secret name
        //    var secretResponse = secretClient.GetSecret(secretName);

        //    // Initialize BlobServiceClient using the retrieved connection string
        //    _blobServiceClient = new BlobServiceClient(secretResponse.Value.Value);
        //}
        //catch (Exception ex)
        //{
        //    throw new ApplicationException($"Error retrieving Azure Blob Storage connection string: {ex.Message}");
        //}
        // Retrieve container name from app settings or hardcode it here

        _containerName = "rlkblobc1";
        this.queueServiceClient = queueServiceClient;
        this.secretClient = secretClient;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is not selected or empty");
        }

        try
        {

            var secretName = "StorageConnectionString"; // Replace with your secret name
            var secretResponse = secretClient.GetSecret(secretName);

            // Initialize BlobServiceClient using the retrieved connection string
            _blobServiceClient = new BlobServiceClient(secretResponse.Value.Value);

            // Get a reference to a blob container
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Create the container if it does not exist
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to a blob
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Open a stream to the file contents
            using (var stream = file.OpenReadStream())
            {
                // Upload the file to Azure Storage
                await blobClient.UploadAsync(stream, true);
            }

            // Optionally, you can return the URL of the uploaded blob
            string blobUrl = blobClient.Uri.AbsoluteUri;

            var queueClient = queueServiceClient.GetQueueClient("rlk-storagequeue");

            await queueClient.CreateIfNotExistsAsync();

            await queueClient.SendMessageAsync(blobUrl);

            return Ok(new { FileUrl = blobUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }
}