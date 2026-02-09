using System;
using System.Collections.Generic;

namespace WebApplication2.Core.Requests.Asistencia
{
    public class RegistrarAsistenciasRequest
    {
        public int IdGrupoMateria { get; set; }
        public DateTime Fecha { get; set; }
        public List<AsistenciaItemRequest> Asistencias { get; set; }
    }

    public class AsistenciaItemRequest
    {
        public int IdInscripcion { get; set; }
        public bool Presente { get; set; }
        public bool Justificada { get; set; }
        public string MotivoJustificacion { get; set; }
    }
}
