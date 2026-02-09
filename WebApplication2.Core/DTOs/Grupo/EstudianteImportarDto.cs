namespace WebApplication2.Core.DTOs.Grupo
{
    public class EstudianteImportarDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string? Curp { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? FechaNacimiento { get; set; }
        public int? IdGenero { get; set; }
        public string? Matricula { get; set; }

        public DateOnly? GetFechaNacimientoAsDateOnly()
        {
            if (string.IsNullOrWhiteSpace(FechaNacimiento))
                return null;

            if (DateOnly.TryParse(FechaNacimiento, out var fecha))
                return fecha;

            if (DateTime.TryParse(FechaNacimiento, out var dateTime))
                return DateOnly.FromDateTime(dateTime);

            return null;
        }
    }
}
