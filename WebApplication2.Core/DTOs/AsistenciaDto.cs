using System;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class AsistenciaDto
    {
        public int IdAsistencia { get; set; }
        public int InscripcionId { get; set; }
        public string NombreEstudiante { get; set; }
        public string Matricula { get; set; }
        public int GrupoMateriaId { get; set; }
        public string NombreMateria { get; set; }
        public DateTime FechaSesion { get; set; }
        public EstadoAsistenciaEnum EstadoAsistencia { get; set; }
        public string EstadoAsistenciaTexto { get; set; }
        public string? Observaciones { get; set; }
        public string NombreProfesor { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
