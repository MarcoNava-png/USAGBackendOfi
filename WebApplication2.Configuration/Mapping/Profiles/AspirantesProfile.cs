using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class AspirantesProfile : Profile
    {
        public AspirantesProfile()
        {
            CreateMap<Aspirante, AspiranteGridItemDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.IdAspirante))
                .ForMember(d => d.Nombre, o => o.MapFrom(s =>
                    (s.IdPersonaNavigation != null
                        ? $"{(s.IdPersonaNavigation.Nombre ?? "").Trim()} {(s.IdPersonaNavigation.ApellidoPaterno ?? "").Trim()} {(s.IdPersonaNavigation.ApellidoMaterno ?? "").Trim()}".Trim()
                        : "").Trim()))
                .ForMember(d => d.PlanEstudiosInteres, o => o.MapFrom(s =>
                    s.IdPlanNavigation != null ? (s.IdPlanNavigation.NombrePlanEstudios ?? "") : ""))
                .ForMember(d => d.Telefono, o => o.MapFrom(s =>
                    s.IdPersonaNavigation != null
                        ? (string.IsNullOrWhiteSpace(s.IdPersonaNavigation.Celular)
                            ? s.IdPersonaNavigation.Telefono
                            : s.IdPersonaNavigation.Celular)
                        : null))
                .ForMember(d => d.Estatus, o => o.MapFrom(s =>
                    s.IdAspiranteEstatusNavigation != null ? (s.IdAspiranteEstatusNavigation.DescEstatus ?? "") : ""))
                .ForMember(d => d.FechaRegistroUtc, o => o.MapFrom(s => s.FechaRegistro))
                .ForMember(d => d.EstatusPago, o => o.Ignore())
                .ForMember(d => d.EstatusDocumentos, o => o.Ignore())
                .ForMember(d => d.BtnRegistrarPagoHabilitado, o => o.Ignore())
                .ForMember(d => d.BtnDocumentosHabilitado, o => o.Ignore());
        }
    }
}
