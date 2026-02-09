namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class ValidacionInscripcionGrupoDto
    {
        public bool EstudianteActivo { get; set; }
        public bool PlanEstudiosCompatible { get; set; }
        public bool PeriodoActivo { get; set; }
        public bool PagosAlCorriente { get; set; }
        public bool CuposDisponibles { get; set; }
        public bool SinDuplicados { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }
}
