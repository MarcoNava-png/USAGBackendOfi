using System;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Asistencia
{
    public class AsistenciaUpdateRequest
    {
        public int IdAsistencia { get; set; }
        public EstadoAsistenciaEnum EstadoAsistencia { get; set; }
        public string? Observaciones { get; set; }
    }
}
