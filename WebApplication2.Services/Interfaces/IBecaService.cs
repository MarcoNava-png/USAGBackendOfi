using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IBecaService
    {
        Task<decimal> CalcularDescuentoPorBecasAsync(
            int idEstudiante,
            int idConceptoPago,
            decimal importeBase,
            DateOnly fechaAplicacion,
            CancellationToken ct = default);

        Task<IReadOnlyList<BecaAsignacion>> ObtenerBecasActivasAsync(
            int idEstudiante,
            DateOnly fecha,
            int? idConceptoPago = null,
            CancellationToken ct = default);

        Task<BecaAsignacion> AsignarBecaAsync(
            int idEstudiante,
            int? idConceptoPago,
            string tipo,
            decimal valor,
            DateOnly vigenciaDesde,
            DateOnly? vigenciaHasta,
            decimal? topeMensual,
            string? observaciones,
            CancellationToken ct = default);

        Task<BecaAsignacion> AsignarBecaDesdeCatalogoAsync(
            int idEstudiante,
            int idBeca,
            DateOnly vigenciaDesde,
            DateOnly? vigenciaHasta,
            string? observaciones,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default);

        Task<BecaAsignacion?> ActualizarBecaAsignacionAsync(
            long idBecaAsignacion,
            DateOnly? vigenciaDesde,
            DateOnly? vigenciaHasta,
            string? observaciones,
            bool? activo,
            int? idPeriodoAcademico,
            CancellationToken ct = default);

        Task<BecaAsignacion?> ObtenerBecaPorIdAsync(long idBecaAsignacion, CancellationToken ct = default);

        Task<bool> CancelarBecaAsync(long idBecaAsignacion, CancellationToken ct = default);

        Task<IReadOnlyList<BecaAsignacion>> ObtenerBecasEstudianteAsync(
            int idEstudiante,
            bool? soloActivas = null,
            CancellationToken ct = default);

        Task<int> RecalcularDescuentosRecibosAsync(
            int idEstudiante,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default);
    }
}
