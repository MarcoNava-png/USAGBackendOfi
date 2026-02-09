namespace WebApplication2.Core.DTOs.Grupo
{
    public class EstudianteImportadoResultDto
    {
        public int Fila { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Curp { get; set; }
        public string? Correo { get; set; }
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
        public int? IdPersona { get; set; }
        public int? IdEstudiante { get; set; }
        public string? MatriculaGenerada { get; set; }
        public int? IdEstudianteGrupo { get; set; }
    }
}
