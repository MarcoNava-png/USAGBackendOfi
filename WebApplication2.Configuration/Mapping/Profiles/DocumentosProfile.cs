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
    public class DocumentosProfile : Profile
    {
        public DocumentosProfile()
        {
            CreateMap<DocumentoRequisito, DocumentoRequisitoDto>();

            CreateMap<AspiranteDocumento, AspiranteDocumentoDto>()
                .ForMember(d => d.IdDocumentoRequisito, o => o.MapFrom(s => s.IdDocumentoRequisito))
                .ForMember(d => d.Clave, o => o.MapFrom(s => s.Requisito != null ? s.Requisito.Clave : string.Empty))
                .ForMember(d => d.Descripcion, o => o.MapFrom(s => s.Requisito != null ? s.Requisito.Descripcion : string.Empty))
                .ForMember(d => d.Estatus, o => o.MapFrom(s => s.Estatus))
                .ForMember(d => d.UrlArchivo, o => o.MapFrom(s => s.UrlArchivo))
                .ForMember(d => d.Notas, o => o.MapFrom(s => s.Notas));

            CreateMap<CargarDocumentoRequestDto, AspiranteDocumento>()
                .ForMember(d => d.IdAspiranteDocumento, o => o.Ignore())
                .ForMember(d => d.Estatus, o => o.MapFrom(_ => EstatusDocumentoEnum.SUBIDO))
                .ForMember(d => d.FechaSubidoUtc, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.Aspirante, o => o.Ignore())
                .ForMember(d => d.Requisito, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore());
        }
    }
}
