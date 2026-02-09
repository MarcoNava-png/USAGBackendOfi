using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class CalificacionDetalleUpsertRequest
    {
        public int CalificacionParcialId { get; set; }
        public int GrupoMateriaId { get; set; }
        public int InscripcionId { get; set; }
        public int TipoEvaluacionEnum { get; set; } 
        public string TipoEvaluacionName { get; set; }
        public string Nombre { get; set; }
        public decimal PesoEvaluacion { get; set; }
        public decimal MaxPuntos { get; set; }
        public decimal Puntos { get; set; }
        public DateTime? FechaAplicacion { get; set; } 
    }
}
