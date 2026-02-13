namespace WebApplication2.Core.DTOs.Importacion
{
    public class ResultadoImportacionProfesor
    {
        public int Fila { get; set; }
        public string NoEmpleado { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? IdProfesor { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }
}
