namespace WebApplication2.Core.Requests.Grupo
{
    public class AgregarMateriaGrupoRequest
    {
        public int IdMateriaPlan { get; set; }
        public int? IdProfesor { get; set; }
        public string? Aula { get; set; }
        public short? Cupo { get; set; }
    }
}
