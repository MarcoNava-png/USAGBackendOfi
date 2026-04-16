namespace WebApplication2.Core.DTOs.Empresa
{
    public class EmpresaDto
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
        public int CantidadAspirantes { get; set; }
    }

    public class CrearEmpresaDto
    {
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; } = true;
    }

    public class ActualizarEmpresaDto
    {
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}
