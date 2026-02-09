using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Parciales;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class ParcialesProfile : Profile
    {
        public ParcialesProfile()
        {
            CreateMap<Parciales, ParcialesDto>();
            CreateMap<ParcialesRequest, Parciales>();
            CreateMap<ParcialesUpdateRequest,  Parciales>();
        }
    }
}
