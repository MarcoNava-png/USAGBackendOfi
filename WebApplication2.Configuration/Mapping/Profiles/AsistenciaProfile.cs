using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Asistencia;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class AsistenciaProfile : Profile
    {
        public AsistenciaProfile()
        {

            CreateMap<AsistenciaRegistroRequest, Asistencia>();

            CreateMap<Asistencia, AsistenciaDto>()
                .ForMember(dto => dto.NombreEstudiante, opt => opt.MapFrom(src =>
                    src.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.Nombre + " " +
                    src.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                    src.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.ApellidoMaterno))
                .ForMember(dto => dto.Matricula, opt => opt.MapFrom(src =>
                    src.Inscripcion.IdEstudianteNavigation.Matricula))
                .ForMember(dto => dto.NombreMateria, opt => opt.MapFrom(src =>
                    src.GrupoMateria.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre))
                .ForMember(dto => dto.NombreProfesor, opt => opt.MapFrom(src =>
                    src.ProfesorRegistro.IdPersonaNavigation.Nombre + " " +
                    src.ProfesorRegistro.IdPersonaNavigation.ApellidoPaterno))
                .ForMember(dto => dto.EstadoAsistenciaTexto, opt => opt.MapFrom(src =>
                    src.EstadoAsistencia.ToString()));
        }
    }
}
