using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class ConceptoPrecioProfile : Profile
    {
        public ConceptoPrecioProfile()
        {

            CreateMap<CrearConceptoDto, ConceptoPago>()
                .ForMember(d => d.IdConceptoPago, o => o.Ignore())
                .ForMember(d => d.Clave, o => o.MapFrom(s => s.Clave.Trim().ToUpper()))
                .ForMember(d => d.Nombre, o => o.MapFrom(s => s.Nombre))
                .ForMember(d => d.Descripcion, o => o.MapFrom(s => s.Descripcion ?? s.Nombre))
                .ForMember(d => d.Tipo, o => o.MapFrom(s => s.ConceptoTipo ?? ConceptoTipoEnum.OTRO))
                .ForMember(d => d.AplicaA, o => o.MapFrom(s => s.ConceptoAplica ?? ConceptoAplicaAEnum.Ambos))
                .ForMember(d => d.EsObligatorio, o => o.MapFrom(s => s.EsObligatorio ?? false))
                .ForMember(d => d.PeriodicidadMeses, o => o.MapFrom(s => s.PeriodicidadMeses))
                .ForMember(d => d.PermiteBeca, o => o.MapFrom(s => s.PermiteBeca))
                .ForMember(d => d.Activo, o => o.MapFrom(_ => true))
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());

            CreateMap<ConceptoPago, ConceptoDto>()
                .ForMember(d => d.Nombre, o => o.MapFrom(s => s.Nombre ?? s.Descripcion ?? s.Clave))
                .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Activo ? 1 : 0))
                .ForMember(d => d.ConceptoTipo, o => o.MapFrom(s => s.Tipo))
                .ForMember(d => d.ConceptoAplica, o => o.MapFrom(s => s.AplicaA));

            CreateMap<CrearPrecioDto, ConceptoPrecio>()
                .ForMember(d => d.IdConceptoPrecio, o => o.Ignore())
                .ForMember(d => d.IdConceptoPago, o => o.Ignore()) 
                .ForMember(d => d.Importe, o => o.MapFrom(s => s.Importe))
                .ForMember(d => d.Moneda, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Moneda) ? "MXN" : s.Moneda.ToUpper()))
                .ForMember(d => d.VigenciaDesde, o => o.MapFrom(s => s.VigenciaDesde))
                .ForMember(d => d.VigenciaHasta, o => o.MapFrom(s => s.VigenciaHasta))
                .ForMember(d => d.Activo, o => o.MapFrom(s => s.Activo))
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());

            CreateMap<ConceptoPrecio, ConceptoPrecioDto>();

            CreateMap<ConceptoPrecio, PrecioDto>();  

        }

    }
}
