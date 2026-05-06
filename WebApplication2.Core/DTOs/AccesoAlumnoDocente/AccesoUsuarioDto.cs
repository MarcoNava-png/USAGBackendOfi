namespace WebApplication2.Core.DTOs.AccesoAlumnoDocente;

public class AccesoUsuarioDto
{
    public string? UserId { get; set; }
    public int? IdEstudiante { get; set; }
    public int? IdProfesor { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Matricula { get; set; }
    public string? PlanEstudios { get; set; }
    public string? GrupoActual { get; set; }
    public int? Cuatrimestre { get; set; }
    public bool TieneCuenta { get; set; }
    public bool CuentaBloqueada { get; set; }
    public DateTime? BloqueadaHasta { get; set; }
    public int IntentosFallidos { get; set; }
    public bool DebeCambiarPassword { get; set; }
    public bool NuncaHaIngresado { get; set; }
    public bool Activo { get; set; }
}

public class AccesosListaDto
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public List<AccesoUsuarioDto> Items { get; set; } = new();
}

public class ResetearPasswordRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? NuevaPassword { get; set; }
    public bool ForzarCambio { get; set; } = true;
}

public class ResetearPasswordResponse
{
    public bool Exito { get; set; }
    public string? PasswordTemporal { get; set; }
    public string? Mensaje { get; set; }
}

public class CrearAccesoRequest
{
    public string Tipo { get; set; } = "alumno";
    public int EntidadId { get; set; }
    public string? EmailPersonalizado { get; set; }
    public string? PasswordPersonalizada { get; set; }
}
