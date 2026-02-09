using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class ValidarDocumentoRequestDto
    {
        public long IdAspiranteDocumento { get; set; }
        public bool Validar { get; set; } 
        public string? Notas { get; set; }
    }
}

