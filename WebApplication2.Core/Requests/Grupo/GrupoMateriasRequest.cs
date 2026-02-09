namespace WebApplication2.Core.Requests.Grupo
{
    public class GrupoMateriasRequest
    {
        public int IdMateriaPlan { get; set; }
        public int IdProfesor { get; set; }
        public string Aula { get; set; }
        public int Cupo { get; set; }
    }
}
