using System.Collections.Generic;
using WebApplication2.Core.DTOs.Recibo;

namespace WebApplication2.Core.DTOs.Aspirante
{
    public class GenerarMensualidadesResultDto
    {
        public bool Success { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int RecibosGenerados { get; set; }
        public int MensualidadesPorPeriodo { get; set; }
        public List<ReciboDto> Recibos { get; set; } = new();
    }
}
