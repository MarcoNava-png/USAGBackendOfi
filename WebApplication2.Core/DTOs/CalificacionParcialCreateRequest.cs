using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class CalificacionParcialCreateRequest
    {
        public int GrupoMateriaId { get; set; }
        public int ParcialId { get; set; }
        public int InscripcionId { get; set; }
        public int ProfesorId { get; set; }
        public DateTime? FechaApertura { get; set; }
    }
}
