using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class AspiranteGridItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string PlanEstudiosInteres { get; set; } = "";
        public string? Telefono { get; set; }
        public string Estatus { get; set; } = "";
        public DateTime FechaRegistroUtc { get; set; }

        public string EstatusPago { get; set; } = "SIN_RECIBO";     
        public string EstatusDocumentos { get; set; } = "INCOMPLETO"; 

        public bool BtnRegistrarPagoHabilitado { get; set; } = true;
        public bool BtnDocumentosHabilitado { get; set; } = true;
    }
}
