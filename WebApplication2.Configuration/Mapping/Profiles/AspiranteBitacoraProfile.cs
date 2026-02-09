using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Aspirante;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class AspiranteBitacoraProfile : Profile
    {
        public AspiranteBitacoraProfile()
        {
            CreateMap<AspiranteBitacoraSeguimiento, AspiranteSeguimientoDto>()
                .ForMember(dto => dto.UsuarioAtiendeNombre, map => map.MapFrom(model => $"{model.UsuarioAtiende.Nombres} {model.UsuarioAtiende.Apellidos}"));
            CreateMap<AspiranteSeguimientoRequest, AspiranteBitacoraSeguimiento>();
            CreateMap<AspiranteSeguimientoUpdateRequest, AspiranteBitacoraSeguimiento>();
        }
    }
}
