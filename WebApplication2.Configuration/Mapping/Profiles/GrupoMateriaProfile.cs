using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Grupo;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class GrupoMateriaProfile : Profile
    {
        public GrupoMateriaProfile()
        {
            CreateMap<GrupoMateria, GrupoMateriaDto>()
                .ForMember(dto => dto.Materia, config => config.MapFrom(model => model.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre))
                .ForMember(dto => dto.Profesor, config => config.MapFrom(model => $"{model.IdProfesorNavigation!.IdPersonaNavigation.ApellidoPaterno} {model.IdProfesorNavigation!.IdPersonaNavigation.ApellidoMaterno} {model.IdProfesorNavigation!.IdPersonaNavigation.Nombre}"));

            CreateMap<GrupoMateriasRequest, GrupoMateria>();
            CreateMap<GrupoMateriaUpdateRequest, GrupoMateria>();
        }
    }
}
