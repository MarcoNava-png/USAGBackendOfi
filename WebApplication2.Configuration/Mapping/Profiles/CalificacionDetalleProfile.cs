using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class CalificacionDetalleProfile : Profile
    {
        public CalificacionDetalleProfile()
        {
            CreateMap<CalificacionDetalleUpsertRequest, CalificacionDetalle>()
                .ForMember(d => d.TipoEvaluacionEnum, o => o.MapFrom(s => (TipoEvaluacionEnum)s.TipoEvaluacionEnum))
                .ForMember(d => d.FechaAplicacion, o => o.MapFrom(s => s.FechaAplicacion ?? DateTime.UtcNow))
                .ForMember(d => d.FechaCaptura, o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CalificacionDetalle, CalificacionDetalleItemResponse>()
                .ForMember(d => d.TipoEvaluacionEnum, o => o.MapFrom(s => (int)s.TipoEvaluacionEnum));
        }

    }
}
