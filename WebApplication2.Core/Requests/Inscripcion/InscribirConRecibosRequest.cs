using System;

namespace WebApplication2.Core.Requests.Inscripcion
{
    public class InscribirConRecibosRequest
    {
        public int IdEstudiante { get; set; }
        public int IdGrupoMateria { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public bool? EsNuevoIngreso { get; set; }
        public string? Observaciones { get; set; }
    }
}
