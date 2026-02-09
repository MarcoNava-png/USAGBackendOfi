using AutoMapper;
using Microsoft.AspNetCore.Identity;
using WebApplication2.Core.Requests.Auth;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserSignupRequest, IdentityUser>()
                .ForMember(user => user.UserName, map => map.MapFrom(dto => dto.Email));
        }
    }
}
