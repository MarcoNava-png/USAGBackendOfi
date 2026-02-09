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
    public class ReciboProfile : Profile
    {
        public ReciboProfile()
        {
            CreateMap<ReciboDetalle, ReciboLineaDto>()
                .ForMember(d => d.IdReciboDetalle, o => o.MapFrom(s => s.IdReciboDetalle))
                .ForMember(d => d.IdConceptoPago, o => o.MapFrom(s => s.IdConceptoPago))
                .ForMember(d => d.Descripcion, o => o.MapFrom(s => s.Descripcion))
                .ForMember(d => d.Cantidad, o => o.MapFrom(s => s.Cantidad))
                .ForMember(d => d.PrecioUnitario, o => o.MapFrom(s => s.PrecioUnitario))
                .ForMember(d => d.Importe, o => o.MapFrom(s => s.Importe))
                .ForMember(d => d.RefTabla, o => o.MapFrom(s => s.RefTabla))
                .ForMember(d => d.RefId, o => o.MapFrom(s => s.RefId));

            CreateMap<Recibo, ReciboDto>()
                .ForMember(d => d.estatus, o => o.MapFrom(s => s.Estatus))
                .ForMember(d => d.Detalles, o => o.MapFrom(s => s.Detalles != null ? s.Detalles.ToList() : new List<ReciboDetalle>()));
        }
    }
}
