namespace WebApplication2.Core.DTOs.Dashboard
{
    public class GrupoDocenteDto
    {
        public int IdGrupoMateria { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public decimal PromedioGrupo { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
        public bool TieneCalificacionesPendientes { get; set; }
    }
}
