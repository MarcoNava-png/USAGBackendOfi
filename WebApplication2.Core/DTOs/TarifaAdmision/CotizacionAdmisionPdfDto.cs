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
        public decimal TotalOriginal { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalFinal { get; set; }
        public string? NombreEmpresa { get; set; }
    }

    public class CotizacionConceptoDto
    {
        public string Nombre { get; set; } = "";
        public string Valor { get; set; } = "N/A";
        public decimal Monto { get; set; }
        public string? NombrePromocion { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal MontoFinal { get; set; }
        public bool Incluido { get; set; } = true;
    }

    public class CotizacionAdmisionRequestDto
    {
        public List<CotizacionConceptoPromocionDto> Conceptos { get; set; } = new();
        public int? IdEmpresa { get; set; }
    }

    public class CotizacionConceptoPromocionDto
    {
        public int IdConceptoPago { get; set; }
        public int? IdPromocion { get; set; }
    }
}
