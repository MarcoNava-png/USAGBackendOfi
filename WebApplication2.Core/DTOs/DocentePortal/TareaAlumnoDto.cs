namespace WebApplication2.Core.DTOs.DocentePortal
{
    public class TareaAlumnoDto
    {
        public int Id { get; set; }
        public string NombreMateria { get; set; } = null!;
        public string? CodigoGrupo { get; set; }
        public string Titulo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal PuntosMaximos { get; set; }
        public bool Entregada { get; set; }
        public decimal? Calificacion { get; set; }
        public string? Retroalimentacion { get; set; }
        public string? NombreProfesor { get; set; }
    }
}
