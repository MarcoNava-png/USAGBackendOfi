using System.Collections.Generic;

namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class ActualizarTarifaAdmisionDto
    {
        public string Nombre { get; set; } = null!;
        public bool AplicaConvenioMensualidad { get; set; }
        public bool EsConvenioEmpresarial { get; set; }
        public bool Activo { get; set; }
        public List<CrearTarifaAdmisionDetalleDto> Detalles { get; set; } = new();
    }
}
