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
    public class BecaRecargoProfile : Profile
    {
        public BecaRecargoProfile()
        {
            CreateMap<CrearBecaDto, BecaAsignacion>()
                .ForMember(d => d.IdBecaAsignacion, o => o.Ignore());

            CreateMap<PoliticaRecargoDto, RecargoPolitica>()
                .ForMember(d => d.IdRecargoPolitica, o => o.Ignore());

            CreateMap<RecargoPolitica, PoliticaRecargoDto>();
        }
    }
}
