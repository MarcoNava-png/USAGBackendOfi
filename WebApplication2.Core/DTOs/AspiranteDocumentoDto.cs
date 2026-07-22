using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class AspiranteDocumentoDto
    {
        public long IdAspiranteDocumento { get; set; }
        public int IdDocumentoRequisito { get; set; }
        public string Clave { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public EstatusDocumentoEnum Estatus { get; set; }
        public bool Entregado { get; set; }
        public string? UrlArchivo { get; set; }
        public string? Notas { get; set; }
        public DateTime? FechaProrroga { get; set; }
        public string? MotivoProrroga { get; set; }
    }
}
