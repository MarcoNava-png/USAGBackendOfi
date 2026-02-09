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
    public class BitacoraProfile : Profile
    {
        public BitacoraProfile()
        {
            CreateMap<BitacoraCreateDto, BitacoraRecibo>()
                .ForMember(d => d.IdBitacora, o => o.Ignore())
                .ForMember(d => d.FechaUtc, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.Usuario, o => o.MapFrom(_ => "system")); 

            CreateMap<BitacoraRecibo, BitacoraDto>();
        }
    }
}
