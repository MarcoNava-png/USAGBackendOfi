using System;

namespace WebApplication2.Core.DTOs
{

    public class AsistenciaEstudianteDto
    {
        public int? IdAsistencia { get; set; } 
        public int IdInscripcion { get; set; }
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public bool? Presente { get; set; }
        public bool Justificada { get; set; }
        public string MotivoJustificacion { get; set; }
        public string HoraRegistro { get; set; } 
    }
}
