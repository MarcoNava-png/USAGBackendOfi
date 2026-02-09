namespace WebApplication2.Core.Requests.Grupo
{
    public class InscribirEstudiantesMasivoRequest
    {
        public List<int> IdsEstudiantes { get; set; } = new();
        public string? Observaciones { get; set; }
    }
}
