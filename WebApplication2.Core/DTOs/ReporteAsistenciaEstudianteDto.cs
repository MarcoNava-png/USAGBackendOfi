using System.Collections.Generic;

namespace WebApplication2.Core.DTOs
{
    public class ReporteAsistenciaEstudianteDto
    {
        public int InscripcionId { get; set; }
        public string NombreEstudiante { get; set; }
        public string Matricula { get; set; }
        public string NombreMateria { get; set; }
        public int TotalSesiones { get; set; }
        public int TotalPresentes { get; set; }
        public int TotalAusentes { get; set; }
        public int TotalRetardos { get; set; }
        public int TotalJustificadas { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
        public List<AsistenciaDto> Asistencias { get; set; }
    }
}
