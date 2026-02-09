using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;

namespace WebApplication2.Services.Interfaces
{
    public interface IConceptoService
    {
        Task<ConceptoDto> CrearConceptoAsync(CrearConceptoDto dto, CancellationToken ct);
        Task<PrecioDto> CrearPrecioAsync(CrearPrecioDto dto, CancellationToken ct);
        Task<IReadOnlyList<ConceptoDto>> ListarAsync(bool? soloActivos, int? tipo, string? busqueda, CancellationToken ct);
        Task<IReadOnlyList<PrecioDto>> ListarPreciosAsync(int idConceptoPago, CancellationToken ct);
        Task<ConceptoDto?> ObtenerPorIdAsync(int id, CancellationToken ct);
        Task<ConceptoDto> ActualizarAsync(int id, ActualizarConceptoDto dto, CancellationToken ct);
        Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct);
        Task EliminarAsync(int id, CancellationToken ct);
    }
}
