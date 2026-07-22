namespace WebApplication2.Core.DTOs.Formatos
{
    public class FormatoDatosResueltos
    {
        public Dictionary<string, string> Variables { get; set; } = new();
        public Dictionary<string, List<Dictionary<string, string>>> Tablas { get; set; } = new();
    }
}
