using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Convenio;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IConvenioService
    {
        Task<IReadOnlyList<ConvenioDto>> ListarConveniosAsync(
            bool? soloActivos = null,
            int? idCampus = null,
            int? idPlanEstudios = null,
            CancellationToken ct = default);

        Task<ConvenioDto?> ObtenerPorIdAsync(int idConvenio, CancellationToken ct = default);

        Task<ConvenioDto> CrearConvenioAsync(CrearConvenioDto dto, string usuarioCreador, CancellationToken ct = default);

        Task<ConvenioDto> ActualizarConvenioAsync(int idConvenio, ActualizarConvenioDto dto, string usuarioModificador, CancellationToken ct = default);

        Task<bool> EliminarConvenioAsync(int idConvenio, CancellationToken ct = default);

        Task<bool> CambiarEstadoConvenioAsync(int idConvenio, bool activo, CancellationToken ct = default);

        Task<IReadOnlyList<ConvenioDisponibleDto>> ObtenerConveniosDisponiblesParaAspiranteAsync(
            int idAspirante,
            CancellationToken ct = default);

        Task<AspiranteConvenioDto> AsignarConvenioAspiranteAsync(
            AsignarConvenioAspiranteDto dto,
            string usuarioCreador,
            CancellationToken ct = default);

        Task<IReadOnlyList<AspiranteConvenioDto>> ObtenerConveniosAspiranteAsync(
            int idAspirante,
            CancellationToken ct = default);

        Task<bool> CambiarEstatusConvenioAspiranteAsync(
            int idAspiranteConvenio,
            string nuevoEstatus,
            CancellationToken ct = default);

        Task<bool> EliminarConvenioAspiranteAsync(int idAspiranteConvenio, CancellationToken ct = default);

        Task<CalculoDescuentoConvenioDto> CalcularDescuentoConvenioAsync(
            int idConvenio,
            decimal montoOriginal,
            CancellationToken ct = default);

        Task<decimal> CalcularDescuentoTotalAspiranteAsync(
            int idAspirante,
            decimal montoOriginal,
            string? tipoConcepto = null,
            CancellationToken ct = default);

        Task IncrementarAplicacionesConvenioAsync(
            int idAspirante,
            string? tipoConcepto = null,
            CancellationToken ct = default);
    }
}
