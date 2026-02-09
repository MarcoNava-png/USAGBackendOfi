using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication2.Core.DTOs.PlantillaCobro;

namespace WebApplication2.Core.DTOs
{
    public class UpdatePlantillaCobroDto
    {
        [StringLength(200)]
        public string? NombrePlantilla { get; set; }

        public DateTime? FechaVigenciaInicio { get; set; }
        public DateTime? FechaVigenciaFin { get; set; }

        [Range(0, 2)]
        public int? EstrategiaEmision { get; set; }

        [Range(1, 12)]
        public int? NumeroRecibos { get; set; }

        [Range(1, 31)]
        public int? DiaVencimiento { get; set; }

        public List<CreatePlantillaCobroDetalleDto>? Detalles { get; set; }
    }
}
