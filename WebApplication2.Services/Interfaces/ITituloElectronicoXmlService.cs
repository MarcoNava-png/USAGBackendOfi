namespace WebApplication2.Services.Interfaces
{
    public interface ITituloElectronicoXmlService
    {
        Task<string> GenerarXmlAsync(int idCertificado, string? cveInstitucionOverride = null, CancellationToken ct = default);
    }
}
