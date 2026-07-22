namespace WebApplication2.Core.Models
{
    public class VentanaCaptura : BaseEntity
    {
        public int IdVentanaCaptura { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int NumeroParcial { get; set; }
        public bool Abierta { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaLimite { get; set; }

        public virtual PeriodoAcademico? IdPeriodoAcademicoNavigation { get; set; }
    }
}
