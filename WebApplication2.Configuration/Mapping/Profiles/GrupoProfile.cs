using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Grupo;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class GrupoProfile : Profile
    {
        public GrupoProfile()
        {
            CreateMap<Grupo, GrupoDto>()
                .ForMember(dto => dto.NombreGrupo, config => config.MapFrom(model => model.NombreGrupo))
                .ForMember(dto => dto.PlanEstudios, config => config.MapFrom(model => model.IdPlanEstudiosNavigation.NombrePlanEstudios))
                .ForMember(dto => dto.PeriodoAcademico, config => config.MapFrom(model => model.IdPeriodoAcademicoNavigation.Nombre))
                .ForMember(dto => dto.Turno, config => config.MapFrom(model => model.IdTurnoNavigation.Nombre))
                .ForMember(dto => dto.EstudiantesInscritos, config => config.MapFrom(model => GetTotalEstudiantesInscritos(model)));
            CreateMap<Grupo, GrupoDetalleDto>()
                .ForMember(dto => dto.PlanEstudios, config => config.MapFrom(model => model.IdPlanEstudiosNavigation.NombrePlanEstudios))
                .ForMember(dto => dto.PeriodoAcademico, config => config.MapFrom(model => model.IdPeriodoAcademicoNavigation.Nombre))
                .ForMember(dto => dto.Turno, config => config.MapFrom(model => model.IdTurnoNavigation.Nombre));
            CreateMap<GrupoRequest, Grupo>();
            CreateMap<GrupoUpdateRequest, Grupo>();
        }

        private static int GetTotalEstudiantesInscritos(Grupo grupo)
        {
            var estudiantesPorMaterias = grupo.GrupoMateria
                .SelectMany(gm => gm.Inscripcion ?? Enumerable.Empty<Inscripcion>())
                .Where(i => i.Status == StatusEnum.Active)
                .Select(i => i.IdEstudiante)
                .Distinct()
                .ToHashSet();

            var estudiantesDirectos = (grupo.EstudianteGrupo ?? Enumerable.Empty<EstudianteGrupo>())
                .Where(eg => eg.Status == StatusEnum.Active)
                .Select(eg => eg.IdEstudiante)
                .ToHashSet();

            return estudiantesPorMaterias.Union(estudiantesDirectos).Count();
        }
    }
}
