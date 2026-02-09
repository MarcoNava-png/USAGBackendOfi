namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ResultadoEnvioNotificacionesDto
    {
        public int NotificacionesCreadas { get; set; }
        public int EmailsEnviados { get; set; }
        public int Errores { get; set; }
        public List<string> Detalles { get; set; } = new();
    }
}
