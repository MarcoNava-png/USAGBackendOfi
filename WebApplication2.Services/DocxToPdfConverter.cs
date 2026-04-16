using System.Diagnostics;

namespace WebApplication2.Services
{
    public static class DocxToPdfConverter
    {
        public static async Task<byte[]> ConvertAsync(byte[] docxBytes, CancellationToken ct = default)
        {
            var tmpDir = Path.Combine(Directory.GetCurrentDirectory(), "tmp");
            Directory.CreateDirectory(tmpDir);

            var inputFile = Path.Combine(tmpDir, $"{Guid.NewGuid()}.docx");
            var outputFile = Path.ChangeExtension(inputFile, ".pdf");

            try
            {
                await File.WriteAllBytesAsync(inputFile, docxBytes, ct);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "libreoffice",
                        Arguments = $"--headless --norestore --convert-to pdf --outdir \"{tmpDir}\" \"{inputFile}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Environment =
                        {
                            ["HOME"] = Path.Combine(Directory.GetCurrentDirectory(), "tmp")
                        }
                    }
                };

                process.Start();
                await process.WaitForExitAsync(ct);

                if (!File.Exists(outputFile))
                    throw new InvalidOperationException("LibreOffice no pudo convertir el documento");

                return await File.ReadAllBytesAsync(outputFile, ct);
            }
            finally
            {
                if (File.Exists(inputFile)) File.Delete(inputFile);
                if (File.Exists(outputFile)) File.Delete(outputFile);
            }
        }
    }
}
