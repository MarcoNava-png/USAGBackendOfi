using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class DocumentoRequisitoDto
    {
        public int IdDocumentoRequisito { get; set; }
        public string Clave { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public bool EsObligatorio { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }
}
