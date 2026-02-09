using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadFile(IFormFile formFile, string blobName, string containerName)
        {
            try
            {
                var azureBlobStorageConnectionString = _configuration["Azure:AzureBlobStorageConnectionString"];

                BlobContainerClient containerClient = new BlobContainerClient(azureBlobStorageConnectionString, containerName);

                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                using (var stream = formFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo cargar el archivo: " + ex.Message);
            }
        }

        public async Task DeleteFile(string blobName, string containerName)
        {
            var azureBlobStorageConnectionString = _configuration["Azure:AzureBlobStorageConnectionString"];

            BlobContainerClient containerClient = new BlobContainerClient(azureBlobStorageConnectionString, containerName);

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                await blobClient.DeleteAsync();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine("El blob no existe.");
            }
        }
    }
}
