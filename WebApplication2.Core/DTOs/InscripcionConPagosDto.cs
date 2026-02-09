using System.Collections.Generic;

namespace WebApplication2.Core.DTOs
{
    public class InscripcionConPagosDto
    {
        public int IdInscripcion { get; set; }
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; }
        public string NombreEstudiante { get; set; }
        public int IdGrupoMateria { get; set; }
        public string NombreMateria { get; set; }
        public bool EsNuevoIngreso { get; set; }
        public string TipoInscripcion { get; set; } 
        public int CantidadRecibosGenerados { get; set; }
        public decimal MontoTotalRecibos { get; set; }
        public List<ReciboDto> Recibos { get; set; }
    }
}
