using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class DocumentoRequisito : BaseEntity
    {
        public int IdDocumentoRequisito { get; set; }
        [Required, MaxLength(50)]
        public string Clave { get; set; } = null!;
        [Required, MaxLength(200)]
        public string Descripcion { get; set; } = null!;
        public bool EsObligatorio { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<AspiranteDocumento> AspiranteDocumentos { get; set; } = new List<AspiranteDocumento>();

        public ICollection<PlanDocumentoRequisito> PlanDocumentosRequisito { get; set; } = new List<PlanDocumentoRequisito>();
    }
}

