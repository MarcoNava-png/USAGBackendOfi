using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Titulacion
{
    public class CertificadoElectronicoListDto
    {
        public int Id { get; set; }
        public string? FolioControl { get; set; }
        public int TipoTitulacion { get; set; }
        public int Estatus { get; set; }
        public string EstatusTexto { get; set; } = null!;
        public string NumeroControl { get; set; } = null!;
        public string? Curp { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string? NombreCarrera { get; set; }
        public string? ClavePlan { get; set; }
        public string? Promedio { get; set; }
        public int TotalAsignaturas { get; set; }
        public int AsignaturasAsignadas { get; set; }
        public string? FolioControlSEP { get; set; }
        public DateTime FechaExpedicion { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CertificadoElectronicoDetalleDto
    {
        public int Id { get; set; }
        public string? FolioControl { get; set; }
        public int TipoTitulacion { get; set; }
        public int Estatus { get; set; }
        public string EstatusTexto { get; set; } = null!;

        public int? IdEstudiante { get; set; }
        public int? IdPersona { get; set; }
        public string NumeroControl { get; set; } = null!;
        public string? Curp { get; set; }
        public string Nombre { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public int IdGenero { get; set; }
        public DateTime FechaNacimiento { get; set; }

        public string IdCarreraSEP { get; set; } = null!;
        public string? ClaveCarrera { get; set; }
        public string? NombreCarrera { get; set; }
        public string IdTipoPeriodo { get; set; } = null!;
        public string? TipoPeriodo { get; set; }
        public string ClavePlan { get; set; } = null!;
        public string NumeroRvoe { get; set; } = null!;
        public DateTime FechaExpedicionRvoe { get; set; }

        public string IdTipoCertificacion { get; set; } = null!;
        public string? TipoCertificacion { get; set; }
        public DateTime FechaExpedicion { get; set; }
        public string IdLugarExpedicion { get; set; } = null!;
        public string? LugarExpedicion { get; set; }

        public int TotalAsignaturas { get; set; }
        public int AsignaturasAsignadas { get; set; }
        public string? Promedio { get; set; }

        public int IdConfiguracionIPES { get; set; }
        public string? XmlGenerado { get; set; }
        public string? CadenaOriginal { get; set; }
        public string? SelloDigital { get; set; }
        public int? NumeroLoteSEP { get; set; }
        public string? FolioControlSEP { get; set; }
        public string? MensajeSEP { get; set; }
        public DateTime? FechaEnvioSEP { get; set; }
        public DateTime? FechaRespuestaSEP { get; set; }

        public List<CertificadoAsignaturaDto> Asignaturas { get; set; } = new();
    }

    public class CertificadoAsignaturaDto
    {
        public int Id { get; set; }
        public int IdAsignatura { get; set; }
        public string? ClaveAsignatura { get; set; }
        public string Nombre { get; set; } = null!;
        public string Ciclo { get; set; } = null!;
        public string Calificacion { get; set; } = null!;
        public int? IdObservaciones { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CrearCertificadoDirectoRequest
    {
        public string NumeroControl { get; set; } = null!;
        public string? Curp { get; set; }
        public string Nombre { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public int IdGenero { get; set; }
        public string FechaNacimiento { get; set; } = null!;

        public string IdCarreraSEP { get; set; } = null!;
        public string? ClaveCarrera { get; set; }
        public string? NombreCarrera { get; set; }
        public string IdTipoPeriodo { get; set; } = null!;
        public string? TipoPeriodo { get; set; }
        public string ClavePlan { get; set; } = null!;
        public string NumeroRvoe { get; set; } = null!;
        public string FechaExpedicionRvoe { get; set; } = null!;

        public string IdTipoCertificacion { get; set; } = null!;
        public string? TipoCertificacion { get; set; }
        public string FechaExpedicion { get; set; } = null!;
        public string IdLugarExpedicion { get; set; } = null!;
        public string? LugarExpedicion { get; set; }

        public int IdConfiguracionIPES { get; set; }

        public List<AsignaturaRequest> Asignaturas { get; set; } = new();
    }

    public class AsignaturaRequest
    {
        public int IdAsignatura { get; set; }
        public string? ClaveAsignatura { get; set; }
        public string Nombre { get; set; } = null!;
        public string Ciclo { get; set; } = null!;
        public string Calificacion { get; set; } = null!;
        public decimal? Creditos { get; set; }
        public int? IdTipoAsignatura { get; set; }
        public string? TipoAsignatura { get; set; }
        public int? IdObservaciones { get; set; }
        public string? Observaciones { get; set; }
    }
}
