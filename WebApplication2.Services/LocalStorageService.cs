using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class LocalStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _basePath;
        private readonly string _baseUrl;

        public LocalStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _basePath = _configuration["LocalStorage:BasePath"] ?? "/app/uploads";
            _baseUrl = _configuration["LocalStorage:BaseUrl"] ?? "/uploads";
        }

        public async Task<string> UploadFile(IFormFile formFile, string blobName, string containerName)
        {
            try
            {
                var containerPath = Path.Combine(_basePath, containerName);
                var filePath = Path.Combine(containerPath, blobName);
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                var publicUrl = $"{_baseUrl}/{containerName}/{blobName}";
                return publicUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo cargar el archivo: " + ex.Message);
            }
        }

        public async Task DeleteFile(string blobName, string containerName)
        {
            try
            {
                var filePath = Path.Combine(_basePath, containerName, blobName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
            }
        }
    }
}
