using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Grupo
{
    public class GrupoUpdateRequest : GrupoRequest
    {
        public int IdGrupo { get; set; }
        public StatusEnum Status { get; set; }
    }
}
