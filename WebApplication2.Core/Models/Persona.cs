namespace WebApplication2.Core.Models;

public partial class Persona : BaseEntity
{
    public int IdPersona { get; set; }

    public string? Nombre { get; set; }

    public string? ApellidoPaterno { get; set; }

    public string? ApellidoMaterno { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? Curp { get; set; }

    public string? Rfc { get; set; }

    public int? IdDireccion { get; set; }

    public string? Telefono { get; set; }

    public string? Celular { get; set; }

    public string? Correo { get; set; }

    public int? IdGenero { get; set; }

    public int? IdEstadoCivil { get; set; }

    public string? NombreContactoEmergencia { get; set; }

    public string? TelefonoContactoEmergencia { get; set; }

    public string? ParentescoContactoEmergencia { get; set; }

    public string? Nacionalidad { get; set; }

    public virtual ICollection<Aspirante> Aspirante { get; set; } = new List<Aspirante>();

    public virtual ICollection<Estudiante> Estudiante { get; set; } = new List<Estudiante>();

    public virtual Direccion? IdDireccionNavigation { get; set; }

    public virtual EstadoCivil? IdEstadoCivilNavigation { get; set; }

    public virtual Genero? IdGeneroNavigation { get; set; }

    public virtual ICollection<Profesor> Profesor { get; set; } = new List<Profesor>();
}
