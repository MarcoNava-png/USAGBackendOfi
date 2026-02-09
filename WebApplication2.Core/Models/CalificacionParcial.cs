using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class CalificacionParcial : BaseEntity
    {
        public int Id { get; set; }
        public GrupoMateria GrupoMateria { get; set; }
        public int GrupoMateriaId { get; set; }
        public Parciales Parcial { get; set; }
        public int ParcialId { get;set; }
        public Inscripcion Inscripcion { get; set; }
        public int InscripcionId { get; set; }
        public Profesor Profesor { get; set; }  
        public int ProfesorId { get; set;}
        public StatusParcialEnum StatusParcial { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }
    }
}
