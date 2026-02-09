namespace WebApplication2.Core.Requests.Grupo
{
    public class InscribirEstudiantesGrupoRequest
    {
        public int IdGrupo { get; set; }
        public List<int> IdsEstudiantes { get; set; } = new();
        public string? Observaciones { get; set; }
    }
}
