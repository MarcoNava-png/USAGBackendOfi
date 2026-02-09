namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class EstudiantePreviewDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool EsElegible { get; set; }
        public string MotivoNoElegible { get; set; } = string.Empty;
        public bool TienePagosPendientes { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int RecibosPendientes { get; set; }
        public bool Seleccionado { get; set; } = true;
    }
}
