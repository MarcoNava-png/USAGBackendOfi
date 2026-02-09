namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class SuperAdminInfoDto
    {
        public int IdSuperAdmin { get; set; }
        public string Email { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public bool AccesoTotal { get; set; }
        public List<int> TenantsAcceso { get; set; } = new();
    }
}
