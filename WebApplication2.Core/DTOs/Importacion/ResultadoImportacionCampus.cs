namespace WebApplication2.Core.DTOs.Importacion
{
    public class ResultadoImportacionCampus
    {
        public int Fila { get; set; }
        public string ClaveCampus { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? IdCampus { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }
}
