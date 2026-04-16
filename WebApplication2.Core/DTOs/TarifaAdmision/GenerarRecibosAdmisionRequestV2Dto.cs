using System.Collections.Generic;

namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class GenerarRecibosAdmisionRequestV2Dto
    {
        public bool PagoCompleto { get; set; } = false;
        public int? IdEmpresa { get; set; }
        public List<ConceptoConPromocionDto> Conceptos { get; set; } = new();
    }

    public class ConceptoConPromocionDto
    {
        public int IdConceptoPago { get; set; }
        public int? IdPromocion { get; set; }
    }
}
