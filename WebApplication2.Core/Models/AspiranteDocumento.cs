using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class AspiranteDocumento : BaseEntity
    {
        public long IdAspiranteDocumento { get; set; }
        public int IdAspirante { get; set; }
        public int IdDocumentoRequisito { get; set; }
        public EstatusDocumentoEnum Estatus { get; set; } = EstatusDocumentoEnum.PENDIENTE;
        public DateTime? FechaSubidoUtc { get; set; }
        [MaxLength(500)]
        public string? UrlArchivo { get; set; }
        [MaxLength(500)]
        public string? Notas { get; set; }

        // Validación
        public DateTime? FechaValidacion { get; set; }
        [MaxLength(450)]
        public string? UsuarioValidacion { get; set; }

        // Prórroga
        public DateTime? FechaProrroga { get; set; }
        [MaxLength(500)]
        public string? MotivoProrroga { get; set; }
        [MaxLength(450)]
        public string? UsuarioProrroga { get; set; }
        public DateTime? FechaProrrogaAsignada { get; set; }

        public Aspirante? Aspirante { get; set; }
        public DocumentoRequisito? Requisito { get; set; }
    }
}
