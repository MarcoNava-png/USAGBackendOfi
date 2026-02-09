using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Auth
{

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Email { get; set; } = null!;
    }

    public class AdminResetPasswordRequest
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(12, ErrorMessage = "La contraseña debe tener al menos 12 caracteres")]
        public string NewPassword { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
