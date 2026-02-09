using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class CargarDocumentoRequestDto
    {
        public int IdAspirante { get; set; }
        public int IdDocumentoRequisito { get; set; }
        public string UrlArchivo { get; set; } = null!;
        public string? Notas { get; set; }
    }
}
