namespace WebApplication2.Core.DTOs.DocentePortal
{
    public class PlaneacionDocenteDto
    {
        public int Id { get; set; }
        public int IdGrupoMateria { get; set; }
        public string NombreArchivo { get; set; } = null!;
        public string UrlArchivo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? TipoArchivo { get; set; }
        public long TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}
