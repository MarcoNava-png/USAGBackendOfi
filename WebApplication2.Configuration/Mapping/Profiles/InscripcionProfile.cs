using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class InscripcionProfile : Profile
    {
        public InscripcionProfile()
        {
            CreateMap<Inscripcion, InscripcionDto>()
                .ForMember(dest => dest.NombreGrupoMateria, opt => opt.MapFrom(src =>
                    src.IdGrupoMateriaNavigation != null && src.IdGrupoMateriaNavigation.IdGrupoNavigation != null
                        ? src.IdGrupoMateriaNavigation.IdGrupoNavigation.NombreGrupo
                        : null))
                .ForMember(dest => dest.NombreMateria, opt => opt.MapFrom(src =>
                    src.IdGrupoMateriaNavigation != null
                    && src.IdGrupoMateriaNavigation.IdMateriaPlanNavigation != null
                    && src.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation != null
                        ? src.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre
                        : null))
                .ForMember(dest => dest.NombreGrupo, opt => opt.MapFrom(src =>
                    src.IdGrupoMateriaNavigation != null && src.IdGrupoMateriaNavigation.IdGrupoNavigation != null
                        ? src.IdGrupoMateriaNavigation.IdGrupoNavigation.NombreGrupo
                        : null))
                .ForMember(dest => dest.IdPeriodoAcademico, opt => opt.MapFrom(src =>
                    src.IdGrupoMateriaNavigation != null && src.IdGrupoMateriaNavigation.IdGrupoNavigation != null
                        ? src.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPeriodoAcademico
                        : (int?)null));
        }
    }
}
