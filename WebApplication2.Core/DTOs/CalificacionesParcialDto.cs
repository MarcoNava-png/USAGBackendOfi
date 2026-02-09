using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class CalificacionesParcialDto
    {
        public int GrupoMateriaId { get; set; }
        public int ParcialId { get; set; }
        public string ParcialName { get; set; }
        public int ProfesorId { get; set; }
        public string StatusParcialName { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }
    }
}
