namespace WebApplication2.Core.DTOs.Diagnostico
{
    public class InconsistenciaInscripcionDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int? IdGrupo { get; set; }
        public string? CodigoGrupo { get; set; }
        public int? IdGrupoSobrante { get; set; }
        public string? CodigoGrupoSobrante { get; set; }
    }

    public class RepararInscripcionRequest
    {
        public int IdEstudiante { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? IdGrupo { get; set; }
        public int? IdGrupoSobrante { get; set; }
    }

    public class RepararInscripcionResultDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
