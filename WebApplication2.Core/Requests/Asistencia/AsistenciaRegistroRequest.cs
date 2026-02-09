using System;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Asistencia
{
    public class AsistenciaRegistroRequest
    {
        public int InscripcionId { get; set; }
        public int GrupoMateriaId { get; set; }
        public DateTime FechaSesion { get; set; }
        public EstadoAsistenciaEnum EstadoAsistencia { get; set; }
        public string? Observaciones { get; set; }
    }
}
