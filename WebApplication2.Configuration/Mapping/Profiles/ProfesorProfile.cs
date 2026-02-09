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
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(model => $"{model.IdPersonaNavigation.Nombre} {model.IdPersonaNavigation.ApellidoPaterno} {model.IdPersonaNavigation.ApellidoMaterno}"));

            CreateMap<ProfesorRequest, Profesor>()
                .ForMember(model => model.IdPersonaNavigation, config => config.MapFrom(dto => new Persona
                {
                    Nombre = dto.Nombre,
                    ApellidoPaterno = dto.ApellidoMaterno,
                    FechaNacimiento = dto.FechaNacimiento,
                    Curp = dto.CURP,
                    Rfc = dto.Rfc,
                    Correo = dto.Correo,
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
                    ApellidoPaterno = dto.ApellidoMaterno,
                    FechaNacimiento = dto.FechaNacimiento,
                    Curp = dto.CURP,
                    Rfc = dto.Rfc,
                    Correo = dto.Correo,
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
