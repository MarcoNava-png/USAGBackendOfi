using WebApplication2.Core.Models;

namespace WebApplication2.Core.Requests.Grupo
{
    public class GrupoRequest
    {

        public int IdPlanEstudios { get; set; }

        public int IdPeriodoAcademico { get; set; }

        public byte NumeroCuatrimestre { get; set; }

        public byte NumeroGrupo { get; set; }

        public int IdTurno { get; set; }

        public short CapacidadMaxima { get; set; }
    }
}
