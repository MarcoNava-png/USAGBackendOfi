using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class BitacoraCreateRequest
    {
        public long IdRecibo { get; set; }
        public string Accion { get; set; }                           
        public string? Origen { get; set; }                          
        public string Usuario { get; set; }                          
        public string? Notas { get; set; }
    }
}
