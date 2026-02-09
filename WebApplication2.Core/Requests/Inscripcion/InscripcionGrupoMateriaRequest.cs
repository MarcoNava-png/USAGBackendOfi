namespace WebApplication2.Core.Requests.Inscripcion
{
    public class InscripcionGrupoMateriaRequest
    {
        public int IdEstudiante { get; set; }
        public int IdGrupoMateria { get; set; }
        public DateTime? FechaInscripcion { get; set; }
    }
}
