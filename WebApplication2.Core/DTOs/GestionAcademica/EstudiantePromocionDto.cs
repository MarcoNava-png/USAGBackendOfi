namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class EstudiantePromocionDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public bool FuePromovido { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public decimal? PromedioGeneral { get; set; }
        public int MateriasReprobadas { get; set; }
        public bool TienePagosPendientes { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int RecibosPendientes { get; set; }
    }
}
