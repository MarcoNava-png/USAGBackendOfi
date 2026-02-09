using System;
using System.Collections.Generic;

namespace WebApplication2.Core.Requests.Asistencia
{
    public class AsistenciaMasivaRequest
    {
        public int GrupoMateriaId { get; set; }
        public DateTime FechaSesion { get; set; }
        public List<AsistenciaRegistroRequest> Asistencias { get; set; }
    }
}
