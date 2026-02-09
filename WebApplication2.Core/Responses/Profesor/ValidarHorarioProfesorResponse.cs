using WebApplication2.Core.DTOs.Profesor;

namespace WebApplication2.Core.Responses.Profesor
{
    public class ValidarHorarioProfesorResponse
    {
        public bool TieneConflicto { get; set; }
        public List<ConflictoHorario> Conflictos { get; set; } = new();
    }
}
