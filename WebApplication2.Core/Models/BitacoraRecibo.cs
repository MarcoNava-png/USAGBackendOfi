using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class BitacoraRecibo
    {
        [Key]
        public long IdBitacora { get; set; }
        public long IdRecibo { get; set; }
        public string? TipoRecibo { get; set; }
        public string Usuario { get; set; } = null!;
        public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
        public string Accion { get; set; } = null!;
        public string? Origen { get; set; }
        public string? Notas { get; set; }

        public Recibo Recibo { get; set; } = null!;
    }
}
