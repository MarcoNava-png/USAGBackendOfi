using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class PlantillaCobroProfile : Profile
    {
        public PlantillaCobroProfile()
        {

            CreateMap<PlantillaCobro, PlantillaCobroDto>()
                .ForMember(d => d.NombrePlanEstudios, o => o.MapFrom(s => s.IdPlanEstudiosNavigation != null ? s.IdPlanEstudiosNavigation.NombrePlanEstudios : null))
                .ForMember(d => d.NombrePeriodo, o => o.Ignore()) 
                .ForMember(d => d.NombreTurno, o => o.Ignore())
                .ForMember(d => d.NombreModalidad, o => o.MapFrom(s => s.IdModalidadNavigation != null ? s.IdModalidadNavigation.DescModalidad : null))
                .ForMember(d => d.TotalConceptos, o => o.MapFrom(s => s.Detalles != null && s.Detalles.Any() ? s.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario) : 0m))
                .ForMember(d => d.Detalles, o => o.MapFrom(s => s.Detalles ?? new List<PlantillaCobroDetalle>()));

            CreateMap<PlantillaCobroDetalle, PlantillaCobroDetalleDto>()
                .ForMember(d => d.NombreConcepto, o => o.MapFrom(s => s.IdConceptoPagoNavigation != null ? s.IdConceptoPagoNavigation.Descripcion : null))
                .ForMember(d => d.ClaveConcepto, o => o.MapFrom(s => s.IdConceptoPagoNavigation != null ? s.IdConceptoPagoNavigation.Clave : null))
                .ForMember(d => d.Importe, o => o.MapFrom(s => s.Cantidad * s.PrecioUnitario));

            CreateMap<CreatePlantillaCobroDto, PlantillaCobro>()
                .ForMember(d => d.IdPlantillaCobro, o => o.Ignore())
                .ForMember(d => d.Version, o => o.MapFrom(_ => 1))
                .ForMember(d => d.EsActiva, o => o.MapFrom(_ => true))
                .ForMember(d => d.FechaCreacion, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.ModificadoPor, o => o.Ignore())
                .ForMember(d => d.FechaModificacion, o => o.Ignore())
                .ForMember(d => d.IdPlanEstudiosNavigation, o => o.Ignore())
                .ForMember(d => d.Detalles, o => o.MapFrom(s => s.Detalles))
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.CreadoPor, o => o.Ignore()); 

            CreateMap<CreatePlantillaCobroDetalleDto, PlantillaCobroDetalle>()
                .ForMember(d => d.IdPlantillaDetalle, o => o.Ignore())
                .ForMember(d => d.IdPlantillaCobro, o => o.Ignore())
                .ForMember(d => d.IdPlantillaCobroNavigation, o => o.Ignore())
                .ForMember(d => d.IdConceptoPagoNavigation, o => o.Ignore());
        }
    }
}
