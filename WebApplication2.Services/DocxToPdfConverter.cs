using System.Diagnostics;

namespace WebApplication2.Services
{
    public static class DocxToPdfConverter
    {
        public static Task<byte[]> ConvertAsync(byte[] docxBytes, CancellationToken ct = default)
            => ConvertAsync(docxBytes, ".docx", ct);

        public static async Task<byte[]> ConvertAsync(byte[] sourceBytes, string sourceExtension, CancellationToken ct = default)
        {
            var ext = string.IsNullOrWhiteSpace(sourceExtension) ? ".docx" : sourceExtension.Trim();
            if (!ext.StartsWith('.')) ext = "." + ext;

            var tmpDir = Path.Combine(Directory.GetCurrentDirectory(), "tmp");
            Directory.CreateDirectory(tmpDir);

            var workId = Guid.NewGuid().ToString("N");
            var inputFile = Path.Combine(tmpDir, $"{workId}{ext}");
            var outputFile = Path.Combine(tmpDir, $"{workId}.pdf");
            var profileDir = Path.Combine(tmpDir, $"lo_{workId}");
            Directory.CreateDirectory(profileDir);

            try
            {
                await File.WriteAllBytesAsync(inputFile, sourceBytes, ct);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "libreoffice",
                        Arguments = $"-env:UserInstallation=file://{profileDir} --headless --norestore --convert-to pdf --outdir \"{tmpDir}\" \"{inputFile}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Environment = { ["HOME"] = profileDir }
                    }
                };

                process.Start();
                var stderr = await process.StandardError.ReadToEndAsync(ct);
                await process.WaitForExitAsync(ct);

                if (!File.Exists(outputFile))
                    throw new InvalidOperationException(
                        $"LibreOffice no pudo convertir el documento (archivo {ext}). {stderr}".Trim());

                return await File.ReadAllBytesAsync(outputFile, ct);
            }
            finally
            {
                if (File.Exists(inputFile)) File.Delete(inputFile);
                if (File.Exists(outputFile)) File.Delete(outputFile);
                if (Directory.Exists(profileDir)) { try { Directory.Delete(profileDir, true); } catch { } }
            }
        }
    }
}
