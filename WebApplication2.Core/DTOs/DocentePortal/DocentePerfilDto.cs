namespace WebApplication2.Core.DTOs.DocentePortal;

public class DocentePerfilDto
{
    public int IdProfesor { get; set; }
    public string NoEmpleado { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string? Nombre { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? EmailInstitucional { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Curp { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? CampusNombre { get; set; }
    public int TotalGrupos { get; set; }
    public int TotalEstudiantes { get; set; }
}
