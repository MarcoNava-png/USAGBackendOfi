namespace WebApplication2.Core.DTOs.DocentePortal
{
    public class TareaDocenteDto
    {
        public int Id { get; set; }
        public int IdGrupoMateria { get; set; }
        public string NombreMateria { get; set; } = null!;
        public string? CodigoGrupo { get; set; }
        public string Titulo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal PuntosMaximos { get; set; }
        public bool Activa { get; set; }
        public int TotalEntregas { get; set; }
        public int TotalPendientes { get; set; }
    }
}
