namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class InscripcionMateriaDto
    {
        public long? IdInscripcion { get; set; }
        public int IdGrupoMateria { get; set; }
        public string NombreMateria { get; set; } = string.Empty;
        public string? Profesor { get; set; }
        public string? Aula { get; set; }
        public short CupoMaximo { get; set; }
        public int EstudiantesInscritos { get; set; }
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }
}
