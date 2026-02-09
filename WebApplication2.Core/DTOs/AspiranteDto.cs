namespace WebApplication2.Core.DTOs
{
    public class AspiranteDto
    {
        public int IdAspirante { get; set; }

        public int PersonaId { get; set; }

        public string NombreCompleto { get; set; }

        public string Email { get; set; }

        public string Telefono { get; set; }

        public string AspiranteEstatus { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string PlanEstudios { get; set; }

        public int IdDireccion { get; set; }

        public int CodigoPostalId { get; set; }

        public int MunicipioId { get; set; }

        public int EstadoId { get; set; }

        public string UsuarioAtiendeNombre { get; set; }

        public string IdAtendidoPorUsuario { get; set; }

        public string CreatedBy { get; set; }

        public string UsuarioRegistroNombre { get; set; }

        public string? EstatusPago { get; set; }

        public string? EstatusDocumentos { get; set; }

        public int? CuatrimestreInteres { get; set; }
    }
}
