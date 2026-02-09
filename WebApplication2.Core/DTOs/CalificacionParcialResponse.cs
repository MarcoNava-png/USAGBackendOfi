using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class CalificacionParcialResponse
    {
        public int Id { get; set; }
        public string NombreParcial { get; set; }
        public int GrupoMateriaId { get; set; }
        public string NombreGrupo { get; set; }
        public int ParcialId { get; set; }
        public string NombreProfesor { get; set; }
        public int ProfesorId { get; set; }
        public string StatusParcial { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
    }
}
