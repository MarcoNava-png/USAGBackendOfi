using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class BitacoraCreateDto
    {
        public long IdRecibo { get; set; }
        public string Accion { get; set; }
        public string? Origen { get; set; } 
        public string? Notas { get; set; }
    }
}
