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
    public class CalificacionesProfile : Profile
    {
        public CalificacionesProfile()
        {
            CreateMap<CalificacionParcialCreateRequest, CalificacionParcial>()
                .ForMember(d => d.StatusParcial, o => o.MapFrom(_ => StatusParcialEnum.Abierto))
                .ForMember(d => d.FechaApertura, o => o.MapFrom(s => DateTime.UtcNow));

            CreateMap<CalificacionParcial, CalificacionParcialResponse>();
            CreateMap<CalificacionParcial, CalificacionesParcialDto>();

        }
    }
}
