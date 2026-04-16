using System.Collections.Generic;

namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class CrearTarifaAdmisionDto
    {
        public int IdPlanEstudios { get; set; }
        public string Nombre { get; set; } = null!;
        public bool AplicaConvenioMensualidad { get; set; } = false;
        public bool EsConvenioEmpresarial { get; set; } = false;
        public bool Activo { get; set; } = true;
        public List<CrearTarifaAdmisionDetalleDto> Detalles { get; set; } = new();
    }

    public class CrearTarifaAdmisionDetalleDto
    {
        public int IdConceptoPago { get; set; }
        public decimal Monto { get; set; }
        public bool EsAplicable { get; set; } = true;
        public string? Notas { get; set; }
        public int Orden { get; set; }
    }
}
