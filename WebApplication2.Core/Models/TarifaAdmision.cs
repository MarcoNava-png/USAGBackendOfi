using System.Collections.Generic;

namespace WebApplication2.Core.Models
{
    public class TarifaAdmision : BaseEntity
    {
        public int IdTarifaAdmision { get; set; }
        public int IdPlanEstudios { get; set; }
        public string Nombre { get; set; } = null!;
        public bool AplicaConvenioMensualidad { get; set; } = false;
        public bool EsConvenioEmpresarial { get; set; } = false;
        public bool Activo { get; set; } = true;

        public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
        public virtual ICollection<TarifaAdmisionDetalle> Detalles { get; set; } = new List<TarifaAdmisionDetalle>();
    }
}
