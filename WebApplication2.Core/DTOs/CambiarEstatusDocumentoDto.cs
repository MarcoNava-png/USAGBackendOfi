using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class CambiarEstatusDocumentoDto
    {
        public EstatusDocumentoEnum Estatus { get; set; }
        public string? Notas { get; set; }
    }
}
