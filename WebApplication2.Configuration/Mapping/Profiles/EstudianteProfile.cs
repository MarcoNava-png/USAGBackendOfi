using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Estudiante;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class EstudianteProfile : Profile
    {
        public EstudianteProfile()
        {
            CreateMap<Estudiante, EstudianteDto>()
                .ForMember(dto => dto.NombreCompleto, map => map.MapFrom(model => $"{model.IdPersonaNavigation.ApellidoPaterno} {model.IdPersonaNavigation.ApellidoMaterno} {model.IdPersonaNavigation.Nombre}"))
                .ForMember(dto => dto.Telefono, map => map.MapFrom(model => model.IdPersonaNavigation.Telefono))
                .ForMember(dto => dto.PlanEstudios, map => map.MapFrom(model => model.IdPlanActualNavigation != null ? model.IdPlanActualNavigation.NombrePlanEstudios : "No seleccionaddo"))
                .ForMember(dto => dto.email, map => map.MapFrom(model => !string.IsNullOrEmpty(model.Email) ? model.Email : model.IdPersonaNavigation.Correo));

            CreateMap<Estudiante, EstudianteDetalleDto>()
                .ForMember(dto => dto.NombreCompleto, map => map.MapFrom(model => $"{model.IdPersonaNavigation.ApellidoPaterno} {model.IdPersonaNavigation.ApellidoMaterno} {model.IdPersonaNavigation.Nombre}"))
                .ForMember(dto => dto.Telefono, map => map.MapFrom(model => model.IdPersonaNavigation.Telefono))
                .ForMember(dto => dto.PlanEstudios, map => map.MapFrom(model => model.IdPlanActualNavigation != null ? model.IdPlanActualNavigation.NombrePlanEstudios : "No seleccionaddo"))
                .ForMember(dto => dto.Materias, map => map.MapFrom(model => model.Inscripcion.Select(i => i.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre)));

            CreateMap<EstudianteMatricularRequest, Estudiante>();
        }
    }
}
