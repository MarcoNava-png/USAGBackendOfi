using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.Requests.Requisitos;

namespace WebApplication2.Services.Interfaces
{
    public interface IAspiranteDocumentoService
    {
        Task<IReadOnlyList<DocumentoRequisitoDto>>ListarRequisitosAsync(ListarRequisitosRequest req);

        Task<IReadOnlyList<AspiranteDocumentoDto>>ListarEstadoAsync(ListarEstadoDocumentosRequest req);

        Task<long> CargarDocumentoAsync(CargarDocumentoRequestDto req);

        Task<long> CargarDocumentoConArchivoAsync(int idAspirante, int idDocumentoRequisito, IFormFile archivo, string? notas);

        Task<AspiranteDocumentoDto?> ObtenerDocumentoPorIdAsync(long idDocumento);

        Task<bool> ValidarDocumentoAsync(ValidarDocumentoRequestDto req);

        Task<bool> CambiarEstatusDocumentoAsync(long idDocumento, CambiarEstatusDocumentoDto dto);

        Task AsignarProrrogaAsync(long idAspiranteDocumento, DateTime fechaProrroga, string motivo, string usuarioId);

        Task AsignarProrrogaGlobalAsync(int idAspirante, DateTime fechaProrroga, string motivo, string usuarioId);

        Task<List<DocumentacionAspiranteResumenDto>> GetResumenDocumentacionAsync(string? filtroEstatus, string? busqueda, CancellationToken ct);

        Task<bool> ResetearDocumentoAsync(long idAspiranteDocumento);
    }
}
