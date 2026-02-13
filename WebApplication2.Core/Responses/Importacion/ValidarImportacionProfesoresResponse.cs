namespace WebApplication2.Core.Responses.Importacion
{
    public class ValidarImportacionProfesoresResponse
    {
        public int TotalRegistros { get; set; }
        public int RegistrosValidos { get; set; }
        public int RegistrosConErrores { get; set; }
        public bool EsValido { get; set; }
        public List<DetalleValidacionProfesor> DetalleValidacion { get; set; } = new();

        public class DetalleValidacionProfesor
        {
            public int Fila { get; set; }
            public string NoEmpleado { get; set; } = string.Empty;
            public string NombreCompleto { get; set; } = string.Empty;
            public bool Exito { get; set; }
            public string Mensaje { get; set; } = string.Empty;
            public List<string> Advertencias { get; set; } = new();
        }
    }
}
