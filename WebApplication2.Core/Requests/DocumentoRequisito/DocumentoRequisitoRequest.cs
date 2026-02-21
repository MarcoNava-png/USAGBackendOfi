using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.DocumentoRequisito
{
    public class DocumentoRequisitoRequest
    {
        [Required, MaxLength(50)]
        public string Clave { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Descripcion { get; set; } = null!;

        public bool EsObligatorio { get; set; }

        public int Orden { get; set; }
    }
}
