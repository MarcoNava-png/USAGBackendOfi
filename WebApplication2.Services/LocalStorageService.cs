using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApplication2.Services.Interfaces;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Services
{
    public class LocalStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly string _basePath;
        private readonly string _baseUrl;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv",
            ".txt", ".zip", ".rar"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public LocalStorageService(IConfiguration configuration, ITenantContextAccessor tenantContextAccessor)
        {
            _configuration = configuration;
            _tenantContextAccessor = tenantContextAccessor;
            _basePath = _configuration["LocalStorage:BasePath"] ?? "/app/uploads";
            _baseUrl = _configuration["LocalStorage:BaseUrl"] ?? "/uploads";
        }

        private string GetTenantPrefix()
        {
            var codigo = _tenantContextAccessor?.TenantContext?.Codigo;
            if (string.IsNullOrWhiteSpace(codigo) || codigo.Equals("USAG", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return Path.GetFileName(codigo.ToLowerInvariant());
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
            var tenantPrefix = GetTenantPrefix();

            try
            {
                var containerPath = string.IsNullOrEmpty(tenantPrefix)
                    ? Path.Combine(_basePath, sanitizedContainer)
                    : Path.Combine(_basePath, tenantPrefix, sanitizedContainer);
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

                var publicUrl = string.IsNullOrEmpty(tenantPrefix)
                    ? $"{_baseUrl}/{sanitizedContainer}/{sanitizedBlob}"
                    : $"{_baseUrl}/{tenantPrefix}/{sanitizedContainer}/{sanitizedBlob}";
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
                var tenantPrefix = GetTenantPrefix();
                var filePath = string.IsNullOrEmpty(tenantPrefix)
                    ? Path.Combine(_basePath, sanitizedContainer, sanitizedBlob)
                    : Path.Combine(_basePath, tenantPrefix, sanitizedContainer, sanitizedBlob);

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

        public string? ResolverRutaLocal(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            string relative;
            if (url.StartsWith(_baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                relative = url.Substring(_baseUrl.Length);
            }
            else
            {
                var idx = url.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    return null;
                relative = url.Substring(idx + "/uploads".Length);
            }

            relative = relative.TrimStart('/', '\\');
            if (string.IsNullOrEmpty(relative) || relative.Contains(".."))
                return null;

            var fullPath = Path.GetFullPath(Path.Combine(_basePath, relative));
            if (!fullPath.StartsWith(Path.GetFullPath(_basePath)))
                return null;

            return File.Exists(fullPath) ? fullPath : null;
        }
    }
}
