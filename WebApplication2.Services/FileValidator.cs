using Microsoft.AspNetCore.Http;

namespace WebApplication2.Services
{
    public static class FileValidator
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly Dictionary<string, byte[][]> MagicBytes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf",  new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },                         // %PDF
            { ".jpg",  new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png",  new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } }, // PNG header
            { ".doc",  new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } } },                         // OLE Compound
            { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },                         // ZIP/OOXML
        };

        public static async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "El archivo esta vacio.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                return (false, $"Extension de archivo no permitida. Solo se permiten: {string.Join(", ", AllowedExtensions)}");

            if (file.Length > MaxFileSizeBytes)
                return (false, $"El archivo excede el tamano maximo permitido de {MaxFileSizeBytes / (1024 * 1024)} MB.");

            if (MagicBytes.TryGetValue(extension, out var signatures))
            {
                var maxLen = signatures.Max(s => s.Length);
                var header = new byte[maxLen];

                using var stream = file.OpenReadStream();
                var bytesRead = await stream.ReadAsync(header, 0, maxLen);

                if (bytesRead < maxLen)
                    return (false, "El archivo es demasiado pequeno o esta corrupto.");

                var matches = signatures.Any(sig =>
                    header.Take(sig.Length).SequenceEqual(sig));

                if (!matches)
                    return (false, $"El contenido del archivo no coincide con la extension {extension}. El archivo podria estar corrupto o haber sido renombrado.");
            }

            return (true, null);
        }
    }
}
