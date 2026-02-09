using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PlanEstudios;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class PlanEstudioProfile : Profile
    {
        public PlanEstudioProfile()
        {
            CreateMap<PlanEstudios, PlanEstudioDto>()
                .ForMember(dto => dto.Periodicidad, conf => conf.MapFrom(model =>
                    model.IdPeriodicidadNavigation != null ? model.IdPeriodicidadNavigation.DescPeriodicidad : ""))
                .ForMember(dto => dto.NombreCampus, conf => conf.MapFrom(model =>
                    model.IdCampusNavigation != null ? model.IdCampusNavigation.Nombre : ""))
                .ForMember(dto => dto.Activo, conf => conf.MapFrom(model => model.Status == StatusEnum.Active));

            CreateMap<PlanEstudiosRequest, PlanEstudios>();
            CreateMap<PlanEstudiosUpdateRequest, PlanEstudios>();
        }
    }
}
