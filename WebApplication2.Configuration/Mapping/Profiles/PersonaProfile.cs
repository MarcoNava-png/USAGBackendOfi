using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class PersonaProfile : Profile
    {
        public PersonaProfile()
        {
            //CreateMap<Persona, PersonaDto>()
            //    .ForMember(dto => dto.PersonaGenero, map => map.MapFrom(model => model.PersonaGenero.Genero));
        }
    }
}
