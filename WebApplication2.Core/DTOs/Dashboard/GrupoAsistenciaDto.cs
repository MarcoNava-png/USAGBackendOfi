namespace WebApplication2.Core.DTOs.Dashboard
{
    public class GrupoAsistenciaDto
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public decimal PorcentajeAsistencia { get; set; }
        public int EstudiantesEnRiesgo { get; set; }
    }
}
