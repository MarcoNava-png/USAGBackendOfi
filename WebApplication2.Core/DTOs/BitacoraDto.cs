using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class BitacoraDto
    {
        public long IdBitacora { get; set; }
        public long IdRecibo { get; set; }
        public string? TipoRecibo { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaUtc { get; set; }
        public string Accion { get; set; }
        public string? Origen { get; set; }
        public string? Notas { get; set; }
    }
}
