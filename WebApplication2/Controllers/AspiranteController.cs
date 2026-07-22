using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Admision;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Aspirante;
using WebApplication2.Core.Requests.Requisitos;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.ADMISIONES},{Rol.ACADEMICO}")]
    public class AspiranteController : ControllerBase
    {
        private readonly IAspiranteService _aspiranteService;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IAspiranteDocumentoService _docsSvc;
        private readonly IReciboService _recibosSvc;
        private readonly IPdfService _pdfService;
        private readonly IComprobanteInscripcionService _comprobanteSvc;
        private readonly Data.DbContexts.ApplicationDbContext _dbContext;

        public AspiranteController(
            IAspiranteService aspiranteService,
            IMapper mapper,
            IAuthService authService,
            IAspiranteDocumentoService docsSvc,
            IReciboService recibosSvc,
            IPdfService pdfService,
            IComprobanteInscripcionService comprobanteSvc,
            Data.DbContexts.ApplicationDbContext dbContext)
        {
            _aspiranteService = aspiranteService;
            _mapper = mapper;
            _authService = authService;
            _dbContext = dbContext;
            _docsSvc = docsSvc;
            _recibosSvc = recibosSvc;
            _pdfService = pdfService;
            _comprobanteSvc = comprobanteSvc;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<AspiranteDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string filter = "",
            [FromQuery] string? registradoPor = null,
            [FromQuery] int? idPeriodoAcademico = null,
            [FromQuery] bool soloSinPeriodo = false,
            [FromQuery] string? estatus = null,
            [FromQuery] DateOnly? fechaRegistroDesde = null,
            [FromQuery] DateOnly? fechaRegistroHasta = null,
            [FromQuery] string? estatusPago = null,
            [FromQuery] string? estatusDocumentos = null,
            [FromQuery] string? idPlan = null,
            [FromQuery] string? accionTipo = null,
            [FromQuery] bool soloOcultos = false,
            CancellationToken ct = default)
        {
            List<string>? Parse(string? s) => string.IsNullOrWhiteSpace(s)
                ? null
                : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            List<int>? ParseInts(string? s) => string.IsNullOrWhiteSpace(s)
                ? null
                : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => int.TryParse(x, out var n) ? n : (int?)null)
                    .Where(n => n.HasValue).Select(n => n!.Value).ToList();

            int? idCampusRestringido = null;
            var currentUserId = User.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(currentUserId) && (User.IsInRole(Rol.DIRECTOR) || User.IsInRole(Rol.COORDINADOR)))
            {
                var currentUser = await _authService.GetUserById(currentUserId);
                idCampusRestringido = currentUser?.IdCampusAsignado;
            }

            var pagination = await _aspiranteService.GetAspirantes(
                page, pageSize, filter, registradoPor, idPeriodoAcademico, soloSinPeriodo,
                Parse(estatus), fechaRegistroDesde, fechaRegistroHasta,
                Parse(estatusPago), Parse(estatusDocumentos),
                ParseInts(idPlan), accionTipo, idCampusRestringido, soloOcultos);

            var aspirantesDtos = _mapper.Map<List<AspiranteDto>>(pagination.Items);

            var aspiranteIds = aspirantesDtos.Select(a => a.IdAspirante).ToList();
            var userIds = aspirantesDtos
                .SelectMany(a => new[] { a.IdAtendidoPorUsuario, a.CreatedBy })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id!)
                .Distinct()
                .ToList();

            var userCache = new Dictionary<string, string?>();
            foreach (var uid in userIds)
            {
                try
                {
                    var u = await _authService.GetUserById(uid);
                    if (u == null) { userCache[uid] = null; continue; }
                    var nombre = $"{u.Nombres} {u.Apellidos}".Trim();
                    userCache[uid] = !string.IsNullOrWhiteSpace(nombre)
                        ? nombre
                        : (!string.IsNullOrWhiteSpace(u.UserName) ? u.UserName : u.Email);
                }
                catch
                {
                    userCache[uid] = null;
                }
            }

            var docsByAspirante = await _dbContext.AspiranteDocumento
                .Where(d => aspiranteIds.Contains(d.IdAspirante))
                .AsNoTracking()
                .Select(d => new { d.IdAspirante, d.Estatus })
                .ToListAsync(ct);

            var docsGroup = docsByAspirante
                .GroupBy(d => d.IdAspirante)
                .ToDictionary(g => g.Key, g => g.Select(d => d.Estatus).ToList());

            var recibosByAspirante = await _dbContext.Recibo
                .Where(r => r.IdAspirante != null
                            && aspiranteIds.Contains(r.IdAspirante.Value)
                            && r.Status == Core.Enums.StatusEnum.Active)
                .AsNoTracking()
                .GroupBy(r => r.IdAspirante!.Value)
                .Select(g => new
                {
                    IdAspirante = g.Key,
                    TotalSaldo = g.Sum(r => r.Saldo),
                    TotalGeneral = g.Sum(r => r.Total),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.IdAspirante, ct);

            foreach (var aspiranteDto in aspirantesDtos)
            {
                if (!string.IsNullOrWhiteSpace(aspiranteDto.IdAtendidoPorUsuario)
                    && userCache.TryGetValue(aspiranteDto.IdAtendidoPorUsuario, out var atiende))
                {
                    aspiranteDto.UsuarioAtiendeNombre = atiende;
                }

                if (!string.IsNullOrWhiteSpace(aspiranteDto.CreatedBy)
                    && userCache.TryGetValue(aspiranteDto.CreatedBy, out var registro))
                {
                    aspiranteDto.UsuarioRegistroNombre = registro;
                }

                if (docsGroup.TryGetValue(aspiranteDto.IdAspirante, out var estatuses) && estatuses.Count > 0)
                {
                    if (estatuses.All(e => e == EstatusDocumentoEnum.VALIDADO))
                        aspiranteDto.EstatusDocumentos = "VALIDADO";
                    else if (estatuses.All(e => e == EstatusDocumentoEnum.SUBIDO || e == EstatusDocumentoEnum.VALIDADO))
                        aspiranteDto.EstatusDocumentos = "COMPLETO";
                    else
                        aspiranteDto.EstatusDocumentos = "INCOMPLETO";
                }
                else
                {
                    aspiranteDto.EstatusDocumentos = "INCOMPLETO";
                }

                if (recibosByAspirante.TryGetValue(aspiranteDto.IdAspirante, out var totales) && totales.Count > 0)
                {
                    if (totales.TotalSaldo == 0)
                        aspiranteDto.EstatusPago = "PAGADO";
                    else if (totales.TotalSaldo < totales.TotalGeneral)
                        aspiranteDto.EstatusPago = "PARCIAL";
                    else
                        aspiranteDto.EstatusPago = "PENDIENTE";
                }
                else
                {
                    aspiranteDto.EstatusPago = "SIN_RECIBO";
                }
            }

            var response = new PagedResult<AspiranteDto>
            {
                TotalItems = pagination.TotalItems,
                Items = aspirantesDtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("contadores")]
        public async Task<ActionResult<Dictionary<string, int>>> GetContadores()
        {
            var contadores = await _aspiranteService.GetContadoresAsync();
            return Ok(contadores);
        }

        [HttpGet("asesores")]
        public async Task<ActionResult> GetAsesores()
        {
            var creadorIds = await _dbContext.Aspirante
                .Where(a => a.Status != Core.Enums.StatusEnum.Deleted && a.CreatedBy != null)
                .Select(a => a.CreatedBy!)
                .Distinct()
                .ToListAsync();

            var asesores = new List<object>();
            foreach (var id in creadorIds)
            {
                var user = await _authService.GetUserById(id);
                if (user != null)
                {
                    var nombre = $"{user.Nombres} {user.Apellidos}".Trim();
                    asesores.Add(new { id, nombre = string.IsNullOrWhiteSpace(nombre) ? user.Email : nombre });
                }
            }

            return Ok(asesores.OrderBy(a => ((dynamic)a).nombre));
        }

        [HttpGet("debug/{id}")]
        public async Task<ActionResult<object>> GetDebug(int id, CancellationToken ct = default)
        {
            var aspirante = await _aspiranteService.GetAspiranteById(id);

            var documentos = await _docsSvc.ListarEstadoAsync(new ListarEstadoDocumentosRequest { IdAspirante = id });
            string estatusDocumentos;
            if (documentos.Count == 0)
            {
                estatusDocumentos = "INCOMPLETO";
            }
            else
            {
                var todosValidados = documentos.All(d => d.Estatus == EstatusDocumentoEnum.VALIDADO);
                var todosSubidos = documentos.All(d => d.Estatus == EstatusDocumentoEnum.SUBIDO || d.Estatus == EstatusDocumentoEnum.VALIDADO);

                if (todosValidados)
                {
                    estatusDocumentos = "VALIDADO";
                }
                else if (todosSubidos)
                {
                    estatusDocumentos = "COMPLETO";
                }
                else
                {
                    estatusDocumentos = "INCOMPLETO";
                }
            }

            var recibos = await _recibosSvc.ListarPorAspiranteAsync(id, ct);
            string estatusPago;
            if (recibos.Count == 0)
            {
                estatusPago = "SIN_RECIBO";
            }
            else
            {
                var totalSaldo = recibos.Sum(r => r.Saldo);
                var totalGeneral = recibos.Sum(r => r.Total);

                if (totalSaldo == 0)
                {
                    estatusPago = "PAGADO";
                }
                else if (totalSaldo < totalGeneral)
                {
                    estatusPago = "PARCIAL";
                }
                else
                {
                    estatusPago = "PENDIENTE";
                }
            }

            return Ok(new
            {
                IdAspirante = aspirante.IdAspirante,
                CreatedBy = aspirante.CreatedBy,
                CreatedAt = aspirante.CreatedAt,
                UpdatedBy = aspirante.UpdatedBy,
                UpdatedAt = aspirante.UpdatedAt,
                IdAtendidoPorUsuario = aspirante.IdAtendidoPorUsuario,
                EstatusDocumentos = estatusDocumentos,
                EstatusPago = estatusPago,
                CantidadDocumentos = documentos.Count,
                CantidadRecibos = recibos.Count,
                Recibos = recibos.Select(r => new {
                    r.IdRecibo,
                    r.Folio,
                    r.Total,
                    r.Saldo,
                    Estatus = r.estatus.ToString()
                })
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AspiranteDto>> GetById(int id)
        {
            var aspirante = await _aspiranteService.GetAspiranteById(id);

            var aspiranteDto = _mapper.Map<AspiranteDto>(aspirante);

            if (!string.IsNullOrEmpty(aspiranteDto.IdAtendidoPorUsuario))
            {
                var usuarioAtiende = await _authService.GetUserById(aspiranteDto.IdAtendidoPorUsuario);

                aspiranteDto.UsuarioAtiendeNombre = ($"{usuarioAtiende.Nombres} {usuarioAtiende.Apellidos}").Trim();
            }

            return Ok(aspiranteDto);
        }

        [HttpGet("{id:int}/editar")]
        public async Task<ActionResult<AspiranteEditDto>> GetForEdit(int id)
        {
            var aspirante = await _aspiranteService.GetAspiranteById(id);
            var persona = aspirante.IdPersonaNavigation;
            var direccion = persona?.IdDireccionNavigation;

            var dto = new AspiranteEditDto
            {
                IdAspirante = aspirante.IdAspirante,
                Nombre = persona?.Nombre,
                ApellidoPaterno = persona?.ApellidoPaterno,
                ApellidoMaterno = persona?.ApellidoMaterno,
                FechaNacimiento = persona?.FechaNacimiento,
                GeneroId = persona?.IdGenero,
                Correo = persona?.Correo,
                Telefono = persona?.Telefono,
                Celular = persona?.Celular,
                CURP = persona?.Curp,
                Calle = direccion?.Calle,
                NumeroExterior = direccion?.NumeroExterior,
                NumeroInterior = direccion?.NumeroInterior,
                CodigoPostalId = direccion?.CodigoPostalId,
                IdEstadoCivil = persona?.IdEstadoCivil,
                Nacionalidad = persona?.Nacionalidad,
                CampusId = aspirante.IdPlanNavigation?.IdCampus,
                PlanEstudiosId = aspirante.IdPlan,
                MedioContactoId = aspirante.IdMedioContacto,
                Notas = aspirante.Observaciones,
                HorarioId = aspirante.TurnoId,
                CuatrimestreInteres = aspirante.CuatrimestreInteres,
                InstitucionProcedencia = aspirante.InstitucionProcedencia,
                IdModalidad = aspirante.IdModalidad,
                GrupoDiasImparticion = aspirante.GrupoDiasImparticion,
                IdPeriodoAcademico = aspirante.IdPeriodoAcademico,
                RecorridoPlantel = aspirante.RecorridoPlantel,
                Trabaja = aspirante.Trabaja,
                NombreEmpresa = aspirante.NombreEmpresa,
                DomicilioEmpresa = aspirante.DomicilioEmpresa,
                PuestoEmpresa = aspirante.PuestoEmpresa,
                QuienCubreGastos = aspirante.QuienCubreGastos,
                IdEmpresa = aspirante.IdEmpresa,
                AtendidoPorUsuarioId = aspirante.IdAtendidoPorUsuario,
                NombreContactoEmergencia = persona?.NombreContactoEmergencia,
                TelefonoContactoEmergencia = persona?.TelefonoContactoEmergencia,
                ParentescoContactoEmergencia = persona?.ParentescoContactoEmergencia,
                IdAspiranteEstatus = aspirante.IdAspiranteEstatus,
                EstadoId = direccion?.CodigoPostal?.Municipio?.EstadoId,
                MunicipioId = direccion?.CodigoPostal?.MunicipioId,
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<AspiranteDto>> Post([FromBody] AspiranteSignupRequest request)
        {
            if (!request.IdPeriodoAcademico.HasValue || request.IdPeriodoAcademico.Value <= 0)
                return BadRequest(new { Error = "El periodo académico es obligatorio para registrar al aspirante." });

            Direccion? direccion = null;

            if (request.Calle != null && request.NumeroExterior != null && request.CodigoPostalId != null)
            {
                direccion = new Direccion
                {
                    Calle = request.Calle,
                    NumeroExterior = request.NumeroExterior,
                    NumeroInterior = request.NumeroInterior,
                    CodigoPostalId = request.CodigoPostalId.Value
                };
            }

            var estatusEnProceso = await _aspiranteService.ObtenerEstatusEnProcesoAsync();
            var idEstatusEnProceso = estatusEnProceso?.IdAspiranteEstatus ?? 2;

            var newAspirante = new Aspirante
            {
                IdPersonaNavigation = new Persona
                {
                    Nombre = request.Nombre,
                    ApellidoPaterno = request.ApellidoPaterno,
                    ApellidoMaterno = request.ApellidoMaterno,
                    FechaNacimiento = request.FechaNacimiento,
                    IdGenero = request.GeneroId,
                    Curp = request.CURP,

                    Correo = request.Correo,
                    Telefono = request.Telefono,
                    Celular = request.Celular,

                    IdDireccionNavigation = direccion,
                    IdEstadoCivil = request.IdEstadoCivil,
                    Nacionalidad = request.Nacionalidad,
                    NombreContactoEmergencia = request.NombreContactoEmergencia,
                    TelefonoContactoEmergencia = request.TelefonoContactoEmergencia,
                    ParentescoContactoEmergencia = request.ParentescoContactoEmergencia
                },
                IdPlan = request.PlanEstudiosId,
                IdMedioContacto = request.MedioContactoId,
                FechaRegistro = DateTime.UtcNow,
                Observaciones = request.Notas,
                TurnoId = request.HorarioId,
                IdAspiranteEstatus = idEstatusEnProceso,
                IdAtendidoPorUsuario = request.AtendidoPorUsuarioId,
                InstitucionProcedencia = request.InstitucionProcedencia,
                IdModalidad = request.IdModalidad,
                GrupoDiasImparticion = request.GrupoDiasImparticion,
                IdPeriodoAcademico = request.IdPeriodoAcademico,
                RecorridoPlantel = request.RecorridoPlantel,
                Trabaja = request.Trabaja,
                NombreEmpresa = request.NombreEmpresa,
                DomicilioEmpresa = request.DomicilioEmpresa,
                PuestoEmpresa = request.PuestoEmpresa,
                QuienCubreGastos = request.QuienCubreGastos,
                IdEmpresa = request.IdEmpresa
            };

            try
            {
                var aspirante = await _aspiranteService.CrearAspirante(newAspirante);

                var aspiranteDto = _mapper.Map<AspiranteDto>(aspirante);

                return CreatedAtAction(nameof(GetById), new { id = aspirante.IdAspirante }, aspiranteDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] AspiranteUpdateRequest request)
        {
            Direccion? direccion = null;

            if (request.Calle != null && request.NumeroExterior != null && request.CodigoPostalId != null)
            {
                direccion = new Direccion
                {
                    Calle = request.Calle,
                    NumeroExterior = request.NumeroExterior,
                    NumeroInterior = request.NumeroInterior,
                    CodigoPostalId = request.CodigoPostalId.Value
                };
            }

            var newAspirante = new Aspirante
            {
                IdAspirante = request.AspiranteId,
                IdPersonaNavigation = new Persona
                {
                    Nombre = request.Nombre,
                    ApellidoPaterno = request.ApellidoPaterno,
                    ApellidoMaterno = request.ApellidoMaterno,
                    FechaNacimiento = request.FechaNacimiento,
                    IdGenero = request.GeneroId,
                    Curp = request.CURP,

                    Correo = request.Correo,
                    Telefono = request.Telefono,
                    Celular = request.Celular,

                    IdDireccionNavigation = direccion,
                    Nacionalidad = request.Nacionalidad,
                    NombreContactoEmergencia = request.NombreContactoEmergencia,
                    TelefonoContactoEmergencia = request.TelefonoContactoEmergencia,
                    ParentescoContactoEmergencia = request.ParentescoContactoEmergencia
                },
                IdPlan = request.PlanEstudiosId,
                IdMedioContacto = request.MedioContactoId,
                FechaRegistro = DateTime.UtcNow,
                Observaciones = request.Notas,
                TurnoId = request.HorarioId,
                IdAspiranteEstatus = request.AspiranteStatusId,
                IdAtendidoPorUsuario = request.AtendidoPorUsuarioId,
                InstitucionProcedencia = request.InstitucionProcedencia,
                IdModalidad = request.IdModalidad,
                GrupoDiasImparticion = request.GrupoDiasImparticion,
                IdPeriodoAcademico = request.IdPeriodoAcademico,
                RecorridoPlantel = request.RecorridoPlantel,
                Trabaja = request.Trabaja,
                NombreEmpresa = request.NombreEmpresa,
                DomicilioEmpresa = request.DomicilioEmpresa,
                PuestoEmpresa = request.PuestoEmpresa,
                QuienCubreGastos = request.QuienCubreGastos,
                IdEmpresa = request.IdEmpresa
            };

            try
            {
                await _aspiranteService.ActualizarAspirante(newAspirante);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("bitacora-seguimiento")]
        public async Task<ActionResult<IEnumerable<AspiranteSeguimientoDto>>> ObtenerBitacoraSeguimiento(int aspiranteId)
        {
            try
            {
                var bitacora = await _aspiranteService.GetBitacoraSeguimiento(aspiranteId);

                if (!bitacora.Any())
                {
                    return NoContent();
                }

                var bitacoraDto = _mapper.Map<IEnumerable<AspiranteSeguimientoDto>>(bitacora);

                foreach (var seguimiento in bitacoraDto)
                {
                    var usuarioAtiende = await _authService.GetUserById(seguimiento.UsuarioAtiendeId);

                    seguimiento.UsuarioAtiendeNombre = ($"{usuarioAtiende.Nombres} {usuarioAtiende.Apellidos}").Trim();
                }

                return Ok(bitacoraDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("bitacora-seguimiento")]
        public async Task<ActionResult<AspiranteSeguimientoDto>> CrearSeguimiento([FromBody] AspiranteSeguimientoRequest request)
        {
            var bitacoraSeguimiento = _mapper.Map<AspiranteBitacoraSeguimiento>(request);

            try
            {
                await _aspiranteService.CrearSeguimiento(bitacoraSeguimiento);

                var seguimientoDto = _mapper.Map<AspiranteSeguimientoDto>(bitacoraSeguimiento);

                return Ok(seguimientoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id:int}/documentos")]
        public async Task<ActionResult<IReadOnlyList<AspiranteDocumentoDto>>> Documentos(int id)
        {
            var list = await _docsSvc.ListarEstadoAsync(new ListarEstadoDocumentosRequest { IdAspirante = id });
            return Ok(list);
        }

        [HttpGet("{id:int}/recibo-inicial")]
        public async Task<ActionResult<IReadOnlyList<ReciboDto>>> ReciboInicial(int id, CancellationToken ct = default)
        {
            var recibos = await _recibosSvc.ListarPorAspiranteAsync(id, ct);
            return Ok(recibos);
        }

        [HttpPost("{id:int}/generar-recibo-inscripcion")]
        public async Task<ActionResult<ReciboDto>> GenerarReciboInscripcion(int id, [FromBody] GenerarReciboAspiranteRequest request, CancellationToken ct = default)
        {
            try
            {
                ReciboDto recibo;
                if (request.IdConceptoPago.HasValue)
                {
                    recibo = await _recibosSvc.GenerarReciboAspiranteConConceptoAsync(id, request.IdConceptoPago.Value, request.DiasVencimiento, ct);
                }
                else
                {
                    recibo = await _recibosSvc.GenerarReciboAspiranteAsync(id, request.Monto, request.Concepto, request.DiasVencimiento, ct);
                }
                return Ok(recibo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("reparar-recibos-sin-detalles")]
        public async Task<ActionResult> RepararRecibosSinDetalles(CancellationToken ct = default)
        {
            try
            {
                var reparados = await _recibosSvc.RepararRecibosSinDetallesAsync(ct);
                return Ok(new { Mensaje = $"Se repararon {reparados} recibos sin detalles", Reparados = reparados });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("recibo/{idRecibo:long}")]
        public async Task<ActionResult> EliminarRecibo(long idRecibo, CancellationToken ct = default)
        {
            try
            {
                await _recibosSvc.EliminarReciboAsync(idRecibo, ct);
                return Ok(new { Mensaje = "Recibo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{id:int}/documentos/requisitos")]
        public async Task<ActionResult<IReadOnlyList<DocumentoRequisitoDto>>> Requisitos(int id)
        {
            var list = await _docsSvc.ListarRequisitosAsync(new ListarRequisitosRequest());
            return Ok(list);
        }

        [HttpGet("documentos/requisitos")]
        public async Task<ActionResult<IReadOnlyList<DocumentoRequisitoDto>>> GetRequisitos()
        {
            var list = await _docsSvc.ListarRequisitosAsync(new ListarRequisitosRequest());
            return Ok(list);
        }

        [HttpPatch("documentos/{idDocumento:long}/validar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR}")]
        public async Task<ActionResult> ValidarDocumento(long idDocumento, [FromBody] ValidarDocumentoRequestDto request)
        {
            request.IdAspiranteDocumento = idDocumento;

            var usuarioId = User.FindFirst("userId")?.Value;
            var (ok, error) = await _docsSvc.ValidarDocumentoAsync(request, usuarioId);

            if (!ok)
                return error == "Documento no encontrado" ? NotFound(error) : BadRequest(new { error });

            return NoContent();
        }

        [HttpPatch("documentos/{idDocumento:long}/estatus")]
        public async Task<ActionResult> CambiarEstatusDocumento(long idDocumento, [FromBody] CambiarEstatusDocumentoDto request)
        {
            var resultado = await _docsSvc.CambiarEstatusDocumentoAsync(idDocumento, request);

            if (!resultado)
                return NotFound("Documento no encontrado");

            return NoContent();
        }

        [HttpPost("documentos/cargar")]
        public async Task<ActionResult> CargarDocumento([FromForm] int idAspirante,
                                                        [FromForm] int idDocumentoRequisito,
                                                        IFormFile archivo,
                                                        [FromForm] string? notas)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debe proporcionar un archivo");

            try
            {
                var docId = await _docsSvc.CargarDocumentoConArchivoAsync(idAspirante, idDocumentoRequisito, archivo, notas);

                return Ok(new { IdAspiranteDocumento = docId, Mensaje = "Documento cargado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("documentos/{idDocumento:long}")]
        public async Task<ActionResult> ResetearDocumento(long idDocumento)
        {
            var resultado = await _docsSvc.ResetearDocumentoAsync(idDocumento);

            if (!resultado)
                return NotFound("Documento no encontrado");

            return Ok(new { Mensaje = "Documento reseteado a estado pendiente" });
        }

        [HttpGet("documentos/{idDocumento:long}")]
        public async Task<ActionResult<AspiranteDocumentoDto>> ObtenerDocumento(long idDocumento)
        {
            var doc = await _docsSvc.ObtenerDocumentoPorIdAsync(idDocumento);

            if (doc == null)
                return NotFound("Documento no encontrado");

            return Ok(doc);
        }

        [HttpPatch("{id:int}/ocultar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult> OcultarAspirante(int id)
        {
            try
            {
                var usuarioId = User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value ?? "SYSTEM";
                var resultado = await _aspiranteService.OcultarAspiranteAsync(id, usuarioId);

                if (!resultado)
                    return NotFound("Aspirante no encontrado");

                return Ok(new { Mensaje = "Aspirante ocultado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("{id:int}/mostrar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult> MostrarAspirante(int id)
        {
            try
            {
                var usuarioId = User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value ?? "SYSTEM";
                var resultado = await _aspiranteService.MostrarAspiranteAsync(id, usuarioId);

                if (!resultado)
                    return NotFound("Aspirante no encontrado");

                return Ok(new { Mensaje = "Aspirante restaurado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("{id:int}/cancelar")]
        public async Task<ActionResult> CancelarAspirante(int id, [FromBody] CancelarAspiranteRequest request)
        {
            try
            {
                var resultado = await _aspiranteService.CancelarAspiranteAsync(id, request.Motivo);

                if (!resultado)
                    return NotFound("Aspirante no encontrado");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("comisiones")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.ADMISIONES}")]
        public async Task<ActionResult<ComisionReporteDto>> CalcularComisiones(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] decimal comisionPorRegistro = 100,
            [FromQuery] decimal porcentajePorPago = 5)
        {
            try
            {
                var desde = fechaDesde ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var hasta = fechaHasta ?? DateTime.UtcNow;

                string? filtrarPorUsuarioId = null;
                if (User.IsInRole(Rol.ADMISIONES) && !User.IsInRole(Rol.ADMIN) && !User.IsInRole(Rol.DIRECTOR) && !User.IsInRole(Rol.FINANZAS))
                {
                    filtrarPorUsuarioId = User.FindFirst("userId")?.Value ?? User.Identity?.Name;
                }

                var resultado = await _aspiranteService.CalcularComisionesAsync(
                    desde, hasta, comisionPorRegistro, porcentajePorPago, filtrarPorUsuarioId);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasAspirantesDto>> ObtenerEstadisticas([FromQuery] int? periodoId)
        {
            var estadisticas = await _aspiranteService.ObtenerEstadisticasAsync(periodoId);
            return Ok(estadisticas);
        }

        [HttpGet("{id:int}/inscripcion-previa")]
        public async Task<ActionResult<Core.DTOs.Aspirante.InscripcionPreviaAspiranteDto>> ObtenerInscripcionPrevia(int id, CancellationToken ct = default)
        {
            try
            {
                var previa = await _aspiranteService.ObtenerInscripcionPreviaAsync(id, ct);
                if (previa == null) return NotFound(new { mensaje = $"Aspirante {id} no encontrado o sin datos suficientes" });
                return Ok(previa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{id:int}/ficha-admision")]
        public async Task<ActionResult<FichaAdmisionDto>> ObtenerFichaAdmision(int id)
        {
            try
            {
                string? usuarioId = User?.Identity?.Name;

                var ficha = await _aspiranteService.ObtenerFichaCompleta(id, usuarioId);

                if (ficha == null)
                    return NotFound($"No se encontró el aspirante con ID {id}");

                return Ok(ficha);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{id:int}/hoja-inscripcion/pdf")]
        public async Task<IActionResult> GenerarHojaInscripcionPdf(int id)
        {
            try
            {
                string? usuarioId = User?.Identity?.Name;

                var ficha = await _aspiranteService.ObtenerFichaCompleta(id, usuarioId);

                if (ficha == null)
                    return NotFound($"No se encontró el aspirante con ID {id}");

                var pdfBytes = _pdfService.GenerarHojaInscripcion(ficha);

                var fileName = $"HojaInscripcion_{ficha.Folio ?? $"ASP-{id:D6}"}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "";
                return StatusCode(500, new { Error = $"Error al generar PDF: {ex.Message}{inner}" });
            }
        }

        [HttpGet("{id:int}/comprobante-inscripcion/pdf")]
        public async Task<IActionResult> GenerarComprobanteInscripcionPdf(int id, CancellationToken ct = default)
        {
            try
            {
                var aspirante = await _dbContext.Aspirante
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdAspirante == id, ct);

                if (aspirante == null || !aspirante.IdPersona.HasValue)
                    return NotFound(new { Error = $"No se encontró el aspirante con ID {id}" });

                var estudiante = await _dbContext.Estudiante
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdPersona == aspirante.IdPersona.Value
                        && e.IdPlanActual == aspirante.IdPlan
                        && e.Status != Core.Enums.StatusEnum.Deleted, ct);

                if (estudiante == null)
                    return NotFound(new { Error = "El aspirante aún no ha sido inscrito como estudiante." });

                var passwordTemporal = $"Usag{estudiante.FechaIngreso:yyyyMMdd}!";

                var pdfBytes = await _comprobanteSvc.GenerarPdfAsync(estudiante.IdEstudiante, passwordTemporal, ct);

                var fileName = $"ComprobanteInscripcion_{estudiante.Matricula}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "";
                return StatusCode(500, new { Error = $"Error al generar PDF: {ex.Message}{inner}" });
            }
        }

        [HttpPost("{id:int}/generar-mensualidades-completas")]
        public async Task<IActionResult> GenerarMensualidadesCompletas(int id, CancellationToken ct = default)
        {
            try
            {
                var resultado = await _aspiranteService.GenerarMensualidadesCompletasAsync(id, ct);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "";
                return StatusCode(500, new { Error = $"Error al generar mensualidades: {ex.Message}{inner}" });
            }
        }

        [HttpPost("{id:int}/recalcular-descuentos-convenio")]
        public async Task<ActionResult<RecalcularDescuentosResultDto>> RecalcularDescuentosConvenio(int id, CancellationToken ct = default)
        {
            try
            {
                var resultado = await _recibosSvc.RecalcularDescuentosConvenioAspiranteAsync(id, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{id:int}/plantilla-disponible")]
        public async Task<ActionResult> PlantillaDisponible(int id, CancellationToken ct = default)
        {
            try
            {
                var plantilla = await _aspiranteService.BuscarPlantillaParaAspiranteAsync(id, ct);
                if (plantilla == null)
                    return NotFound(new { Mensaje = "No se encontró plantilla de cobro para este aspirante" });
                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{id:int}/generar-recibos-plantilla")]
        public async Task<ActionResult<IReadOnlyList<ReciboDto>>> GenerarRecibosDesdeePlantilla(int id, [FromBody] GenerarRecibosPlantillaAspiranteRequest request, CancellationToken ct = default)
        {
            try
            {
                var recibos = await _aspiranteService.GenerarRecibosDesdeePlantillaParaAspiranteAsync(
                    id, request.IdPlantillaCobro, request.EliminarPendientesExistentes, ct);
                return Ok(recibos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("documentos/{id:long}/toggle-recibido")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.ADMISIONES}")]
        public async Task<ActionResult> ToggleRecibido(long id)
        {
            try
            {
                var usuarioId = User.FindFirst("userId")?.Value;
                var ok = await _docsSvc.ToggleEntregadoAsync(id, usuarioId);
                if (!ok) return NotFound(new { error = "Documento no encontrado" });

                var doc = await _dbContext.AspiranteDocumento.FirstOrDefaultAsync(d => d.IdAspiranteDocumento == id);
                return Ok(new { entregado = doc?.Entregado ?? false, recibido = doc?.Entregado ?? false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("documentos/{id:long}/prorroga")]
        public async Task<ActionResult> AsignarProrroga(long id, [FromBody] AsignarProrrogaRequest request)
        {
            try
            {
                var userId = User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value ?? "System";
                await _docsSvc.AsignarProrrogaAsync(id, request.FechaProrroga, request.Motivo ?? "", userId);
                return Ok(new { message = "Prorroga asignada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{idAspirante:int}/documentos/prorroga-global")]
        public async Task<ActionResult> AsignarProrrogaGlobal(int idAspirante, [FromBody] ProrrogaGlobalRequest request)
        {
            try
            {
                var userId = User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value ?? "System";
                await _docsSvc.AsignarProrrogaGlobalAsync(idAspirante, request.FechaProrroga, request.Motivo ?? "", userId);
                return Ok(new { message = "Prorroga global asignada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("documentacion/panel")]
        public async Task<ActionResult<List<DocumentacionAspiranteResumenDto>>> GetPanelDocumentacion(
            [FromQuery] string? estatus = null,
            [FromQuery] string? busqueda = null,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _docsSvc.GetResumenDocumentacionAsync(estatus, busqueda, ct);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/inscribir-como-estudiante")]
        public async Task<ActionResult<InscripcionAspiranteResultDto>> InscribirComoEstudiante(
            int id,
            [FromBody] InscribirAspiranteRequest request)
        {
            try
            {
                string? usuarioId = User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value;

                Console.WriteLine($"=== INSCRIBIR ASPIRANTE {id} ===");
                Console.WriteLine($"Usuario que procesa: {usuarioId ?? "NO IDENTIFICADO"}");

                var resultado = await _aspiranteService.InscribirAspiranteComoEstudianteAsync(id, request, usuarioId);

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Error al inscribir aspirante: {ex.Message}" });
            }
        }

    }
}
