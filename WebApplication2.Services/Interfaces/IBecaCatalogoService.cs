using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IBecaCatalogoService
    {
        Task<IReadOnlyList<Beca>> ObtenerTodasAsync(bool? soloActivas = null, CancellationToken ct = default);

        Task<Beca?> ObtenerPorIdAsync(int idBeca, CancellationToken ct = default);

        Task<Beca?> ObtenerPorClaveAsync(string clave, CancellationToken ct = default);

        Task<Beca> CrearAsync(
            string clave,
            string nombre,
            string? descripcion,
            string tipo,
            decimal valor,
            decimal? topeMensual,
            int? idConceptoPago,
            CancellationToken ct = default);

        Task<Beca> ActualizarAsync(
            int idBeca,
            string nombre,
            string? descripcion,
            string tipo,
            decimal valor,
            decimal? topeMensual,
            int? idConceptoPago,
            bool activo,
            CancellationToken ct = default);

        Task<bool> DesactivarAsync(int idBeca, CancellationToken ct = default);

        Task<bool> ActivarAsync(int idBeca, CancellationToken ct = default);

        Task<bool> ExisteClaveAsync(string clave, int? excluirId = null, CancellationToken ct = default);
    }
}
