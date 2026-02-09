using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class AspiranteProfile : Profile
    {
        public AspiranteProfile()
        {
            CreateMap<Aspirante, AspiranteDto>()
                .ForMember(dto => dto.NombreCompleto, map => map.MapFrom(model => $"{model.IdPersonaNavigation!.ApellidoPaterno} {model.IdPersonaNavigation!.ApellidoMaterno} {model.IdPersonaNavigation!.Nombre}"))
                .ForMember(dto => dto.PersonaId, map => map.MapFrom(model => model.IdPersonaNavigation!.IdPersona))
                .ForMember(dto => dto.Email, map => map.MapFrom(model => model.IdPersonaNavigation!.Correo))
                .ForMember(dto => dto.Telefono, map => map.MapFrom(model => model.IdPersonaNavigation!.Telefono))
                .ForMember(dto => dto.AspiranteEstatus, map => map.MapFrom(model => model.IdAspiranteEstatusNavigation.DescEstatus))
                .ForMember(dto => dto.PlanEstudios, map => map.MapFrom(model => model.IdPlanNavigation.NombrePlanEstudios))
                .ForMember(dto => dto.IdDireccion, map => map.MapFrom(model => model.IdPersonaNavigation!.IdDireccion))
                .ForMember(dto => dto.CodigoPostalId, map => map.MapFrom(model => model.IdPersonaNavigation!.IdDireccionNavigation!.CodigoPostalId))
                .ForMember(dto => dto.MunicipioId, map => map.MapFrom(model => model.IdPersonaNavigation!.IdDireccionNavigation!.CodigoPostal!.MunicipioId))
                .ForMember(dto => dto.EstadoId, map => map.MapFrom(model => model.IdPersonaNavigation!.IdDireccionNavigation!.CodigoPostal!.Municipio.EstadoId))
                .ForMember(dto => dto.CreatedBy, map => map.MapFrom(model => model.CreatedBy))
                .ForMember(dto => dto.UsuarioRegistroNombre, map => map.Ignore()); 
        }
    }
}
