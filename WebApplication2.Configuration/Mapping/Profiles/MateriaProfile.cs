using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.NewFolder;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class MateriaProfile : Profile
    {
        public MateriaProfile()
        {
            CreateMap<MateriaPlan, MateriaPlanDto>()
            .ForMember(dto => dto.Materia, map => map.MapFrom(model => model.IdMateriaNavigation.Nombre))
            .ForMember(dto => dto.NombrePlanEstudios, map => map.MapFrom(model => model.IdPlanEstudiosNavigation.NombrePlanEstudios));

            CreateMap<MateriaPlanRequest, MateriaPlan>();
        }
    }
}
