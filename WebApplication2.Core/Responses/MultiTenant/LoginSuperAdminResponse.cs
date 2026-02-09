using WebApplication2.Core.DTOs.MultiTenant;

namespace WebApplication2.Core.Responses.MultiTenant
{
    public class LoginSuperAdminResponse
    {
        public bool Exitoso { get; set; }
        public string? Token { get; set; }
        public string? Mensaje { get; set; }
        public SuperAdminInfoDto? SuperAdmin { get; set; }
    }
}
