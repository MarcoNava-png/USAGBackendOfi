using WebApplication2.Core.DTOs.Grupo;

namespace WebApplication2.Core.Requests.Grupo
{
    public class ActualizarHorariosRequest
    {
        public List<HorarioDto> HorarioJson { get; set; } = new();
    }
}
