namespace WebApplication2.Core.DTOs.Convenio
{
    public class CrearConvenioAlcanceDto
    {
        public int? IdCampus { get; set; }
        public int? IdPlanEstudios { get; set; }
        public DateOnly? VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
    }
}
