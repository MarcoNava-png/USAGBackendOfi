using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Campus;

namespace WebApplication2.Configuration.Mapping.Profiles
{
    public class CampusProfile : Profile
    {
        public CampusProfile()
        {
            CreateMap<Campus, CampusDto>()
                .ForMember(dto => dto.Direccion, config => config.MapFrom(src => FormatDireccion(src)))
                .ForMember(dto => dto.Activo, cfg => cfg.MapFrom(m => m.Status == StatusEnum.Active));

            CreateMap<CampusRequest, Campus>()
                .ForMember(model => model.IdDireccionNavigation, config => config.MapFrom(src => CreateDireccion(src.Calle, src.NumeroExterior, src.NumeroInterior, src.CodigoPostalId)));

            CreateMap<CampusUpdateRequest, Campus>()
                .ForMember(model => model.IdDireccionNavigation, config => config.MapFrom(src => CreateDireccion(src.Calle, src.NumeroExterior, src.NumeroInterior, src.CodigoPostalId)))
                .ForMember(dto => dto.Activo, cfg => cfg.MapFrom(m => m.Status));
        }

        private static string FormatDireccion(Campus source)
        {
            if (source.IdDireccionNavigation == null)
                return "";

            var direccion = source.IdDireccionNavigation;
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(direccion.Calle))
                parts.Add(direccion.Calle);
            if (!string.IsNullOrWhiteSpace(direccion.NumeroExterior))
                parts.Add(direccion.NumeroExterior);
            if (!string.IsNullOrWhiteSpace(direccion.NumeroInterior))
                parts.Add(direccion.NumeroInterior);

            if (direccion.CodigoPostal != null)
            {
                if (!string.IsNullOrWhiteSpace(direccion.CodigoPostal.Asentamiento))
                    parts.Add(direccion.CodigoPostal.Asentamiento);
                if (direccion.CodigoPostal.Municipio != null)
                {
                    if (!string.IsNullOrWhiteSpace(direccion.CodigoPostal.Municipio.Nombre))
                        parts.Add(direccion.CodigoPostal.Municipio.Nombre);
                    if (direccion.CodigoPostal.Municipio.Estado != null &&
                        !string.IsNullOrWhiteSpace(direccion.CodigoPostal.Municipio.Estado.Abreviatura))
                        parts.Add(direccion.CodigoPostal.Municipio.Estado.Abreviatura);
                }
            }

            return string.Join(" ", parts);
        }

        private static Direccion? CreateDireccion(string? calle, string? numeroExterior, string? numeroInterior, int? codigoPostalId)
        {
            if (string.IsNullOrWhiteSpace(calle) ||
                string.IsNullOrWhiteSpace(numeroExterior) ||
                codigoPostalId == null)
                return null;

            return new Direccion
            {
                Calle = calle.Trim(),
                NumeroExterior = numeroExterior.Trim(),
                NumeroInterior = numeroInterior?.Trim(),
                CodigoPostalId = codigoPostalId.Value
            };
        }
    }
}
