using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class PlanPagoProfile : Profile
    {
        public PlanPagoProfile()
        {
            CreateMap<CrearPlanPagoDto, PlanPago>()
                .ForMember(d => d.IdPlanPago, o => o.Ignore())
                .ForMember(d => d.IdModalidadPlan, o => o.MapFrom(s => s.idModalidadPlan))
                .ForMember(d => d.Activo, o => o.MapFrom(s => s.Activo));

            CreateMap<CrearPlanDetalleDto, PlanPagoDetalle>()
                .ForMember(d => d.IdPlanPagoDetalle, o => o.Ignore())
                .ForMember(d => d.IdPlanPago, o => o.Ignore()); 

            CreateMap<AsignarPlanDto, PlanPagoAsignacion>()
                .ForMember(d => d.IdPlanPagoAsignacion, o => o.Ignore())
                .ForMember(d => d.FechaAsignacionUtc, o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<PlanPagoAsignacion, AsignacionDto>();
        }
    }
}
