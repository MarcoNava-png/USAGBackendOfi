namespace WebApplication2.Core.Requests.Inscripcion
{
    public class InscripcionRequest
    {
        public int IdEstudiante { get; set; }

        public string NombreGrupoMateria { get; set; }

        public DateTime FechaInscripcion { get; set; }

        public string Estado { get; set; } = null!;
    }
}
