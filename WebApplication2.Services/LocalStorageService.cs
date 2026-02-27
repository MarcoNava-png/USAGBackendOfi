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

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv",
            ".txt", ".zip", ".rar"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public LocalStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _basePath = _configuration["LocalStorage:BasePath"] ?? "/app/uploads";
            _baseUrl = _configuration["LocalStorage:BaseUrl"] ?? "/uploads";
        }

        public async Task<string> UploadFile(IFormFile formFile, string blobName, string containerName)
        {
            if (formFile == null || formFile.Length == 0)
                throw new ArgumentException("El archivo está vacío.");

            if (formFile.Length > MaxFileSizeBytes)
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {MaxFileSizeBytes / (1024 * 1024)} MB.");

            var extension = Path.GetExtension(blobName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Tipo de archivo no permitido: {extension}");

            // Path traversal protection
            if (containerName.Contains("..") || blobName.Contains(".."))
                throw new ArgumentException("Nombre de archivo no válido.");

            var sanitizedContainer = Path.GetFileName(containerName);
            var sanitizedBlob = Path.GetFileName(blobName);

            try
            {
                var containerPath = Path.Combine(_basePath, sanitizedContainer);
                var filePath = Path.Combine(containerPath, sanitizedBlob);

                // Verify resolved path is within base path
                var resolvedPath = Path.GetFullPath(filePath);
                if (!resolvedPath.StartsWith(Path.GetFullPath(_basePath)))
                    throw new ArgumentException("Ruta de archivo no válida.");

                var directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                var publicUrl = $"{_baseUrl}/{sanitizedContainer}/{sanitizedBlob}";
                return publicUrl;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new Exception("No se pudo cargar el archivo.");
            }
        }

        public async Task DeleteFile(string blobName, string containerName)
        {
            try
            {
                if (containerName.Contains("..") || blobName.Contains(".."))
                    return;

                var sanitizedContainer = Path.GetFileName(containerName);
                var sanitizedBlob = Path.GetFileName(blobName);
                var filePath = Path.Combine(_basePath, sanitizedContainer, sanitizedBlob);

                var resolvedPath = Path.GetFullPath(filePath);
                if (!resolvedPath.StartsWith(Path.GetFullPath(_basePath)))
                    return;

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
