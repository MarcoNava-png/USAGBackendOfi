namespace WebApplication2.Core.DTOs.Dashboard
{
    public class DocentePendienteDto
    {
        public int IdProfesor { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int CalificacionesPendientes { get; set; }
        public int AsistenciasPendientes { get; set; }
    }
}
