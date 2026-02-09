namespace WebApplication2.Core.DTOs.Convenio
{
    public class ConvenioAlcanceDto
    {
        public int IdConvenioAlcance { get; set; }
        public int IdConvenio { get; set; }
        public int? IdCampus { get; set; }
        public string? NombreCampus { get; set; }
        public int? IdPlanEstudios { get; set; }
        public string? NombrePlanEstudios { get; set; }
        public DateOnly? VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
    }
}
