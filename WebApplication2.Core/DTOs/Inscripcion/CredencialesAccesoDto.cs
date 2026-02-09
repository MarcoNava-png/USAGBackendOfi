namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class CredencialesAccesoDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string PasswordTemporal { get; set; } = string.Empty;
        public string UrlAcceso { get; set; } = string.Empty;
        public string Mensaje { get; set; } = "Credenciales generadas exitosamente. El estudiante debe cambiar su contraseña en el primer acceso.";
    }
}
