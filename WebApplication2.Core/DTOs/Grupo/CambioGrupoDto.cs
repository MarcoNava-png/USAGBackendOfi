namespace WebApplication2.Core.DTOs.Grupo
{
    public class CambioGrupoRequestDto
    {
        public int IdEstudianteGrupo { get; set; }
        public int IdGrupoDestino { get; set; }
    }

    public class CambioGrupoResultDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string GrupoOrigen { get; set; } = string.Empty;
        public string GrupoDestino { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
    }
}
