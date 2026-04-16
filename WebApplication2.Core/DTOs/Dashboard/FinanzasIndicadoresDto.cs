namespace WebApplication2.Core.DTOs.Dashboard
{
    public class FinanzasIndicadoresDto
    {
        public List<IngresoDiarioDto> IngresosPorDia { get; set; } = new();
        public List<IngresoMensualDto> IngresosMensuales { get; set; } = new();
        public List<DistribucionRecibosDto> DistribucionRecibos { get; set; } = new();
        public List<MorosidadRangoDto> MorosidadPorRango { get; set; } = new();
        public List<IngresoMetodoPagoDto> IngresosPorMetodoPago { get; set; } = new();
    }

    public class IngresoDiarioDto
    {
        public string Dia { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Transacciones { get; set; }
    }

    public class IngresoMensualDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Transacciones { get; set; }
    }

    public class DistribucionRecibosDto
    {
        public string Estatus { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
    }

    public class MorosidadRangoDto
    {
        public string Rango { get; set; } = string.Empty;
        public int Estudiantes { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class IngresoMetodoPagoDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Transacciones { get; set; }
    }
}
