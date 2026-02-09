using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class CalificacionParcialEstadoRequest
    {
        public int GrupoMateriaId { get; set; }
        public int Id { get; set; }
        public StatusParcialEnum StatusParcial { get; set; }
    }
}
