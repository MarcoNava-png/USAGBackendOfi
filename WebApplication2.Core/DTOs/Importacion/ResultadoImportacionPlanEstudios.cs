namespace WebApplication2.Core.DTOs.Importacion
{
    public class ResultadoImportacionPlanEstudios
    {
        public int Fila { get; set; }
        public string ClavePlanEstudios { get; set; } = string.Empty;
        public string NombrePlanEstudios { get; set; } = string.Empty;
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? IdPlanEstudios { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }
}
