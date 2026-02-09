namespace WebApplication2.Core.Models;

public partial class Direccion : BaseEntity
{
    public int IdDireccion { get; set; }

    public string? Calle { get; set; }

    public string? NumeroExterior { get; set; }

    public string? NumeroInterior { get; set; }

    public int CodigoPostalId { get; set; }

    public virtual ICollection<Campus> Campus { get; set; } = new List<Campus>();

    public virtual ICollection<Persona> Persona { get; set; } = new List<Persona>();
    public virtual CodigoPostal? CodigoPostal { get; set; }
}
