namespace WebApplication2.Core.DTOs.VentanaCaptura
{
    public class VentanaCapturaDto
    {
        public int IdVentanaCaptura { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int NumeroParcial { get; set; }
        public bool Abierta { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaLimite { get; set; }
        public bool Vigente { get; set; }
    }

    public class AbrirVentanaRequest
    {
        public int IdPeriodoAcademico { get; set; }
        public int NumeroParcial { get; set; }
        public DateTime? FechaLimite { get; set; }
    }

    public class CerrarVentanaRequest
    {
        public int IdPeriodoAcademico { get; set; }
        public int NumeroParcial { get; set; }
    }

    public class EstadoCapturaDto
    {
        public int NumeroParcial { get; set; }
        public bool PuedeCapturar { get; set; }
        public bool VentanaAbierta { get; set; }
        public DateTime? FechaLimite { get; set; }
        public bool TieneProrrogaAprobada { get; set; }
        public DateTime? FechaLimiteProrroga { get; set; }
        public string? ProrrogaPendiente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class SolicitarProrrogaRequest
    {
        public int IdGrupoMateria { get; set; }
        public int NumeroParcial { get; set; }
        public string? Motivo { get; set; }
    }

    public class ResolverProrrogaRequest
    {
        public bool Aprobar { get; set; }
        public DateTime? FechaLimiteProrroga { get; set; }
        public string? Nota { get; set; }
    }

    public class SolicitudProrrogaDto
    {
        public int IdSolicitudProrroga { get; set; }
        public int IdProfesor { get; set; }
        public string Profesor { get; set; } = string.Empty;
        public int IdGrupoMateria { get; set; }
        public string Grupo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public int NumeroParcial { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaLimiteProrroga { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? NotaResolucion { get; set; }
    }

    public class AvanceCapturaItemDto
    {
        public int IdGrupoMateria { get; set; }
        public string Grupo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public int? IdProfesor { get; set; }
        public string Profesor { get; set; } = "Sin asignar";
        public int? IdCampus { get; set; }
        public string? Campus { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime? UltimaActualizacion { get; set; }
    }

    public class AvanceCapturaDto
    {
        public int IdPeriodoAcademico { get; set; }
        public int NumeroParcial { get; set; }
        public int Total { get; set; }
        public int Capturados { get; set; }
        public int Pendientes { get; set; }
        public List<AvanceCapturaItemDto> Items { get; set; } = new();
    }
}
