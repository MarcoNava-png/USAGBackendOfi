using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace WebApplication2.Core.Models
{
    public class MedioPago
    {
        public int IdMedioPago { get; set; }
        public string Clave { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool RequiereReferencia { get; set; } = false;
        public bool Activo { get; set; } = true;

        [JsonPropertyName("nombre")]
        public string Nombre => Descripcion ?? Clave;
    }
}
