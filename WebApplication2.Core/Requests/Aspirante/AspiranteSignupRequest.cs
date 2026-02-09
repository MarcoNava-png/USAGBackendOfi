using System.ComponentModel.DataAnnotations;
using WebApplication2.Core.Requests.Auth;

namespace WebApplication2.Core.Requests.Aspirante
{
    public class AspiranteSignupRequest : PersonaSignupRequest
    {

        public int? CampusId { get; set; }

        [Required]
        public int PlanEstudiosId { get; set; }

        public int AspiranteStatusId { get; set; }

        public int MedioContactoId { get; set; }

        public string? Notas { get; set; }

        public string? AtendidoPorUsuarioId { get; set; }

        public int? HorarioId { get; set; }

        public int? CuatrimestreInteres { get; set; }
    }
}
