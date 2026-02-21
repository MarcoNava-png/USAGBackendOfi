using System;
using System.Collections.Generic;

namespace WebApplication2.Core.Models
{
    public class PlantillaCobro : BaseEntity
    {
        public int IdPlantillaCobro { get; set; }

        public string NombrePlantilla { get; set; } = null!;

        public int IdPlanEstudios { get; set; }

        public int NumeroCuatrimestre { get; set; }

        public int? IdPeriodoAcademico { get; set; }

        public int? IdTurno { get; set; }

        public int? IdModalidad { get; set; }

        public int Version { get; set; } = 1;

        public bool EsActiva { get; set; } = true;

        public DateTime FechaVigenciaInicio { get; set; }

        public DateTime? FechaVigenciaFin { get; set; }

        public int EstrategiaEmision { get; set; }

        public int NumeroRecibos { get; set; }

        public int DiaVencimiento { get; set; }

        public string CreadoPor { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }

        public string? ModificadoPor { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public virtual PlanEstudios? IdPlanEstudiosNavigation { get; set; }
        public virtual Modalidad? IdModalidadNavigation { get; set; }
        public virtual ICollection<PlantillaCobroDetalle> Detalles { get; set; } = new List<PlantillaCobroDetalle>();
    }
}
