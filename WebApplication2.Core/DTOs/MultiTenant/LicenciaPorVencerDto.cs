namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class LicenciaPorVencerDto
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string Plan { get; set; } = null!;
        public DateTime? FechaVencimiento { get; set; }
        public int DiasRestantes { get; set; }
        public string EmailContacto { get; set; } = null!;
    }
}
