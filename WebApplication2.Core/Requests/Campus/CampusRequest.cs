using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Campus
{
    public class CampusRequest
    {
        [Required]
        public string ClaveCampus { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Calle { get; set; }

        public string? NumeroExterior { get; set; }

        public string? NumeroInterior { get; set; }

        public int? CodigoPostalId { get; set; }
    }
}
