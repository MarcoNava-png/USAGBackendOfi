namespace WebApplication2.Core.DTOs.Dashboard
{
    public class CarteraVencidaDto
    {
        public decimal TotalVencida { get; set; }
        public int TotalAlumnos { get; set; }
        public decimal RecaudadoMesTotal { get; set; }
        public List<CarteraCampusDto> PorCampus { get; set; } = new();
        public List<CarteraAntiguedadDto> PorAntiguedad { get; set; } = new();
        public List<CarteraConceptoDto> PorConcepto { get; set; } = new();
        public List<CarteraCampusDto> RecaudadoMesPorCampus { get; set; } = new();
    }

    public class CarteraCampusDto
    {
        public string Campus { get; set; } = "";
        public decimal Deuda { get; set; }
        public int Alumnos { get; set; }
    }

    public class CarteraAntiguedadDto
    {
        public string Rango { get; set; } = "";
        public decimal Deuda { get; set; }
        public int Recibos { get; set; }
    }

    public class CarteraConceptoDto
    {
        public string Concepto { get; set; } = "";
        public decimal Importe { get; set; }
    }
}
