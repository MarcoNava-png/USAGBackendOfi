using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PeriodoAcademico;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class PeriodoAcademicoProfile : Profile
    {
        public PeriodoAcademicoProfile()
        {
            CreateMap<PeriodoAcademico, PeriodoAcademicoDto>()
                .ForMember(dto => dto.Periodicidad, config => config.MapFrom(model =>
                    model.IdPeriodicidadNavigation != null
                        ? model.IdPeriodicidadNavigation.DescPeriodicidad
                        : null));
            CreateMap<PeriodoAcademicoRequest, PeriodoAcademico>();
            CreateMap<PeriodoAcademicoUpdateRequest, PeriodoAcademico>();
        }
    }
}
