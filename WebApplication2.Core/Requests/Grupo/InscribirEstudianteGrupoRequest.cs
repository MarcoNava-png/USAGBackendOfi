namespace WebApplication2.Core.Requests.Grupo
{
    public class InscribirEstudianteGrupoRequest
    {
        public int IdEstudiante { get; set; }
        public bool ForzarInscripcion { get; set; } = false;
        public string? Observaciones { get; set; }
    }
}
