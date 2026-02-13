using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Profesor;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class ProfesorProfile : Profile
    {
        public ProfesorProfile()
        {
            CreateMap<Profesor, ProfesorDto>()
                .ForMember(dto => dto.NoEmpleado, config => config.MapFrom(model => model.NoEmpleado))
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(model => $"{model.IdPersonaNavigation.Nombre} {model.IdPersonaNavigation.ApellidoPaterno} {model.IdPersonaNavigation.ApellidoMaterno}"))
                .ForMember(dto => dto.Nombre, config => config.MapFrom(model => model.IdPersonaNavigation.Nombre))
                .ForMember(dto => dto.ApellidoPaterno, config => config.MapFrom(model => model.IdPersonaNavigation.ApellidoPaterno))
                .ForMember(dto => dto.ApellidoMaterno, config => config.MapFrom(model => model.IdPersonaNavigation.ApellidoMaterno))
                .ForMember(dto => dto.Correo, config => config.MapFrom(model => model.IdPersonaNavigation.Correo))
                .ForMember(dto => dto.Telefono, config => config.MapFrom(model => model.IdPersonaNavigation.Telefono))
                .ForMember(dto => dto.Curp, config => config.MapFrom(model => model.IdPersonaNavigation.Curp))
                .ForMember(dto => dto.Rfc, config => config.MapFrom(model => model.IdPersonaNavigation.Rfc))
                .ForMember(dto => dto.FechaNacimiento, config => config.MapFrom(model => model.IdPersonaNavigation.FechaNacimiento))
                .ForMember(dto => dto.GeneroId, config => config.MapFrom(model => model.IdPersonaNavigation.IdGenero))
                .ForMember(dto => dto.IdEstadoCivil, config => config.MapFrom(model => model.IdPersonaNavigation.IdEstadoCivil))
                .ForMember(dto => dto.CampusId, config => config.MapFrom(model => model.CampusId))
                .ForMember(dto => dto.Calle, config => config.MapFrom(model => model.IdPersonaNavigation.IdDireccionNavigation != null ? model.IdPersonaNavigation.IdDireccionNavigation.Calle : null))
                .ForMember(dto => dto.NumeroExterior, config => config.MapFrom(model => model.IdPersonaNavigation.IdDireccionNavigation != null ? model.IdPersonaNavigation.IdDireccionNavigation.NumeroExterior : null))
                .ForMember(dto => dto.NumeroInterior, config => config.MapFrom(model => model.IdPersonaNavigation.IdDireccionNavigation != null ? model.IdPersonaNavigation.IdDireccionNavigation.NumeroInterior : null))
                .ForMember(dto => dto.CodigoPostalId, config => config.MapFrom(model => model.IdPersonaNavigation.IdDireccionNavigation != null ? model.IdPersonaNavigation.IdDireccionNavigation.CodigoPostalId : (int?)null));

            CreateMap<ProfesorRequest, Profesor>()
                .ForMember(model => model.IdPersonaNavigation, config => config.MapFrom(dto => new Persona
                {
                    Nombre = dto.Nombre,
                    ApellidoPaterno = dto.ApellidoPaterno,
                    ApellidoMaterno = dto.ApellidoMaterno,
                    FechaNacimiento = dto.FechaNacimiento,
                    Curp = dto.CURP,
                    Rfc = dto.Rfc,
                    Correo = dto.Correo,
                    Telefono = dto.Telefono,
                    IdGenero = dto.GeneroId,
                    IdEstadoCivil = dto.IdEstadoCivil,
                    IdDireccionNavigation = (
                        !string.IsNullOrEmpty(dto.Calle)
                        && !string.IsNullOrEmpty(dto.NumeroExterior)
                        && dto.CodigoPostalId.HasValue) ? new Direccion
                        {
                            Calle = dto.Calle,
                            NumeroExterior = dto.NumeroExterior,
                            NumeroInterior = dto.NumeroInterior,
                            CodigoPostalId = dto.CodigoPostalId.Value
                        } : null
                }));

            CreateMap<ProfesorUpdateRequest, Profesor>()
                .ForMember(model => model.IdPersonaNavigation, config => config.MapFrom(dto => new Persona
                {
                    Nombre = dto.Nombre,
                    ApellidoPaterno = dto.ApellidoPaterno,
                    ApellidoMaterno = dto.ApellidoMaterno,
                    FechaNacimiento = dto.FechaNacimiento,
                    Curp = dto.CURP,
                    Rfc = dto.Rfc,
                    Correo = dto.Correo,
                    Telefono = dto.Telefono,
                    IdGenero = dto.GeneroId,
                    IdEstadoCivil = dto.IdEstadoCivil,
                    IdDireccionNavigation = (
                        !string.IsNullOrEmpty(dto.Calle)
                        && !string.IsNullOrEmpty(dto.NumeroExterior)
                        && dto.CodigoPostalId.HasValue) ? new Direccion
                        {
                            Calle = dto.Calle,
                            NumeroExterior = dto.NumeroExterior,
                            NumeroInterior = dto.NumeroInterior,
                            CodigoPostalId = dto.CodigoPostalId.Value
                        } : null
                }));
        }
    }
}
