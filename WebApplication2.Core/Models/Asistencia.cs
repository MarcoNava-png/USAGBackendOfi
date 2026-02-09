using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class Asistencia : BaseEntity
    {
        public int IdAsistencia { get; set; }
        public int InscripcionId { get; set; }
        public virtual Inscripcion Inscripcion { get; set; }
        public int GrupoMateriaId { get; set; }
        public virtual GrupoMateria GrupoMateria { get; set; }
        public DateTime FechaSesion { get; set; }
        public EstadoAsistenciaEnum EstadoAsistencia { get; set; }
        public string? Observaciones { get; set; }
        public int ProfesorRegistroId { get; set; }
        public virtual Profesor ProfesorRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
