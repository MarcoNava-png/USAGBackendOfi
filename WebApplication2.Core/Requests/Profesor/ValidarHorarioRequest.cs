using WebApplication2.Core.DTOs.Profesor;

namespace WebApplication2.Core.Requests.Profesor
{
    public class ValidarHorarioRequest
    {
        public List<HorarioValidacionDto> HorarioJson { get; set; } = new();
        public int? IdGrupoMateriaActual { get; set; }
    }
}
