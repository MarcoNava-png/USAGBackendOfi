using System.Text.Json.Serialization;

namespace WebApplication2.Core.DTOs.Caja
{
    public class ResumenCorteCajaDto
    {
        public CajeroInfoDto? Cajero { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<PagoDetalladoDto> Pagos { get; set; } = new();
        public TotalesCorteCajaDto Totales { get; set; } = new();

        [JsonIgnore]
        public List<PagoDto>? PagosSimples { get; set; }
    }
}
