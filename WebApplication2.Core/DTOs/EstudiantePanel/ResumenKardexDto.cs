namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class ResumenKardexDto
    {
        public decimal PromedioGeneral { get; set; }
        public int CreditosCursados { get; set; }
        public int CreditosTotales { get; set; }
        public decimal PorcentajeAvance { get; set; }
        public int MateriasAprobadas { get; set; }
        public int MateriasReprobadas { get; set; }
        public int MateriasCursando { get; set; }
        public int MateriasPendientes { get; set; }
        public string EstatusAcademico { get; set; } = "Regular";

        public List<MateriaResumenDto> UltimasMaterias { get; set; } = new();
    }
}
