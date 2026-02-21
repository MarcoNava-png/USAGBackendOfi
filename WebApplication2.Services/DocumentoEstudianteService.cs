using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Documentos;
using WebApplication2.Core.Responses.Documentos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class DocumentoEstudianteService : IDocumentoEstudianteService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPdfService _pdfService;
        private readonly IBitacoraAccionService _bitacora;

        public DocumentoEstudianteService(
            ApplicationDbContext db,
            IConfiguration config,
            IPdfService pdfService,
            IBitacoraAccionService bitacora)
        {
            _db = db;
            _config = config;
            _pdfService = pdfService;
            _bitacora = bitacora;
        }

        #region Tipos de Documento

        public async Task<List<TipoDocumentoDto>> GetTiposDocumentoAsync()
        {
            return await _db.TiposDocumentoEstudiante
                .Where(t => t.Activo)
                .OrderBy(t => t.Orden)
                .Select(t => new TipoDocumentoDto
                {
                    IdTipoDocumento = t.IdTipoDocumento,
                    Clave = t.Clave,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Precio = t.Precio,
                    DiasVigencia = t.DiasVigencia,
                    RequierePago = t.RequierePago,
                    Activo = t.Activo
                })
                .ToListAsync();
        }

        public async Task<TipoDocumentoDto?> GetTipoDocumentoByIdAsync(int id)
        {
            return await _db.TiposDocumentoEstudiante
                .Where(t => t.IdTipoDocumento == id)
                .Select(t => new TipoDocumentoDto
                {
                    IdTipoDocumento = t.IdTipoDocumento,
                    Clave = t.Clave,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Precio = t.Precio,
                    DiasVigencia = t.DiasVigencia,
                    RequierePago = t.RequierePago,
                    Activo = t.Activo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoDocumentoDto?> GetTipoDocumentoByClaveAsync(string clave)
        {
            return await _db.TiposDocumentoEstudiante
                .Where(t => t.Clave == clave)
                .Select(t => new TipoDocumentoDto
                {
                    IdTipoDocumento = t.IdTipoDocumento,
                    Clave = t.Clave,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Precio = t.Precio,
                    DiasVigencia = t.DiasVigencia,
                    RequierePago = t.RequierePago,
                    Activo = t.Activo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoDocumentoEstudiante> CreateTipoDocumentoAsync(TipoDocumentoEstudiante tipo)
        {
            _db.TiposDocumentoEstudiante.Add(tipo);
            await _db.SaveChangesAsync();
            return tipo;
        }

        public async Task<TipoDocumentoEstudiante> UpdateTipoDocumentoAsync(TipoDocumentoEstudiante tipo)
        {
            _db.TiposDocumentoEstudiante.Update(tipo);
            await _db.SaveChangesAsync();
            return tipo;
        }

        public async Task DeleteTipoDocumentoAsync(int id)
        {
            var tipo = await _db.TiposDocumentoEstudiante.FindAsync(id);
            if (tipo != null)
            {
                tipo.Activo = false;
                await _db.SaveChangesAsync();
            }
        }

        #endregion

        #region Solicitudes

        public async Task<SolicitudDocumentoDto> CrearSolicitudAsync(CrearSolicitudDocumentoRequest request, string usuarioId)
        {
            // Validar request
            if (request.IdEstudiante <= 0)
                throw new InvalidOperationException($"IdEstudiante inválido: {request.IdEstudiante}");

            if (request.IdTipoDocumento <= 0)
                throw new InvalidOperationException($"IdTipoDocumento inválido: {request.IdTipoDocumento}");

            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == request.IdEstudiante);

            if (estudiante == null)
                throw new InvalidOperationException($"El estudiante con ID {request.IdEstudiante} no existe.");

            var tipoDoc = await _db.TiposDocumentoEstudiante
                .FirstOrDefaultAsync(t => t.IdTipoDocumento == request.IdTipoDocumento && t.Activo);

            if (tipoDoc == null)
                throw new InvalidOperationException($"El tipo de documento con ID {request.IdTipoDocumento} no existe o no está activo.");

            // Validar variante
            if (!Enum.TryParse<VarianteDocumento>(request.Variante, true, out var variante))
                throw new InvalidOperationException($"Variante inválida: '{request.Variante}'. Valores válidos: COMPLETO, PERIODO_ACTUAL, BASICO");

            try
            {
                var solicitud = new SolicitudDocumento
                {
                    FolioSolicitud = await GenerarFolioSolicitudAsync(),
                    IdEstudiante = request.IdEstudiante,
                    IdTipoDocumento = request.IdTipoDocumento,
                    Variante = variante,
                    Notas = request.Notas,
                    UsuarioSolicita = usuarioId,
                    Estatus = tipoDoc.RequierePago ? EstatusSolicitudDocumento.PENDIENTE_PAGO : EstatusSolicitudDocumento.PAGADO
                };

                if (tipoDoc.RequierePago && tipoDoc.Precio > 0)
                {
                    var recibo = await CrearReciboParaDocumentoAsync(estudiante, tipoDoc);
                    solicitud.IdRecibo = recibo.IdRecibo;
                }

                _db.SolicitudesDocumento.Add(solicitud);
                await _db.SaveChangesAsync();

                await _bitacora.RegistrarAsync(usuarioId, usuarioId, "CREAR_SOLICITUD", "Documentos",
                    "SolicitudDocumento", solicitud.IdSolicitud.ToString(),
                    $"Solicitud {solicitud.FolioSolicitud} creada para estudiante {request.IdEstudiante} - {tipoDoc.Nombre}");

                return await GetSolicitudByIdAsync(solicitud.IdSolicitud)
                    ?? throw new InvalidOperationException("Error al crear la solicitud.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Extraer el mensaje del inner exception para mejor diagnóstico
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Error de base de datos: {innerMessage}");
            }
        }

        private async Task<Recibo> CrearReciboParaDocumentoAsync(Estudiante estudiante, TipoDocumentoEstudiante tipoDoc)
        {
            // Buscar concepto de pago para documentos escolares
            var conceptoPago = await _db.ConceptoPago
                .FirstOrDefaultAsync(c => c.Clave == "DOC_ESCOLAR" && c.Activo);

            // Si no existe, buscar cualquier concepto activo como fallback
            if (conceptoPago == null)
            {
                conceptoPago = await _db.ConceptoPago
                    .FirstOrDefaultAsync(c => c.Activo);
            }

            if (conceptoPago == null)
            {
                throw new InvalidOperationException("No existe ningún concepto de pago activo en el sistema. Por favor, configure al menos un concepto de pago.");
            }

            var idConcepto = conceptoPago.IdConceptoPago;

            var año = DateTime.UtcNow.Year;
            var prefijo = $"REC-{año}-";
            var ultimoFolio = await _db.Recibo
                .Where(r => r.Folio != null && r.Folio.StartsWith(prefijo))
                .OrderByDescending(r => r.Folio)
                .Select(r => r.Folio)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimoFolio != null)
            {
                var numeroStr = ultimoFolio.Substring(prefijo.Length);
                if (int.TryParse(numeroStr, out int numero))
                    siguienteNumero = numero + 1;
            }

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var recibo = new Recibo
            {
                Folio = $"{prefijo}{siguienteNumero:D6}",
                IdEstudiante = estudiante.IdEstudiante,
                FechaEmision = hoy,
                FechaVencimiento = hoy.AddDays(30),
                Estatus = EstatusRecibo.PENDIENTE,
                Subtotal = tipoDoc.Precio,
                Saldo = tipoDoc.Precio,
                Notas = $"Pago por {tipoDoc.Nombre}"
            };

            _db.Recibo.Add(recibo);
            await _db.SaveChangesAsync();

            var detalle = new ReciboDetalle
            {
                IdRecibo = recibo.IdRecibo,
                IdConceptoPago = idConcepto,
                Descripcion = tipoDoc.Nombre,
                Cantidad = 1,
                PrecioUnitario = tipoDoc.Precio,
                RefTabla = "TipoDocumentoEstudiante",
                RefId = tipoDoc.IdTipoDocumento
            };

            _db.ReciboDetalle.Add(detalle);
            await _db.SaveChangesAsync();

            return recibo;
        }

        public async Task<SolicitudDocumentoDto?> GetSolicitudByIdAsync(long id)
        {
            return await _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.TipoDocumento)
                .Include(s => s.Recibo)
                .Where(s => s.IdSolicitud == id)
                .Select(s => MapToDto(s))
                .FirstOrDefaultAsync();
        }

        public async Task<SolicitudDocumentoDto?> GetSolicitudByCodigoVerificacionAsync(Guid codigo)
        {
            return await _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.TipoDocumento)
                .Include(s => s.Recibo)
                .Where(s => s.CodigoVerificacion == codigo)
                .Select(s => MapToDto(s))
                .FirstOrDefaultAsync();
        }

        public async Task<SolicitudesListResponse> GetSolicitudesAsync(SolicitudesFiltro filtro)
        {
            var query = _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.TipoDocumento)
                .Include(s => s.Recibo)
                .AsQueryable();

            if (filtro.IdEstudiante.HasValue)
                query = query.Where(s => s.IdEstudiante == filtro.IdEstudiante.Value);

            if (filtro.IdTipoDocumento.HasValue)
                query = query.Where(s => s.IdTipoDocumento == filtro.IdTipoDocumento.Value);

            if (!string.IsNullOrEmpty(filtro.Estatus) && Enum.TryParse<EstatusSolicitudDocumento>(filtro.Estatus, out var estatus))
                query = query.Where(s => s.Estatus == estatus);

            if (filtro.FechaDesde.HasValue)
                query = query.Where(s => s.FechaSolicitud >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(s => s.FechaSolicitud <= filtro.FechaHasta.Value);

            if (!string.IsNullOrEmpty(filtro.Busqueda))
            {
                var busqueda = filtro.Busqueda.ToLower();
                query = query.Where(s =>
                    s.FolioSolicitud.ToLower().Contains(busqueda) ||
                    s.Estudiante!.Matricula.ToLower().Contains(busqueda) ||
                    s.Estudiante.IdPersonaNavigation!.Nombre.ToLower().Contains(busqueda) ||
                    s.Estudiante.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(busqueda));
            }

            var total = await query.CountAsync();

            var solicitudes = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
                .Take(filtro.TamanoPagina)
                .Select(s => MapToDto(s))
                .ToListAsync();

            return new SolicitudesListResponse
            {
                Solicitudes = solicitudes,
                TotalRegistros = total,
                Pagina = filtro.Pagina,
                TamanoPagina = filtro.TamanoPagina,
                TotalPaginas = (int)Math.Ceiling((double)total / filtro.TamanoPagina)
            };
        }

        public async Task<List<SolicitudDocumentoDto>> GetSolicitudesByEstudianteAsync(int idEstudiante)
        {
            return await _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.TipoDocumento)
                .Include(s => s.Recibo)
                .Where(s => s.IdEstudiante == idEstudiante)
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => MapToDto(s))
                .ToListAsync();
        }

        public async Task ActualizarEstatusPagoAsync(long idRecibo)
        {
            // Verificar el estatus del recibo directamente en BD (evitar caché de EF Core
            // que puede tener datos obsoletos si el recibo se actualizó con raw SQL)
            var estatusRecibo = await _db.Recibo
                .AsNoTracking()
                .Where(r => r.IdRecibo == idRecibo)
                .Select(r => r.Estatus)
                .FirstOrDefaultAsync();

            if (estatusRecibo != EstatusRecibo.PAGADO)
                return;

            var solicitudes = await _db.SolicitudesDocumento
                .Where(s => s.IdRecibo == idRecibo && s.Estatus == EstatusSolicitudDocumento.PENDIENTE_PAGO)
                .ToListAsync();

            foreach (var sol in solicitudes)
            {
                sol.Estatus = EstatusSolicitudDocumento.PAGADO;
                sol.FechaModificacion = DateTime.UtcNow;
            }

            if (solicitudes.Any())
            {
                await _db.SaveChangesAsync();
            }
        }

        public async Task<SolicitudDocumentoDto> MarcarComoGeneradaAsync(long idSolicitud, string usuarioId)
        {
            var solicitud = await _db.SolicitudesDocumento
                .Include(s => s.TipoDocumento)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null)
                throw new InvalidOperationException("La solicitud no existe.");

            if (solicitud.Estatus != EstatusSolicitudDocumento.PAGADO && solicitud.Estatus != EstatusSolicitudDocumento.GENERADO)
                throw new InvalidOperationException("La solicitud debe estar pagada para poder generar el documento.");

            solicitud.FechaGeneracion = DateTime.UtcNow;
            solicitud.FechaVencimiento = DateTime.UtcNow.AddDays(solicitud.TipoDocumento?.DiasVigencia ?? 30);
            solicitud.Estatus = EstatusSolicitudDocumento.GENERADO;
            solicitud.UsuarioGenera = usuarioId;
            solicitud.VecesImpreso++;
            solicitud.FechaModificacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetSolicitudByIdAsync(idSolicitud)
                ?? throw new InvalidOperationException("Error al actualizar la solicitud.");
        }

        public async Task<SolicitudDocumentoDto> MarcarComoEntregadoAsync(long idSolicitud, string usuarioId)
        {
            var solicitud = await _db.SolicitudesDocumento
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null)
                throw new InvalidOperationException("La solicitud no existe.");

            if (solicitud.Estatus != EstatusSolicitudDocumento.GENERADO)
                throw new InvalidOperationException("Solo se pueden marcar como entregadas las solicitudes con estatus GENERADO.");

            solicitud.Estatus = EstatusSolicitudDocumento.ENTREGADO;
            solicitud.FechaEntrega = DateTime.UtcNow;
            solicitud.UsuarioEntrega = usuarioId;
            solicitud.FechaModificacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetSolicitudByIdAsync(idSolicitud)
                ?? throw new InvalidOperationException("Error al actualizar la solicitud.");
        }

        public async Task CancelarSolicitudAsync(long idSolicitud, string motivo, string usuarioId)
        {
            var solicitud = await _db.SolicitudesDocumento.FindAsync(idSolicitud);

            if (solicitud == null)
                throw new InvalidOperationException("La solicitud no existe.");

            if (solicitud.Estatus == EstatusSolicitudDocumento.GENERADO)
                throw new InvalidOperationException("No se puede cancelar una solicitud que ya ha sido generada.");

            solicitud.Estatus = EstatusSolicitudDocumento.CANCELADO;
            solicitud.Notas = $"{solicitud.Notas}\n[CANCELADO por {usuarioId}]: {motivo}";
            solicitud.FechaModificacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _bitacora.RegistrarAsync(usuarioId, usuarioId, "CANCELAR_SOLICITUD", "Documentos",
                "SolicitudDocumento", idSolicitud.ToString(),
                $"Solicitud {solicitud.FolioSolicitud} cancelada. Motivo: {motivo}");
        }

        #endregion

        #region Verificación

        public async Task<VerificacionDocumentoDto> VerificarDocumentoAsync(Guid codigoVerificacion)
        {
            var solicitud = await _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPlanActualNavigation)
                .Include(s => s.TipoDocumento)
                .FirstOrDefaultAsync(s => s.CodigoVerificacion == codigoVerificacion);

            if (solicitud == null)
            {
                return new VerificacionDocumentoDto
                {
                    EsValido = false,
                    EstaVigente = false,
                    Mensaje = "El documento no existe o el código de verificación es inválido."
                };
            }

            if (solicitud.Estatus != EstatusSolicitudDocumento.GENERADO)
            {
                return new VerificacionDocumentoDto
                {
                    EsValido = false,
                    EstaVigente = false,
                    Mensaje = "El documento aún no ha sido generado."
                };
            }

            var estaVigente = solicitud.FechaVencimiento.HasValue && solicitud.FechaVencimiento.Value > DateTime.UtcNow;

            var persona = solicitud.Estudiante?.IdPersonaNavigation;
            var nombreCompleto = persona != null
                ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                : "N/A";

            return new VerificacionDocumentoDto
            {
                EsValido = true,
                EstaVigente = estaVigente,
                Mensaje = estaVigente
                    ? "El documento es válido y se encuentra vigente."
                    : "El documento es válido pero ha expirado.",
                TipoDocumento = solicitud.TipoDocumento?.Nombre,
                NombreEstudiante = nombreCompleto,
                Matricula = solicitud.Estudiante?.Matricula,
                Carrera = solicitud.Estudiante?.IdPlanActualNavigation?.NombrePlanEstudios,
                FechaEmision = solicitud.FechaGeneracion,
                FechaVencimiento = solicitud.FechaVencimiento,
                FolioDocumento = solicitud.FolioSolicitud
            };
        }

        #endregion

        #region Generación de Documentos

        public async Task<KardexEstudianteDto> GenerarKardexAsync(int idEstudiante, bool soloPeriodoActual = false)
        {
            var estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Include(e => e.IdPlanActualNavigation)
                .Include(e => e.Inscripcion)
                    .ThenInclude(i => i.IdGrupoMateriaNavigation)
                        .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                            .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(e => e.Inscripcion)
                    .ThenInclude(i => i.IdGrupoMateriaNavigation)
                        .ThenInclude(gm => gm.IdGrupoNavigation)
                            .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante);

            if (estudiante == null)
                throw new InvalidOperationException("El estudiante no existe.");

            var persona = estudiante.IdPersonaNavigation;
            var plan = estudiante.IdPlanActualNavigation;

            var creditosTotales = await _db.MateriaPlan
                .Where(mp => mp.IdPlanEstudios == estudiante.IdPlanActual)
                .Include(mp => mp.IdMateriaNavigation)
                .SumAsync(mp => mp.IdMateriaNavigation.Creditos);

            var inscripcionesList = estudiante.Inscripcion.ToList();

            if (soloPeriodoActual)
            {
                var periodoActual = await _db.PeriodoAcademico
                    .Where(p => p.EsPeriodoActual)
                    .OrderByDescending(p => p.FechaInicio)
                    .FirstOrDefaultAsync();

                if (periodoActual != null)
                {
                    inscripcionesList = inscripcionesList
                        .Where(i => i.IdGrupoMateriaNavigation?.IdGrupoNavigation?.IdPeriodoAcademico == periodoActual.IdPeriodoAcademico)
                        .ToList();
                }
            }

            var periodos = inscripcionesList
                .Where(i => i.IdGrupoMateriaNavigation?.IdGrupoNavigation?.IdPeriodoAcademicoNavigation != null)
                .GroupBy(i => i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademicoNavigation)
                .OrderBy(g => g.Key!.FechaInicio)
                .Select(g => new KardexPeriodoDto
                {
                    Periodo = g.Key!.Nombre,
                    Ciclo = g.Key.Clave ?? "",
                    Materias = g.Select(i => new KardexMateriaDto
                    {
                        ClaveMateria = i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Clave ?? "",
                        NombreMateria = i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "",
                        Creditos = (int)(i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Creditos ?? 0),
                        CalificacionFinal = i.CalificacionFinal,
                        Estatus = DeterminarEstatusMateria(i),
                        TipoAcreditacion = "Ordinario"
                    }).ToList(),
                    PromedioPeriodo = g.Where(i => i.CalificacionFinal.HasValue).Select(i => i.CalificacionFinal!.Value).DefaultIfEmpty(0).Average(),
                    CreditosPeriodo = g.Sum(i => (int)(i.IdGrupoMateriaNavigation?.IdMateriaPlanNavigation?.IdMateriaNavigation?.Creditos ?? 0))
                })
                .ToList();

            var creditosCursados = periodos.Sum(p => p.CreditosPeriodo);
            var promedioGeneral = periodos
                .SelectMany(p => p.Materias)
                .Where(m => m.CalificacionFinal.HasValue)
                .Select(m => m.CalificacionFinal!.Value)
                .DefaultIfEmpty(0)
                .Average();

            return new KardexEstudianteDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
                Carrera = plan?.NombrePlanEstudios ?? "",
                PlanEstudios = plan?.NombrePlanEstudios ?? "",
                RVOE = plan?.RVOE,
                FechaIngreso = estudiante.FechaIngreso.ToDateTime(TimeOnly.MinValue),
                Estatus = estudiante.Activo ? "Activo" : "Inactivo",
                PromedioGeneral = promedioGeneral,
                CreditosCursados = creditosCursados,
                CreditosTotales = (int)creditosTotales,
                PorcentajeAvance = creditosTotales > 0 ? (creditosCursados / creditosTotales) * 100 : 0,
                Periodos = periodos
            };
        }

        private string DeterminarEstatusMateria(Inscripcion inscripcion)
        {
            if (!inscripcion.CalificacionFinal.HasValue)
                return "Cursando";

            var minAprobatoria = 7.0m;
            return inscripcion.CalificacionFinal.Value >= minAprobatoria ? "Aprobada" : "Reprobada";
        }

        public async Task<ConstanciaEstudiosDto> GenerarConstanciaAsync(long idSolicitud)
        {
            var solicitud = await _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPlanActualNavigation)
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPlanActualNavigation)
                        .ThenInclude(p => p.IdCampusNavigation)
                .Include(s => s.TipoDocumento)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null)
                throw new InvalidOperationException("La solicitud no existe.");

            var estudiante = solicitud.Estudiante!;
            var persona = estudiante.IdPersonaNavigation;
            var plan = estudiante.IdPlanActualNavigation;

            var incluyeMaterias = solicitud.Variante != VarianteDocumento.BASICO;

            var materias = new List<ConstanciaMateriaDto>();

            if (incluyeMaterias)
            {
                var periodoActual = await _db.PeriodoAcademico
                    .Where(p => p.EsPeriodoActual)
                    .OrderByDescending(p => p.FechaInicio)
                    .FirstOrDefaultAsync();

                if (periodoActual != null)
                {
                    materias = await _db.Inscripcion
                        .Include(i => i.IdGrupoMateriaNavigation)
                            .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                                .ThenInclude(mp => mp.IdMateriaNavigation)
                        .Include(i => i.IdGrupoMateriaNavigation)
                            .ThenInclude(gm => gm.IdProfesorNavigation)
                                .ThenInclude(p => p.IdPersonaNavigation)
                        .Include(i => i.IdGrupoMateriaNavigation)
                            .ThenInclude(gm => gm.Horario)
                                .ThenInclude(h => h.IdDiaSemanaNavigation)
                        .Where(i => i.IdEstudiante == estudiante.IdEstudiante &&
                                   i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademico == periodoActual.IdPeriodoAcademico)
                        .Select(i => new ConstanciaMateriaDto
                        {
                            ClaveMateria = i.IdGrupoMateriaNavigation!.IdMateriaPlanNavigation!.IdMateriaNavigation!.Clave,
                            NombreMateria = i.IdGrupoMateriaNavigation.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                            Profesor = i.IdGrupoMateriaNavigation.IdProfesorNavigation != null
                                ? $"{i.IdGrupoMateriaNavigation.IdProfesorNavigation.IdPersonaNavigation!.Nombre} {i.IdGrupoMateriaNavigation.IdProfesorNavigation.IdPersonaNavigation.ApellidoPaterno}"
                                : "N/A",
                            Horario = string.Join(", ", i.IdGrupoMateriaNavigation.Horario
                                .Select(h => $"{h.IdDiaSemanaNavigation!.Nombre} {h.HoraInicio}-{h.HoraFin}"))
                        })
                        .ToListAsync();
                }
            }

            var periodoNombre = (await _db.PeriodoAcademico
                .Where(p => p.EsPeriodoActual)
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync())?.Nombre ?? "N/A";

            return new ConstanciaEstudiosDto
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
                Carrera = plan?.NombrePlanEstudios ?? "",
                PlanEstudios = plan?.NombrePlanEstudios ?? "",
                RVOE = plan?.RVOE,
                PeriodoActual = periodoNombre,
                Grado = "N/A",
                Turno = "Matutino",
                Campus = plan?.IdCampusNavigation?.Nombre ?? "",
                FechaIngreso = estudiante.FechaIngreso.ToDateTime(TimeOnly.MinValue),
                IncluyeMaterias = incluyeMaterias,
                Materias = materias,
                FechaEmision = solicitud.FechaGeneracion ?? DateTime.UtcNow,
                FechaVencimiento = solicitud.FechaVencimiento ?? DateTime.UtcNow.AddDays(solicitud.TipoDocumento?.DiasVigencia ?? 30),
                FolioDocumento = solicitud.FolioSolicitud,
                CodigoVerificacion = solicitud.CodigoVerificacion,
                UrlVerificacion = GenerarUrlVerificacion(solicitud.CodigoVerificacion)
            };
        }

        public async Task<byte[]> GenerarKardexPdfAsync(long idSolicitud)
        {
            var solicitud = await _db.SolicitudesDocumento
                .Include(s => s.TipoDocumento)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null)
                throw new InvalidOperationException("La solicitud no existe.");

            var esHistoricoCompleto = solicitud.Variante != VarianteDocumento.PERIODO_ACTUAL;
            var kardex = await GenerarKardexAsync(solicitud.IdEstudiante, !esHistoricoCompleto);

            return await _pdfService.GenerarKardexPdf(kardex, solicitud.FolioSolicitud,
                solicitud.CodigoVerificacion, GenerarUrlVerificacion(solicitud.CodigoVerificacion));
        }

        public async Task<byte[]> GenerarConstanciaPdfAsync(long idSolicitud)
        {
            var constancia = await GenerarConstanciaAsync(idSolicitud);
            return await _pdfService.GenerarConstanciaPdf(constancia);
        }

        #endregion

        #region Utilidades

        public async Task<string> GenerarFolioSolicitudAsync()
        {
            var año = DateTime.UtcNow.Year;
            var prefijo = $"DOC-{año}-";

            var ultimoFolio = await _db.SolicitudesDocumento
                .Where(s => s.FolioSolicitud.StartsWith(prefijo))
                .OrderByDescending(s => s.FolioSolicitud)
                .Select(s => s.FolioSolicitud)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimoFolio != null)
            {
                var numeroStr = ultimoFolio.Substring(prefijo.Length);
                if (int.TryParse(numeroStr, out int numero))
                    siguienteNumero = numero + 1;
            }

            return $"{prefijo}{siguienteNumero:D5}";
        }

        public string GenerarUrlVerificacion(Guid codigoVerificacion)
        {
            var baseUrl = _config["App:UrlVerificacionDocumentos"]
                ?? "https://usag.edu.mx/verificar";
            return $"{baseUrl}/{codigoVerificacion}";
        }

        public async Task<bool> PuedeGenerarAsync(long idSolicitud)
        {
            var solicitud = await _db.SolicitudesDocumento.FindAsync(idSolicitud);
            return solicitud?.Estatus == EstatusSolicitudDocumento.PAGADO
                || solicitud?.Estatus == EstatusSolicitudDocumento.GENERADO;
        }

        public async Task ActualizarDocumentosVencidosAsync()
        {
            var documentosVencidos = await _db.SolicitudesDocumento
                .Where(s => s.Estatus == EstatusSolicitudDocumento.GENERADO
                         && s.FechaVencimiento.HasValue
                         && s.FechaVencimiento.Value < DateTime.UtcNow)
                .ToListAsync();

            foreach (var doc in documentosVencidos)
            {
                doc.Estatus = EstatusSolicitudDocumento.VENCIDO;
                doc.FechaModificacion = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        private static SolicitudDocumentoDto MapToDto(SolicitudDocumento s)
        {
            var persona = s.Estudiante?.IdPersonaNavigation;
            var ahora = DateTime.UtcNow;

            return new SolicitudDocumentoDto
            {
                IdSolicitud = s.IdSolicitud,
                FolioSolicitud = s.FolioSolicitud,
                IdEstudiante = s.IdEstudiante,
                NombreEstudiante = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "",
                Matricula = s.Estudiante?.Matricula ?? "",
                IdTipoDocumento = s.IdTipoDocumento,
                TipoDocumentoNombre = s.TipoDocumento?.Nombre ?? "",
                TipoDocumentoClave = s.TipoDocumento?.Clave ?? "",
                IdRecibo = s.IdRecibo,
                FolioRecibo = s.Recibo?.Folio,
                Variante = s.Variante.ToString(),
                FechaSolicitud = s.FechaSolicitud,
                FechaGeneracion = s.FechaGeneracion,
                FechaVencimiento = s.FechaVencimiento,
                Estatus = s.Estatus.ToString(),
                CodigoVerificacion = s.CodigoVerificacion,
                UrlVerificacion = "",
                VecesImpreso = s.VecesImpreso,
                Notas = s.Notas,
                Precio = s.TipoDocumento?.Precio,
                EstaVigente = s.FechaVencimiento.HasValue && s.FechaVencimiento.Value > ahora,
                PuedeGenerar = s.Estatus == EstatusSolicitudDocumento.PAGADO || s.Estatus == EstatusSolicitudDocumento.GENERADO
            };
        }

        #endregion

        #region Panel Control Escolar

        public async Task<SolicitudesPendientesDto> GetSolicitudesParaControlEscolarAsync(
            string? filtroEstatus = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? busqueda = null,
            CancellationToken ct = default)
        {
            // Auto-sincronizar: solicitudes PENDIENTE_PAGO cuyo recibo ya está PAGADO
            var desincronizadas = await _db.SolicitudesDocumento
                .Include(s => s.Recibo)
                .Where(s => s.Estatus == EstatusSolicitudDocumento.PENDIENTE_PAGO
                         && s.IdRecibo != null
                         && s.Recibo!.Estatus == EstatusRecibo.PAGADO)
                .ToListAsync(ct);

            if (desincronizadas.Any())
            {
                foreach (var sol in desincronizadas)
                {
                    sol.Estatus = EstatusSolicitudDocumento.PAGADO;
                    sol.FechaModificacion = DateTime.UtcNow;
                }
                await _db.SaveChangesAsync(ct);
            }

            var query = _db.SolicitudesDocumento
                .Include(s => s.Estudiante)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(s => s.TipoDocumento)
                .Include(s => s.Recibo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtroEstatus) && Enum.TryParse<EstatusSolicitudDocumento>(filtroEstatus, out var estatus))
            {
                query = query.Where(s => s.Estatus == estatus);
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(s => s.FechaSolicitud >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(s => s.FechaSolicitud <= fechaHasta.Value);
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                query = query.Where(s =>
                    s.FolioSolicitud.ToLower().Contains(busquedaLower) ||
                    s.Estudiante!.Matricula.ToLower().Contains(busquedaLower) ||
                    s.Estudiante.IdPersonaNavigation!.Nombre.ToLower().Contains(busquedaLower) ||
                    s.Estudiante.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(busquedaLower));
            }

            var todasSolicitudes = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync(ct);

            var resultado = new SolicitudesPendientesDto
            {
                TotalPendientesPago = todasSolicitudes.Count(s => s.Estatus == EstatusSolicitudDocumento.PENDIENTE_PAGO),
                TotalListosGenerar = todasSolicitudes.Count(s => s.Estatus == EstatusSolicitudDocumento.PAGADO),
                TotalGenerados = todasSolicitudes.Count(s => s.Estatus == EstatusSolicitudDocumento.GENERADO),
                TotalVencidos = todasSolicitudes.Count(s => s.Estatus == EstatusSolicitudDocumento.VENCIDO),
                TotalCancelados = todasSolicitudes.Count(s => s.Estatus == EstatusSolicitudDocumento.CANCELADO),
                TotalEntregados = todasSolicitudes.Count(s => s.Estatus == EstatusSolicitudDocumento.ENTREGADO),
                Solicitudes = todasSolicitudes.Select(s => MapToResumenDto(s)).ToList()
            };

            // Resolver nombres de usuarios (UsuarioSolicita, UsuarioGenera, UsuarioEntrega son GUIDs)
            var userIds = resultado.Solicitudes
                .SelectMany(s => new[] { s.UsuarioSolicita, s.UsuarioGenera, s.UsuarioEntrega })
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (userIds.Any())
            {
                var nombresUsuarios = await _db.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => $"{u.Nombres} {u.Apellidos}".Trim(), ct);

                foreach (var sol in resultado.Solicitudes)
                {
                    if (sol.UsuarioSolicita != null && nombresUsuarios.TryGetValue(sol.UsuarioSolicita, out var nombreSolicita))
                        sol.UsuarioSolicita = nombreSolicita;
                    if (sol.UsuarioGenera != null && nombresUsuarios.TryGetValue(sol.UsuarioGenera, out var nombreGenera))
                        sol.UsuarioGenera = nombreGenera;
                    if (sol.UsuarioEntrega != null && nombresUsuarios.TryGetValue(sol.UsuarioEntrega, out var nombreEntrega))
                        sol.UsuarioEntrega = nombreEntrega;
                }
            }

            return resultado;
        }

        public async Task<int> GetContadorSolicitudesListasAsync(CancellationToken ct = default)
        {
            return await _db.SolicitudesDocumento
                .CountAsync(s => s.Estatus == EstatusSolicitudDocumento.PAGADO, ct);
        }

        private static SolicitudResumenDto MapToResumenDto(SolicitudDocumento s)
        {
            var persona = s.Estudiante?.IdPersonaNavigation;

            return new SolicitudResumenDto
            {
                IdSolicitud = s.IdSolicitud,
                FolioSolicitud = s.FolioSolicitud,
                IdEstudiante = s.IdEstudiante,
                Matricula = s.Estudiante?.Matricula ?? "",
                NombreEstudiante = persona != null
                    ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                    : "",
                IdTipoDocumento = s.IdTipoDocumento,
                TipoDocumento = s.TipoDocumento?.Nombre ?? "",
                TipoDocumentoClave = s.TipoDocumento?.Clave ?? "",
                Variante = s.Variante.ToString(),
                Estatus = s.Estatus.ToString(),
                FechaSolicitud = s.FechaSolicitud,
                FechaGeneracion = s.FechaGeneracion,
                FechaVencimiento = s.FechaVencimiento,
                PrecioDocumento = s.TipoDocumento?.Precio,
                IdRecibo = s.IdRecibo,
                FolioRecibo = s.Recibo?.Folio,
                EstatusRecibo = s.Recibo?.Estatus.ToString(),
                PuedeGenerar = s.Estatus == EstatusSolicitudDocumento.PAGADO || s.Estatus == EstatusSolicitudDocumento.GENERADO,
                UsuarioSolicita = s.UsuarioSolicita,
                UsuarioGenera = s.UsuarioGenera,
                FechaEntrega = s.FechaEntrega,
                UsuarioEntrega = s.UsuarioEntrega,
                PuedeMarcarEntregado = s.Estatus == EstatusSolicitudDocumento.GENERADO
            };
        }

        #endregion
    }
}
