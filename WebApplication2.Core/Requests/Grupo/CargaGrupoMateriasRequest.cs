namespace WebApplication2.Core.Requests.Grupo
{
    public class CargaGrupoMateriasRequest
    {
        public int IdGrupo { get; set; }
        public List<GrupoMateriasRequest> GrupoMaterias { get; set; }
    }
}
