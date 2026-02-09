using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class PagoProfile : Profile
    {
        public PagoProfile()
        {
            CreateMap<RegistrarPagoDto, Pago>()
                .ForMember(d => d.IdPago, o => o.Ignore())
                .ForMember(d => d.FechaPagoUtc, o => o.MapFrom(s => s.FechaPagoUtc == default ? DateTime.UtcNow : s.FechaPagoUtc))
                .ForMember(d => d.Estatus, o => o.MapFrom(s => s.estatus))
                .ForMember(d => d.Aplicaciones, o => o.Ignore())
                .ForMember(d => d.MedioPago, o => o.Ignore());

            CreateMap<Pago, PagoDto>()
                .ForMember(d => d.Estatus, o => o.MapFrom(s => s.Estatus));

            CreateMap<AplicacionLineaDto, PagoAplicacion>()
                .ForMember(d => d.IdPagoAplicacion, o => o.Ignore())
                .ForMember(d => d.IdPago, o => o.Ignore()) 
                .ForMember(d => d.MontoAplicado, o => o.MapFrom(s => s.Monto))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<ReciboDetalle, ReciboLineaDto>();
        }
    }
}
