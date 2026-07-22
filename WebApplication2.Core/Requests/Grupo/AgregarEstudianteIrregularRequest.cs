using WebApplication2.Core.DTOs.Grupo;

namespace WebApplication2.Core.Requests.Grupo
{
    public class AgregarEstudianteIrregularRequest
    {
        public int IdGrupo { get; set; }
        public EstudianteImportarDto Datos { get; set; } = new();
        public string? Observaciones { get; set; }

        public bool CrearAcceso { get; set; } = false;
        public bool CrearCorreoM365 { get; set; } = false;
        public string? EmailInstitucional { get; set; }
        public string? UsuarioCorreo { get; set; }
        public string? Dominio { get; set; }
        public string? PasswordPersonalizada { get; set; }
    }
}
