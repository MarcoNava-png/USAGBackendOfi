using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class LigaPago : BaseEntity
    {
        public long IdLigaPago { get; set; }
        public string Token { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string TipoRecibo { get; set; } = null!;
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public DateTime FechaGeneracionUtc { get; set; } = DateTime.UtcNow;
        public DateTime? FechaPrimeraVistaUtc { get; set; }
        public string? IPPrimeraVista { get; set; }

        public Recibo Recibo { get; set; } = null!;
    }
}
