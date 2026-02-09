namespace WebApplication2.Core.DTOs.Importacion
{
    public class ResultadoImportacionMateria
    {
        public int Fila { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public int Cuatrimestre { get; set; }
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? IdMateria { get; set; }
        public int? IdMateriaPlan { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }
}
