namespace WebApplication2.Core.DTOs.Titulacion
{
    public class ConfiguracionIPESDto
    {
        public int Id { get; set; }
        public string IdNombreInstitucion { get; set; } = null!;
        public string? NombreInstitucion { get; set; }
        public string IdCampusSEP { get; set; } = null!;
        public string? CampusSEP { get; set; }
        public string IdEntidadFederativa { get; set; } = null!;
        public string? EntidadFederativa { get; set; }
        public int IdCampus { get; set; }
        public string? NombreCampus { get; set; }
        public bool Activa { get; set; }
        public ResponsableFirmaDto? ResponsableActivo { get; set; }
        public CredencialSEPDto? CredencialActiva { get; set; }
    }

    public class ResponsableFirmaDto
    {
        public int Id { get; set; }
        public string Curp { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string IdCargo { get; set; } = null!;
        public string? Cargo { get; set; }
        public bool TieneCertificadoCer { get; set; }
        public bool TieneLlaveKey { get; set; }
        public string? NoCertificadoResponsable { get; set; }
        public bool Activo { get; set; }
        public DateTime? VigenciaInicio { get; set; }
        public DateTime? VigenciaFin { get; set; }
    }

    public class CredencialSEPDto
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = null!;
        public bool TienePassword { get; set; }
        public string? EndpointUrl { get; set; }
        public bool EsProduccion { get; set; }
        public bool Activa { get; set; }
    }

    public class GuardarConfiguracionIPESRequest
    {
        public int? Id { get; set; }
        public string IdNombreInstitucion { get; set; } = null!;
        public string? NombreInstitucion { get; set; }
        public string IdCampusSEP { get; set; } = null!;
        public string? CampusSEP { get; set; }
        public string IdEntidadFederativa { get; set; } = null!;
        public string? EntidadFederativa { get; set; }
        public int IdCampus { get; set; }
    }

    public class GuardarResponsableFirmaRequest
    {
        public int? Id { get; set; }
        public int IdConfiguracionIPES { get; set; }
        public string Curp { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string IdCargo { get; set; } = null!;
        public string? Cargo { get; set; }
        public string? PasswordLlavePrivada { get; set; }
    }

    public class GuardarCredencialSEPRequest
    {
        public int? Id { get; set; }
        public int IdConfiguracionIPES { get; set; }
        public string Usuario { get; set; } = null!;
        public string? Password { get; set; }
        public string? EndpointUrl { get; set; }
        public bool EsProduccion { get; set; }
    }
}
