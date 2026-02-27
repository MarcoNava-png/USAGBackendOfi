using System.Collections.Generic;

namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class GenerarRecibosAdmisionResultDto
    {
        public List<ReciboDto> RecibosAdmision { get; set; } = new();
        public List<ReciboDto> RecibosMensualidades { get; set; } = new();
        public int TotalRecibos => RecibosAdmision.Count + RecibosMensualidades.Count;
    }
}
