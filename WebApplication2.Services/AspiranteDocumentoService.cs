using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Requisitos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class AspiranteDocumentoService : IAspiranteDocumentoService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public AspiranteDocumentoService(ApplicationDbContext db, IMapper mapper, IBlobStorageService blobStorageService, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<DocumentoRequisitoDto>> ListarRequisitosAsync(ListarRequisitosRequest req)
        {
            var requisitos = await _db.DocumentoRequisito
                                      .Where(r => r.Activo)
                                      .OrderBy(r => r.Orden)
                                      .AsNoTracking()
                                      .ToListAsync();

            return _mapper.Map<IReadOnlyList<DocumentoRequisitoDto>>(requisitos);
        }

        public async Task<IReadOnlyList<AspiranteDocumentoDto>> ListarEstadoAsync(ListarEstadoDocumentosRequest req)
        {
            var existe = await _db.AspiranteDocumento.AnyAsync(x => x.IdAspirante == req.IdAspirante);
            if (!existe)
            {
                // Get the aspirant's plan to check for plan-specific documents
                var aspirante = await _db.Aspirante.AsNoTracking().FirstOrDefaultAsync(a => a.IdAspirante == req.IdAspirante);
                var planDocs = aspirante != null
                    ? await _db.PlanDocumentoRequisito
                        .Where(pd => pd.IdPlanEstudios == aspirante.IdPlan)
                        .ToListAsync()
                    : new List<PlanDocumentoRequisito>();

                if (planDocs.Count > 0)
                {
                    // Use plan-specific documents
                    foreach (var pd in planDocs)
                    {
                        _db.AspiranteDocumento.Add(new AspiranteDocumento
                        {
                            IdAspirante = req.IdAspirante,
                            IdDocumentoRequisito = pd.IdDocumentoRequisito,
                            Estatus = EstatusDocumentoEnum.PENDIENTE
                        });
                    }
                }
                else
                {
                    // Fallback: use all active documents
                    var reqs = await _db.DocumentoRequisito.Where(r => r.Activo).ToListAsync();
                    foreach (var r in reqs)
                    {
                        _db.AspiranteDocumento.Add(new AspiranteDocumento
                        {
                            IdAspirante = req.IdAspirante,
                            IdDocumentoRequisito = r.IdDocumentoRequisito,
                            Estatus = EstatusDocumentoEnum.PENDIENTE
                        });
                    }
                }
                await _db.SaveChangesAsync();
            }

            var docs = await _db.AspiranteDocumento
                                .Include(d => d.Requisito)
                                .Where(d => d.IdAspirante == req.IdAspirante)
                                .OrderBy(d => d.Requisito!.Orden)
                                .AsNoTracking()
                                .ToListAsync();

            return _mapper.Map<IReadOnlyList<AspiranteDocumentoDto>>(docs);
        }

        public async Task<long> CargarDocumentoAsync(CargarDocumentoRequestDto req)
        {
            var doc = await _db.AspiranteDocumento
                .FirstOrDefaultAsync(x => x.IdAspirante == req.IdAspirante
                                       && x.IdDocumentoRequisito == req.IdDocumentoRequisito);

            if (doc == null)
            {
                doc = new AspiranteDocumento
                {
                    IdAspirante = req.IdAspirante,
                    IdDocumentoRequisito = req.IdDocumentoRequisito
                };
                _db.AspiranteDocumento.Add(doc);
            }

            doc.UrlArchivo = req.UrlArchivo;
            doc.Notas = req.Notas;
            doc.Estatus = EstatusDocumentoEnum.SUBIDO;
            doc.FechaSubidoUtc = System.DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return doc.IdAspiranteDocumento;
        }

        public async Task<bool> ValidarDocumentoAsync(ValidarDocumentoRequestDto req)
        {
            var doc = await _db.AspiranteDocumento.FirstOrDefaultAsync(x => x.IdAspiranteDocumento == req.IdAspiranteDocumento);
            if (doc == null) return false;

            doc.Estatus = req.Validar ? EstatusDocumentoEnum.VALIDADO : EstatusDocumentoEnum.RECHAZADO;
            if (!string.IsNullOrWhiteSpace(req.Notas)) doc.Notas = req.Notas;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CambiarEstatusDocumentoAsync(long idDocumento, CambiarEstatusDocumentoDto dto)
        {
            var doc = await _db.AspiranteDocumento.FirstOrDefaultAsync(x => x.IdAspiranteDocumento == idDocumento);
            if (doc == null) return false;

            doc.Estatus = dto.Estatus;
            if (!string.IsNullOrWhiteSpace(dto.Notas))
                doc.Notas = dto.Notas;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<long> CargarDocumentoConArchivoAsync(int idAspirante, int idDocumentoRequisito, IFormFile archivo, string? notas)
        {
            var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                throw new InvalidOperationException($"Extensión de archivo no permitida. Solo se permiten: {string.Join(", ", extensionesPermitidas)}");
            }

            const long maxFileSize = 10 * 1024 * 1024;
            if (archivo.Length > maxFileSize)
            {
                throw new InvalidOperationException("El archivo excede el tamaño máximo permitido de 10 MB");
            }

            var requisito = await _db.DocumentoRequisito.FindAsync(idDocumentoRequisito);
            if (requisito == null)
            {
                throw new InvalidOperationException("Requisito de documento no encontrado");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{idAspirante}/{requisito.Clave}_{timestamp}{extension}";

            var containerName = _configuration["Azure:DocumentosContainerName"] ?? "documentos";
            string urlArchivo;

            try
            {
                urlArchivo = await _blobStorageService.UploadFile(archivo, fileName, containerName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al subir el archivo: {ex.Message}", ex);
            }

            var request = new CargarDocumentoRequestDto
            {
                IdAspirante = idAspirante,
                IdDocumentoRequisito = idDocumentoRequisito,
                UrlArchivo = urlArchivo,
                Notas = notas
            };

            return await CargarDocumentoAsync(request);
        }

        public async Task<AspiranteDocumentoDto?> ObtenerDocumentoPorIdAsync(long idDocumento)
        {
            var doc = await _db.AspiranteDocumento
                .Include(d => d.Requisito)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdAspiranteDocumento == idDocumento);

            if (doc == null) return null;

            return _mapper.Map<AspiranteDocumentoDto>(doc);
        }

        public async Task AsignarProrrogaAsync(long idAspiranteDocumento, DateTime fechaProrroga, string motivo, string usuarioId)
        {
            var doc = await _db.AspiranteDocumento
                .FirstOrDefaultAsync(x => x.IdAspiranteDocumento == idAspiranteDocumento);

            if (doc == null)
                throw new InvalidOperationException("Documento de aspirante no encontrado.");

            doc.FechaProrroga = fechaProrroga;
            doc.MotivoProrroga = motivo;
            doc.UsuarioProrroga = usuarioId;
            doc.FechaProrrogaAsignada = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task AsignarProrrogaGlobalAsync(int idAspirante, DateTime fechaProrroga, string motivo, string usuarioId)
        {
            var docsPendientes = await _db.AspiranteDocumento
                .Where(d => d.IdAspirante == idAspirante && d.Estatus == EstatusDocumentoEnum.PENDIENTE)
                .ToListAsync();

            if (!docsPendientes.Any())
                throw new InvalidOperationException("No hay documentos pendientes para este aspirante.");

            foreach (var doc in docsPendientes)
            {
                doc.FechaProrroga = fechaProrroga;
                doc.MotivoProrroga = motivo;
                doc.UsuarioProrroga = usuarioId;
                doc.FechaProrrogaAsignada = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<DocumentacionAspiranteResumenDto>> GetResumenDocumentacionAsync(
            string? filtroEstatus, string? busqueda, CancellationToken ct)
        {
            var aspirantes = await _db.Set<Aspirante>()
                .Include(a => a.IdPersonaNavigation)
                .Include(a => a.IdPlanNavigation)
                .Include(a => a.Documentos)
                    .ThenInclude(d => d.Requisito)
                .AsNoTracking()
                .ToListAsync(ct);

            var ahora = DateTime.UtcNow;

            var resultado = new List<DocumentacionAspiranteResumenDto>();

            foreach (var asp in aspirantes)
            {
                var persona = asp.IdPersonaNavigation;
                if (persona == null) continue;

                var nombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim();

                // Check if aspirant has been enrolled as student (same IdPersona)
                string? matricula = null;
                if (asp.IdPersona.HasValue)
                {
                    matricula = await _db.Set<Estudiante>()
                        .Where(e => e.IdPersona == asp.IdPersona.Value)
                        .Select(e => e.Matricula)
                        .FirstOrDefaultAsync(ct);
                }

                var docs = asp.Documentos.ToList();
                var totalDocs = docs.Count;
                var docsCompletos = docs.Count(d => d.Estatus == EstatusDocumentoEnum.VALIDADO || d.Estatus == EstatusDocumentoEnum.SUBIDO);
                var docsPendientes = docs.Count(d => d.Estatus == EstatusDocumentoEnum.PENDIENTE);
                var docsConProrroga = docs.Count(d => d.FechaProrroga.HasValue && d.FechaProrroga.Value > ahora && d.Estatus == EstatusDocumentoEnum.PENDIENTE);
                var prorrogasVencidas = docs.Count(d => d.FechaProrroga.HasValue && d.FechaProrroga.Value <= ahora && d.Estatus == EstatusDocumentoEnum.PENDIENTE);

                var estatusGeneral = "INCOMPLETO";
                if (totalDocs > 0 && docsCompletos == totalDocs)
                    estatusGeneral = "COMPLETO";
                else if (prorrogasVencidas > 0)
                    estatusGeneral = "PRORROGA_VENCIDA";
                else if (docsConProrroga > 0)
                    estatusGeneral = "CON_PRORROGA";

                // Apply filters
                if (!string.IsNullOrEmpty(filtroEstatus) && estatusGeneral != filtroEstatus)
                    continue;

                if (!string.IsNullOrEmpty(busqueda))
                {
                    var busquedaLower = busqueda.ToLower();
                    if (!nombreCompleto.ToLower().Contains(busquedaLower) &&
                        !(matricula?.ToLower().Contains(busquedaLower) ?? false))
                        continue;
                }

                resultado.Add(new DocumentacionAspiranteResumenDto
                {
                    IdAspirante = asp.IdAspirante,
                    NombreCompleto = nombreCompleto,
                    Matricula = matricula,
                    PlanEstudios = asp.IdPlanNavigation?.NombrePlanEstudios ?? "",
                    TotalDocumentos = totalDocs,
                    DocumentosCompletos = docsCompletos,
                    DocumentosPendientes = docsPendientes,
                    DocumentosConProrroga = docsConProrroga,
                    ProrrogasVencidas = prorrogasVencidas,
                    EstatusGeneral = estatusGeneral,
                    Documentos = docs.Select(d => new AspiranteDocumentoDetalleDto
                    {
                        IdAspiranteDocumento = d.IdAspiranteDocumento,
                        Clave = d.Requisito?.Clave ?? "",
                        Descripcion = d.Requisito?.Descripcion ?? "",
                        EsObligatorio = d.Requisito?.EsObligatorio ?? false,
                        Estatus = d.Estatus.ToString(),
                        FechaSubida = d.FechaSubidoUtc,
                        FechaProrroga = d.FechaProrroga,
                        MotivoProrroga = d.MotivoProrroga,
                        ProrrogaVencida = d.FechaProrroga.HasValue && d.FechaProrroga.Value <= ahora && d.Estatus == EstatusDocumentoEnum.PENDIENTE,
                        UrlArchivo = d.UrlArchivo,
                        Notas = d.Notas
                    }).ToList()
                });
            }

            return resultado;
        }

        public async Task<bool> ResetearDocumentoAsync(long idAspiranteDocumento)
        {
            var doc = await _db.AspiranteDocumento
                .Include(d => d.Requisito)
                .FirstOrDefaultAsync(x => x.IdAspiranteDocumento == idAspiranteDocumento);

            if (doc == null) return false;

            if (!string.IsNullOrEmpty(doc.UrlArchivo))
            {
                try
                {
                    var containerName = _configuration["Azure:DocumentosContainerName"] ?? "documentos";
                    var baseUrl = _configuration["LocalStorage:BaseUrl"] ?? "/uploads";
                    var blobName = doc.UrlArchivo
                        .Replace($"{baseUrl}/{containerName}/", "")
                        .Replace($"https://", "");

                    if (blobName.Contains("/"))
                    {
                        var idx = blobName.IndexOf($"{doc.IdAspirante}/");
                        if (idx >= 0) blobName = blobName.Substring(idx);
                    }

                    await _blobStorageService.DeleteFile(blobName, containerName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: No se pudo eliminar el archivo fisico: {ex.Message}");
                }
            }

            doc.Estatus = EstatusDocumentoEnum.PENDIENTE;
            doc.UrlArchivo = null;
            doc.Notas = null;
            doc.FechaSubidoUtc = null;
            doc.FechaValidacion = null;
            doc.UsuarioValidacion = null;

            await _db.SaveChangesAsync();
            return true;
        }

    }
}
