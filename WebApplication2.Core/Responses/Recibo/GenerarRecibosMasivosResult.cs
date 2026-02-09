using WebApplication2.Core.DTOs.Recibo;

namespace WebApplication2.Core.Responses.Recibo
{
    public class GenerarRecibosMasivosResult
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = null!;
        public int TotalEstudiantes { get; set; }
        public int TotalRecibosGenerados { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal TotalDescuentosBecas { get; set; }
        public int EstudiantesOmitidos { get; set; }
        public List<string>? Errores { get; set; }
        public List<ReciboEstudianteResumen>? DetalleEstudiantes { get; set; }
    }
}
