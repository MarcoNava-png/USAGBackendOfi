namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class MateriaDetalleDto
    {
        public int IdInscripcion { get; set; }
        public int IdMateria { get; set; }
        public string ClaveMateria { get; set; } = string.Empty;
        public string NombreMateria { get; set; } = string.Empty;
        public decimal Creditos { get; set; }
        public string Grupo { get; set; } = string.Empty;
        public string? Profesor { get; set; }
        public CalificacionesParcialesDto Parciales { get; set; } = new();
        public decimal? CalificacionFinal { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public DateTime FechaInscripcion { get; set; }
        public string? Observaciones { get; set; }
    }
}
