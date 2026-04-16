namespace WebApplication2.Services.Interfaces
{
    public interface ICertificadoXmlService
    {
        Task<string> GenerarXmlAsync(int idCertificado, CancellationToken ct = default);
        string GenerarCadenaOriginal(int idCertificado, string xml);
    }
}
