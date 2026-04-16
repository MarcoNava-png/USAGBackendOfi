using System.Collections.Generic;

namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class TarifaAdmisionDto
    {
        public int IdTarifaAdmision { get; set; }
        public int IdPlanEstudios { get; set; }
        public string NombrePlanEstudios { get; set; } = null!;
        public string ClavePlanEstudios { get; set; } = null!;
        public string? NombreCampus { get; set; }
        public string Nombre { get; set; } = null!;
        public bool AplicaConvenioMensualidad { get; set; }
        public bool EsConvenioEmpresarial { get; set; }
        public bool Activo { get; set; }
        public List<TarifaAdmisionDetalleDto> Detalles { get; set; } = new();
    }
}
