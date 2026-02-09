using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class CalificacionDetalle : BaseEntity
    {
        public int Id { get; set; }
        public int CalificacionParcialId { get; set; }
        public CalificacionParcial CalificacionParcial { get; set; } 
        public int GrupoMateriaId { get; set; }                      
        public TipoEvaluacionEnum TipoEvaluacionEnum { get; set; }   
        public string Nombre { get; set; }                          
        public decimal PesoEvaluacion { get; set; }                  
        public decimal MaxPuntos { get; set; }                       
        public DateTime FechaAplicacion { get; set; }
        public decimal Puntos { get; set; }                          
        public string ApplicationUserName { get; set; }             
        public DateTime FechaCaptura { get; set; }                  
    }
}
