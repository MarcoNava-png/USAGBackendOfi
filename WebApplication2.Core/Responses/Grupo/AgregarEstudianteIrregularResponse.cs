namespace WebApplication2.Core.Responses.Grupo
{
    public class AgregarEstudianteIrregularResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? IdEstudiante { get; set; }
        public string? Matricula { get; set; }
        public int? IdEstudianteGrupo { get; set; }
        public int MateriasInscritas { get; set; }

        public bool AccesoCreado { get; set; }
        public string? EmailAcceso { get; set; }
        public string? PasswordTemporal { get; set; }

        public bool CorreoM365Creado { get; set; }
        public string? MensajeM365 { get; set; }
    }
}
