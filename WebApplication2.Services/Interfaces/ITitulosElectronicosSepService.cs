namespace WebApplication2.Services.Interfaces
{
    public class EnvioSepResultDto
    {
        public bool Exitoso { get; set; }
        public int? NumeroLote { get; set; }
        public int? EstatusLote { get; set; }
        public string? FolioControl { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? ArchivoBase64 { get; set; }
        public string? RespuestaCruda { get; set; }
    }

    public interface ITitulosElectronicosSepService
    {
        Task<EnvioSepResultDto> EnviarAsync(int idCertificado, string? cveInstitucionOverride = null, CancellationToken ct = default);
        Task<EnvioSepResultDto> ConsultarAsync(int idCertificado, CancellationToken ct = default);
        Task<EnvioSepResultDto> DescargarAsync(int idCertificado, CancellationToken ct = default);
    }
}
