using System;
using System.Collections.Generic;
using WebApplication2.Core.DTOs.Recibo;

namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class CotizacionAdmisionPdfDto
    {
        public string NombreAspirante { get; set; } = "";
        public string Licenciatura { get; set; } = "";
        public string ClavePlan { get; set; } = "";
        public string NombreTarifa { get; set; } = "";
        public DateOnly Fecha { get; set; }
        public List<CotizacionConceptoDto> Conceptos { get; set; } = new();
        public InstitucionPdfDto? Institucion { get; set; }
    }

    public class CotizacionConceptoDto
    {
        public string Nombre { get; set; } = "";
        public string Valor { get; set; } = "N/A";
    }
}
